// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Providers;
using SariphLib.Mvvm;
using System;

namespace PassKeep.Lib.Providers
{
    public class ThreadPoolTimerFactory : ITimerFactory
    {
        public ITimer Assemble()
        {
            return new ConcreteThreadPoolTimer();
        }

        /// <summary>
        /// Creates a new timer.
        /// </summary>
        /// <param name="interval">Interval to use for the new timer.</param>
        /// <returns></returns>
        public ITimer Assemble(TimeSpan interval)
        {
            return new ConcreteThreadPoolTimer
            {
                Interval = interval
            };
        }
    }
}
