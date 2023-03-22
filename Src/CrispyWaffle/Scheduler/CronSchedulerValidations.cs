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
namespace CrispyWaffle.Scheduler
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Class CronSchedulerValidations.
    /// </summary>
    public static class CronSchedulerValidations
    {
        /// <summary>
        /// The divided regex
        /// </summary>
        public static readonly Regex DividedRegex = new Regex(@"(\*/\d+)", RegexOptions.Compiled);

        /// <summary>
        /// The range regex
        /// </summary>
        public static readonly Regex RangeRegex = new Regex(@"(\d+\-\d+)\/?(\d+)?", RegexOptions.Compiled);

        /// <summary>
        /// The wild regex
        /// </summary>
        public static readonly Regex WildRegex = new Regex(@"(\*)", RegexOptions.Compiled);

        /// <summary>
        /// The list regex
        /// </summary>
        public static readonly Regex ListRegex = new Regex(@"(((\d+,)*\d+)+)", RegexOptions.Compiled);

        /// <summary>
        /// The validation regex
        /// </summary>
        public static readonly Regex ValidationRegex = new Regex(DividedRegex + "|" + RangeRegex + "|" + WildRegex + "|" + ListRegex, RegexOptions.Compiled);
    }
}
