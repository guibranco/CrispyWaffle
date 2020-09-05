// ***********************************************************************
// Assembly         : CrispyWaffle
// Author           : Guilherme Branco Stracini
// Created          : 09-05-2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 09-05-2020
// ***********************************************************************
// <copyright file="IJobRunner.cs" company="Guilherme Branco Stracini ME">
//     © 2020 Guilherme Branco Stracini. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;

namespace CrispyWaffle.Scheduler
{
    /// <summary>
    /// Interface IJobRunner
    /// </summary>
    public interface IJobRunner
    {
        /// <summary>
        /// Executes the specified date time.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        void Execute(DateTime dateTime);
    }
}