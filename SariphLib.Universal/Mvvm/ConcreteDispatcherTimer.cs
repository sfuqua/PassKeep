// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.UI.Xaml;

namespace SariphLib.Mvvm
{
    /// <summary>
    /// An ITimer implemented by a DispatcherTimer.
    /// </summary>
    public class ConcreteDispatcherTimer : ITimer
    {
        private DispatcherTimer thisTimer;

        public ConcreteDispatcherTimer()
        {
            thisTimer = new DispatcherTimer();
            thisTimer.Tick += (s, e) =>
            {
                if (Tick != null)
                {
                    Tick(this, e);
                }
            };
        }

        /// <summary>
        /// The time between timer ticks.
        /// </summary>
        public TimeSpan Interval
        {
            get { return thisTimer.Interval;  }
            set { thisTimer.Interval = value; }
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
            thisTimer.Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            thisTimer.Stop();
        }
    }
}
