// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="Scheduler.cs" company="Guilherme Branco Stracini ME">
//     © 2020 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace CrispyWaffle.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class Scheduler.
    /// Implements the <see cref="CrispyWaffle.Scheduler.IScheduler" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Scheduler.IScheduler" />
    public class CronScheduler : IScheduler
    {
        #region Private fields

        /// <summary>
        /// The expression
        /// </summary>
        private readonly string _expression;

        /// <summary>
        /// The days of week
        /// </summary>
        private readonly HashSet<int> _daysOfWeek = new HashSet<int>();
        /// <summary>
        /// The months
        /// </summary>
        private readonly HashSet<int> _months = new HashSet<int>();
        /// <summary>
        /// The days of month
        /// </summary>
        private readonly HashSet<int> _daysOfMonth = new HashSet<int>();
        /// <summary>
        /// The hours
        /// </summary>
        private readonly HashSet<int> _hours = new HashSet<int>();
        /// <summary>
        /// The minutes
        /// </summary>
        private readonly HashSet<int> _minutes = new HashSet<int>();

        #endregion

        #region ~Ctors

        /// <summary>
        /// Initializes a new instance of the <see cref="CronScheduler"/> class.
        /// </summary>
        public CronScheduler() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CronScheduler" /> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public CronScheduler(string expression)
        {
            if (!IsValid(expression))
            {
                throw new ArgumentOutOfRangeException(nameof(expression), expression, "The expression isn't a valid CRON expression");
            }

            _expression = expression;

            GenerateData();

        }

        #endregion

        #region Private methods

        /// <summary>
        /// Generates the data.
        /// </summary>
        private void GenerateData()
        {
            var matches = CronSchedulerValidations.ValidationRegex.Matches(_expression);

            GenerateData(matches[0].ToString(), 0, 60).ForEach(i => _minutes.Add(i));
            GenerateData(matches.Count > 1 ? matches[1].ToString() : "*", 0, 24).ForEach(i => _hours.Add(i));
            GenerateData(matches.Count > 2 ? matches[2].ToString() : "*", 1, 32).ForEach(i => _daysOfMonth.Add(i));
            GenerateData(matches.Count > 3 ? matches[3].ToString() : "*", 1, 13).ForEach(i => _months.Add(i));
            GenerateData(matches.Count > 4 ? matches[4].ToString() : "*", 0, 7).ForEach(i => _daysOfWeek.Add(i));
        }

        /// <summary>
        /// Generates the data.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="start">The start.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        private static List<int> GenerateData(string value, int start, int max)
        {
            if (CronSchedulerValidations.DividedRegex.IsMatch(value))
            {
                return GenerateDivided(value, start, max);
            }

            if (CronSchedulerValidations.RangeRegex.IsMatch(value))
            {
                return GenerateRange(value);
            }

            if (CronSchedulerValidations.WildRegex.IsMatch(value))
            {
                return GenerateWild(start, max);
            }

            return CronSchedulerValidations.ListRegex.IsMatch(value) ? GenerateList(value) : new List<int>();
        }


        /// <summary>
        /// Generates the divided.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="start">The start.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        private static List<int> GenerateDivided(string value, int start, int max)
        {
            var result = new List<int>();

            var split = value.Split('/');
            var divisor = int.Parse(split[1]);

            for (var i = start; i < max; i++)
            {
                if (i % divisor == 0)
                {
                    result.Add(i);
                }
            }

            return result;
        }

        /// <summary>
        /// Generates the range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        private static List<int> GenerateRange(string value)
        {
            var result = new List<int>();

            var split = value.Split('-');
            var start = int.Parse(split[0]);
            int end;

            if (split[1].Contains("/"))
            {
                split = split[1].Split('/');
                end = int.Parse(split[0]);
                var divisor = int.Parse(split[1]);

                for (var i = start; i <= end; i++)
                {
                    if (i % divisor == 0)
                    {
                        result.Add(i);
                    }
                }

                return result;
            }

            end = int.Parse(split[1]);

            for (var i = start; i <= end; i++)
            {
                result.Add(i);
            }

            return result;
        }

        /// <summary>
        /// Generates the wild.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        private static List<int> GenerateWild(int start, int max) => Enumerable.Range(start, max).ToList();


        /// <summary>
        /// Generates the list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
        private static List<int> GenerateList(string value) => value.Split(',').Select(int.Parse).ToList();


        #endregion

        #region Implementation of IScheduler

        /// <summary>
        /// Gets the days of week.
        /// </summary>
        /// <value>The days of week.</value>
        public ICollection<int> DaysOfWeek => _daysOfWeek;

        /// <summary>
        /// Gets the months.
        /// </summary>
        /// <value>The months.</value>
        public ICollection<int> Months => _months;

        /// <summary>
        /// Gets the days of month.
        /// </summary>
        /// <value>The days of month.</value>
        public ICollection<int> DaysOfMonth => _daysOfMonth;

        /// <summary>
        /// Gets the hours.
        /// </summary>
        /// <value>The hours.</value>
        public ICollection<int> Hours => _hours;

        /// <summary>
        /// Gets the minutes.
        /// </summary>
        /// <value>The minutes.</value>
        public ICollection<int> Minutes => _minutes;

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns><c>true</c> if the specified expression is valid; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsValid(string expression) => CronSchedulerValidations.ValidationRegex.Matches(expression).Count > 0;

        /// <summary>
        /// Determines whether the specified date time is time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns><c>true</c> if the specified date time is time; otherwise, <c>false</c>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsTime(DateTime dateTime)
        {
            return _minutes.Contains(dateTime.Minute) &&
                   _hours.Contains(dateTime.Hour) &&
                   _daysOfMonth.Contains(dateTime.Day) &&
                   _months.Contains(dateTime.Month) &&
                   _daysOfWeek.Contains((int)dateTime.DayOfWeek);
        }

        #endregion
    }
}
