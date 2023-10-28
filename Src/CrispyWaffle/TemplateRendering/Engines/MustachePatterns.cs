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

using System;
using System.Text.RegularExpressions;

namespace CrispyWaffle.TemplateRendering.Engines
{
    /// <summary>
    /// Class MustachePatterns.
    /// </summary>
    internal static class MustachePatterns
    {
        /// <summary>
        /// The loop pattern
        /// </summary>
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
        /// The with pattern
        /// </summary>
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
        /// The conditional pattern
        /// </summary>
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
        /// The property pattern
        /// </summary>
        public static readonly Regex PropertyPattern =
            new(
                "{{(?<property>.+?)}}",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(5)
            );

        /// <summary>
        /// The loop property pattern
        /// </summary>
        public static readonly Regex LoopPropertyPattern =
            new(
                "{{this}}",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(5)
            );

        /// <summary>
        /// The import pattern
        /// </summary>
        public static readonly Regex ImportPattern =
            new(
                "{{>import (?<kvp>(?<key>file|type)=\"(?<value>.+?)\"\\s?){2}}}",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(5)
            );
    }
}
