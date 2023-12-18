// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-04-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-04-2020
// ***********************************************************************
// <copyright file="MustacheTemplateRender.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CrispyWaffle.Extensions;

namespace CrispyWaffle.TemplateRendering.Engines
{
    /// <summary>
    /// A simple template processing system.
    /// </summary>
    /// <seealso cref="ITemplateRender" />
    public class MustacheTemplateRender : ITemplateRender
    {
        /// <summary>
        /// The properties
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
                    property => property.Name.ToLower(),
                    property => property.GetValue(data, null)
                );
        }

        /// <summary>
        /// Evaluates the conditional.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>String.</returns>
        private string EvaluateConditional(string template)
        {
            return !MustachePatterns.ConditionalPattern.IsMatch(template)
                ? template
                : MustachePatterns
                    .ConditionalPattern.Matches(template)
                    // ReSharper disable once RedundantEnumerableCastCall
                    .Cast<Match>()
                    .Aggregate(template, (current, match) => EvaluateConditional(match, current));
        }

        /// <summary>
        /// Evaluates the conditional.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <param name="template">The template.</param>
        /// <returns>String.</returns>
        private string EvaluateConditional(Match match, string template)
        {
            var condition = match.Groups["condition"].Value.ToLower();
            var matchText = match.ToString();

            var value = IsPropertyInDictionary(condition)
                ? match.Groups["innerContent"].Value
                : match.Groups["elseInnerContent"].Success
                    ? match.Groups["elseInnerContent"].Value
                    : string.Empty;

            return template.Replace(matchText, value);
        }

        /// <summary>
        /// Determines whether specified property is in dictionary
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

            return typeof(string) == type
                ? !string.IsNullOrWhiteSpace((string)value)
                : type.IsSimpleType()
                    || type.GetConstructors().Any(c => c.GetParameters().Length == 0)
                        && Convert.ChangeType(value, type, CultureInfo.InvariantCulture)
                            != Activator.CreateInstance(type);
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
                    // ReSharper disable once RedundantEnumerableCastCall
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
            var withProperty = match.Groups["property"].Value.ToLower();
            var with = match.Groups["innerContent"].Value;
            if (!_properties.ContainsKey(withProperty))
            {
                return template.Replace(match.Value, string.Empty);
            }

            var property = _properties[withProperty];
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
                    // ReSharper disable once RedundantEnumerableCastCall
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
            var loopProperty = match.Groups["property"].Value.ToLower();
            var pattern = match.Groups["innerContent"].Value;
            if (!_properties.ContainsKey(loopProperty))
            {
                return template.Replace(match.Value, string.Empty);
            }

            var replacements = _properties[loopProperty];
            var type = replacements.GetType();
            if (!type.IsArray)
            {
                return template.Replace(match.Value, string.Empty);
            }

            var subType = type.GetElementType();
            return type.IsSimpleType() || subType.IsSimpleType()
                ? ProcessSimpleLoop(match, template, pattern, (string[])replacements)
                : ProcessComplexLoop(match, template, pattern, (object[])replacements);
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
                    current + MustachePatterns.LoopPropertyPattern.Replace(pattern, value)
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
            object[] replacements
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
            var property = match.Groups["property"].Value.ToLower();
            return properties.TryGetValue(property, out var property1)
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
        /// Render a template using the object data as the values/condition checks
        /// </summary>
        /// <param name="template">The template to be rendered</param>
        /// <param name="data">The properties to replace in template</param>
        /// <returns>The template with conditions resolved and the properties replaced with value</returns>
        /// <exception cref="ArgumentNullException">data</exception>
        public string Render(string template, object data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            _properties = ParseObject(data);
            return RenderData(ProcessLoop(ProcessWith(EvaluateConditional(template))));
        }
    }
}
