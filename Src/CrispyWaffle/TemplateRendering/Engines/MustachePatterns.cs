// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-04-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 07-06-2020
// ***********************************************************************
// <copyright file="MustachePatterns.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace CrispyWaffle.TemplateRendering.Engines
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class MustachePatterns.
    /// </summary>
    internal static class MustachePatterns
    {
        /// <summary>
        /// The loop pattern
        /// </summary>
        public static readonly Regex LoopPattern = new Regex(
            @"{{\#each (?<property>.+?)}}(?<innerContent>.+?){{/each}}",
            RegexOptions.Compiled
                | RegexOptions.CultureInvariant
                | RegexOptions.IgnoreCase
                | RegexOptions.Singleline
        );

        /// <summary>
        /// The with pattern
        /// </summary>
        public static readonly Regex WithPattern = new Regex(
            @"{{\#with (?<property>.+?)}}(?<innerContent>.+?){{/with}}",
            RegexOptions.Compiled
                | RegexOptions.CultureInvariant
                | RegexOptions.IgnoreCase
                | RegexOptions.Singleline
        );

        /// <summary>
        /// The conditional pattern
        /// </summary>
        public static readonly Regex ConditionalPattern = new Regex(
            @"{{\#(?<condition>.+?)}}(?<innerContent>.+?)(?:{{\#else}}(?<elseInnerContent>.+?))?{{/\1}}",
            RegexOptions.Compiled
                | RegexOptions.CultureInvariant
                | RegexOptions.IgnoreCase
                | RegexOptions.Singleline
        );

        /// <summary>
        /// The property pattern
        /// </summary>
        public static readonly Regex PropertyPattern = new Regex(
            "{{(?<property>.+?)}}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// The loop property pattern
        /// </summary>
        public static readonly Regex LoopPropertyPattern = new Regex(
            "{{this}}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// The import pattern
        /// </summary>
        public static readonly Regex ImportPattern = new Regex(
            "{{>import (?<kvp>(?<key>file|type)=\"(?<value>.+?)\"\\s?){2}}}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
        );
    }
}
