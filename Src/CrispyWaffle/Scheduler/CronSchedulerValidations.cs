// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="CronSchedulerValidations.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CrispyWaffle.Scheduler
{
    /// <summary>
    /// Class CronSchedulerValidations.
    /// </summary>
    public static class CronSchedulerValidations
    {
        /// <summary>
        /// The divided regex
        /// </summary>
        public static readonly Regex DividedRegex =
            new(@"(\*/\d+)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The range regex
        /// </summary>
        public static readonly Regex RangeRegex =
            new(@"(\d+\-\d+)\/?(\d+)?", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The wild regex
        /// </summary>
        public static readonly Regex WildRegex =
            new(@"(\*)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The list regex
        /// </summary>
        public static readonly Regex ListRegex =
            new(@"(((\d+,)*\d+)+)", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

        /// <summary>
        /// The validation regex
        /// </summary>
        [SuppressMessage("ReSharper", "ComplexConditionExpression")]
        public static readonly Regex ValidationRegex =
            new(
                DividedRegex + "|" + RangeRegex + "|" + WildRegex + "|" + ListRegex,
                RegexOptions.Compiled,
                TimeSpan.FromSeconds(5)
            );
    }
}
