using System;
using System.Text.RegularExpressions;

namespace CrispyWaffle.TemplateRendering.Engines;

/// <summary>
/// Contains regular expression patterns for parsing and processing Mustache-style template syntax.
/// These patterns are used to match common Mustache constructs such as loops, conditionals, properties, and imports.
/// </summary>
internal static class MustachePatterns
{
    /// <summary>
    /// A regular expression pattern that matches a loop construct (e.g., {{#each property}}...{{/each}}) in a Mustache template.
    /// </summary>
    /// <remarks>
    /// This pattern captures the property being iterated over and the content inside the loop.
    /// </remarks>
    public static readonly Regex LoopPattern =
        new(
            @"{{\#each (?<property>.+?)}}(?<innerContent>.+?){{/each}}",
            RegexOptions.Compiled
                | RegexOptions.CultureInvariant
                | RegexOptions.IgnoreCase
                | RegexOptions.Singleline,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// A regular expression pattern that matches a "with" construct (e.g., {{#with property}}...{{/with}}) in a Mustache template.
    /// </summary>
    /// <remarks>
    /// This pattern captures the property being used and the content inside the "with" block.
    /// </remarks>
    public static readonly Regex WithPattern =
        new(
            @"{{\#with (?<property>.+?)}}(?<innerContent>.+?){{/with}}",
            RegexOptions.Compiled
                | RegexOptions.CultureInvariant
                | RegexOptions.IgnoreCase
                | RegexOptions.Singleline,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// A regular expression pattern that matches a conditional block (e.g., {{#condition}}...{{/condition}}) in a Mustache template.
    /// Optionally matches an "else" block (e.g., {{#else}}...{{/else}}).
    /// </summary>
    /// <remarks>
    /// This pattern captures the condition being evaluated, the content for the "true" case, and optionally, the content for the "else" case.
    /// </remarks>
    public static readonly Regex ConditionalPattern =
        new(
            @"{{\#(?<condition>.+?)}}(?<innerContent>.+?)(?:{{\#else}}(?<elseInnerContent>.+?))?{{/\1}}",
            RegexOptions.Compiled
                | RegexOptions.CultureInvariant
                | RegexOptions.IgnoreCase
                | RegexOptions.Singleline,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// A regular expression pattern that matches a simple property reference (e.g., {{property}}) in a Mustache template.
    /// </summary>
    /// <remarks>
    /// This pattern captures the name of the property to be replaced in the template.
    /// </remarks>
    public static readonly Regex PropertyPattern =
        new(
            "{{(?<property>.+?)}}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// A regular expression pattern that matches the "this" reference (e.g., {{this}}) in a Mustache template.
    /// This is used within loops to refer to the current item being iterated over.
    /// </summary>
    /// <remarks>
    /// This pattern is used specifically in loops to refer to the current item in context.
    /// </remarks>
    public static readonly Regex LoopPropertyPattern =
        new(
            "{{this}}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );

    /// <summary>
    /// A regular expression pattern that matches an import statement (e.g., {{>import file="filename"}}) in a Mustache template.
    /// </summary>
    /// <remarks>
    /// This pattern captures key-value pairs in the import statement, specifically looking for "file" or "type" attributes.
    /// </remarks>
    public static readonly Regex ImportPattern =
        new(
            "{{>import (?<kvp>(?<key>file|type)=\"(?<value>.+?)\"\\s?){2}}}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
            TimeSpan.FromSeconds(5)
        );
}
