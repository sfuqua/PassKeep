// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Mvvm;
using System;

namespace PassKeep.Lib.Contracts.Providers
{
    public interface ITimerFactory
    {
        /// <summary>
        /// Creates a new timer.
        /// </summary>
        /// <returns></returns>
        ITimer Assemble();

        /// <summary>
        /// Creates a new timer.
        /// </summary>
        /// <param name="interval">Interval to use for the new timer.</param>
        /// <returns></returns>
        ITimer Assemble(TimeSpan interval);
    }
}
