// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.System.Threading;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// An ITimer implemented by ThreadPoolTimer
    /// </summary>
    public class ConcreteThreadPoolTimer : ITimer
    {
        private TimeSpan interval;
        private ThreadPoolTimer thisTimer;

        /// <summary>
        /// The time between timer ticks.
        /// </summary>
        public TimeSpan Interval
        {
            get { return this.interval; }
            set
            {
                if (this.interval != value)
                {
                    this.interval = value;
                    if (thisTimer != null)
                    {
                        Stop();
                        Start();
                    }
                }
            }
        }

        /// <summary>
        /// Fires whenever the timer ticks.
        /// </summary>
        public event EventHandler<object> Tick;

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            if (thisTimer == null)
            {
                thisTimer = ThreadPoolTimer.CreatePeriodicTimer(ThreadPoolTimerElapsed, Interval);
            }
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            if (thisTimer != null)
            {
                thisTimer.Cancel();
                thisTimer = null;
            }
        }

        /// <summary>
        /// Marshals the ThreadPoolTimer's shots to the interface's <see cref="Tick"/> event.
        /// </summary>
        /// <param name="timer"></param>
        private void ThreadPoolTimerElapsed(ThreadPoolTimer timer)
        {
            Tick?.Invoke(this, timer);
        }
    }
}
