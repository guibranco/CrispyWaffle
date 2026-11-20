using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CrispyWaffle.Extensions;

namespace CrispyWaffle.TemplateRendering.Engines;

/// <summary>
/// A simple template processing system.
/// </summary>
/// <seealso cref="ITemplateRender" />
public class MustacheTemplateRender : ITemplateRender
{
    /// <summary>
    /// The properties.
    /// </summary>
    private Dictionary<string, object> _properties;

    /// <summary>
    /// Parses the object.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>Dictionary&lt;String, Object&gt;.</returns>
    private static Dictionary<string, object> ParseObject(object data)
    {
        return data.GetType()
            .GetProperties()
            .ToDictionary(
                property => property.Name.ToLowerInvariant(),
                property => property.GetValue(data, null)
            );
    }

    private static readonly Regex InnermostConditionalPattern = new(
        @"{{\#(?<condition>(?!else}}).+?)}}(?<innerContent>(?:(?!{{\#(?!else}})).)+?)(?:{{\#else}}(?<elseInnerContent>(?:(?!{{\#(?!else}})).)+?))?{{/\1}}",
        RegexOptions.Compiled
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnoreCase
            | RegexOptions.Singleline,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// Evaluates the conditional.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns>String.</returns>
    private string EvaluateConditional(string template)
    {
        while (InnermostConditionalPattern.IsMatch(template))
        {
            template = InnermostConditionalPattern
                .Matches(template)
                .Cast<Match>()
                .Aggregate(template, (current, match) => EvaluateConditional(match, current));
        }

        return template;
    }

    /// <summary>
    /// Evaluates the conditional.
    /// </summary>
    /// <param name="match">The match.</param>
    /// <param name="template">The template.</param>
    /// <returns>String.</returns>
    private string EvaluateConditional(Match match, string template)
    {
        var condition = match.Groups["condition"].Value.ToLowerInvariant();
        var matchText = match.ToString();

        string value;

        if (IsPropertyInDictionary(condition))
        {
            value = match.Groups["innerContent"].Value;
        }
        else
        {
            value = match.Groups["elseInnerContent"].Success
                ? match.Groups["elseInnerContent"].Value
                : string.Empty;
        }

        return template.Replace(matchText, value);
    }

    /// <summary>
    /// Determines whether specified property is in dictionary.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>Boolean.</returns>
    private bool IsPropertyInDictionary(string propertyName)
    {
        if (!_properties.ContainsKey(propertyName))
        {
            return false;
        }

        if (_properties[propertyName] == null)
        {
            return false;
        }

        var value = _properties[propertyName];
        var type = value.GetType();
        if (typeof(bool) == type)
        {
            return (bool)value;
        }

        if (value is string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        if (type.IsSimpleType())
        {
            return true;
        }

        if (value is IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Any();
        }

        return true;
    }

    /// <summary>
    /// Processes the with.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns>String.</returns>
    private string ProcessWith(string template)
    {
        return !MustachePatterns.WithPattern.IsMatch(template)
            ? template
            : MustachePatterns
                .WithPattern.Matches(template)
                .Cast<Match>()
                .Aggregate(template, (current, match) => ProcessWith(match, current));
    }

    /// <summary>
    /// Processes the with.
    /// </summary>
    /// <param name="match">The match.</param>
    /// <param name="template">The template.</param>
    /// <returns>String.</returns>
    private string ProcessWith(Match match, string template)
    {
        var withProperty = match.Groups["property"].Value.ToLowerInvariant();
        var with = match.Groups["innerContent"].Value;
        if (!_properties.TryGetValue(withProperty, out var property) || property == null)
        {
            return template.Replace(match.Value, string.Empty);
        }

        if (property.GetType().IsSimpleType())
        {
            return template.Replace(match.Value, string.Empty);
        }

        var renderer = new MustacheTemplateRender();
        var result = renderer.Render(with, property);
        return template.Replace(match.Value, result);
    }

    /// <summary>
    /// Processes the loop.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns>String.</returns>
    private string ProcessLoop(string template)
    {
        return !MustachePatterns.LoopPattern.IsMatch(template)
            ? template
            : MustachePatterns
                .LoopPattern.Matches(template)
                .Cast<Match>()
                .Aggregate(template, (current, match) => ProcessLoop(match, current));
    }

    /// <summary>
    /// Processes the loop.
    /// </summary>
    /// <param name="match">The match.</param>
    /// <param name="template">The template.</param>
    /// <returns>String.</returns>
    private string ProcessLoop(Match match, string template)
    {
        var loopProperty = match.Groups["property"].Value.ToLowerInvariant();
        var pattern = match.Groups["innerContent"].Value;
        if (
            !_properties.TryGetValue(loopProperty, out var replacements)
            || replacements is null or string
            || replacements is not IEnumerable enumerable
        )
        {
            return template.Replace(match.Value, string.Empty);
        }

        var items = enumerable.Cast<object>().ToList();
        if (items.Count == 0)
        {
            return template.Replace(match.Value, string.Empty);
        }

        var type = replacements.GetType();
        var subType = type.IsArray
            ? type.GetElementType()
            : type.IsGenericType
                ? type.GetGenericArguments().FirstOrDefault()
                : null;
        var isSimple = subType != null
            ? subType.IsSimpleType()
            : items.All(item => item == null || item.GetType().IsSimpleType());

        return isSimple
            ? ProcessSimpleLoop(
                match,
                template,
                pattern,
                items.Select(item => item?.ToString() ?? string.Empty)
            )
            : ProcessComplexLoop(match, template, pattern, items);
    }

    /// <summary>
    /// Processes the simple loop.
    /// </summary>
    /// <param name="match">The match.</param>
    /// <param name="template">The template.</param>
    /// <param name="pattern">The pattern.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>String.</returns>
    private static string ProcessSimpleLoop(
        Capture match,
        string template,
        string pattern,
        IEnumerable<string> replacements
    )
    {
        var result = string.Empty;
        result = replacements.Aggregate(
            result,
            (current, value) =>
                current + MustachePatterns.LoopPropertyPattern.Replace(pattern, _ => value)
        );
        return template.Replace(match.Value, result);
    }

    /// <summary>
    /// Processes the complex loop.
    /// </summary>
    /// <param name="match">The match.</param>
    /// <param name="template">The template.</param>
    /// <param name="pattern">The pattern.</param>
    /// <param name="replacements">The replacements.</param>
    /// <returns>String.</returns>
    private static string ProcessComplexLoop(
        Capture match,
        string template,
        string pattern,
        IEnumerable<object> replacements
    )
    {
        var result = string.Empty;
        result = replacements.Aggregate(
            result,
            (current, value) =>
            {
                var properties = ParseObject(value);
                return current
                    + MustachePatterns.PropertyPattern.Replace(
                        pattern,
                        patch => RenderProperties(patch, properties)
                    );
            }
        );
        return template.Replace(match.Value, result);
    }

    /// <summary>
    /// Renders the properties.
    /// </summary>
    /// <param name="match">The match.</param>
    /// <param name="properties">The properties.</param>
    /// <returns>String.</returns>
    private static string RenderProperties(Match match, IDictionary<string, object> properties)
    {
        var property = match.Groups["property"].Value.ToLowerInvariant();
        return properties.TryGetValue(property, out var property1) && property1 != null
            ? property1.ToString()
            : string.Empty;
    }

    /// <summary>
    /// Renders the data.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <returns>String.</returns>
    private string RenderData(string template)
    {
        if (!MustachePatterns.PropertyPattern.IsMatch(template))
        {
            return template;
        }

        template = MustachePatterns.PropertyPattern.Replace(
            template,
            match => RenderProperties(match, _properties)
        );
        return template;
    }

    /// <summary>
    /// Render a template using the object data as the values/condition checks.
    /// </summary>
    /// <param name="template">The template to be rendered.</param>
    /// <param name="data">The properties to replace in template.</param>
    /// <returns>The template with conditions resolved and the properties replaced with value.</returns>
    /// <exception cref="ArgumentNullException">data.</exception>
    public string Render(string template, object data)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(data);
#else
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }
#endif

        _properties = ParseObject(data);
        return RenderData(ProcessLoop(ProcessWith(EvaluateConditional(template))));
    }
}
