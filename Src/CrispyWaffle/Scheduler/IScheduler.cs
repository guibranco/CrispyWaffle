// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="IScheduler.cs" company="Guilherme Branco Stracini ME">
//     © 2023 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;

namespace CrispyWaffle.Scheduler
{
    /// <summary>
    /// Interface IScheduler
    /// </summary>
    public interface IScheduler
    {
        /// <summary>
        /// Returns true if the expression is valid.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns><c>true</c> if the specified expression is valid; otherwise, <c>false</c>.</returns>
        bool IsValid(string expression);

        /// <summary>
        /// Determines whether the specified date time is time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns><c>true</c> if the specified date time is time; otherwise, <c>false</c>.</returns>
        bool IsTime(DateTime dateTime);

        /// <summary>
        /// Gets the days of week.
        /// </summary>
        /// <value>The days of week.</value>
        ICollection<int> DaysOfWeek { get; }

        /// <summary>
        /// Gets the months.
        /// </summary>
        /// <value>The months.</value>
        ICollection<int> Months { get; }

        /// <summary>
        /// Gets the days of month.
        /// </summary>
        /// <value>The days of month.</value>
        ICollection<int> DaysOfMonth { get; }

        /// <summary>
        /// Gets the hours.
        /// </summary>
        /// <value>The hours.</value>
        ICollection<int> Hours { get; }

        /// <summary>
        /// Gets the minutes.
        /// </summary>
        /// <value>The minutes.</value>
        ICollection<int> Minutes { get; }
    }
}
