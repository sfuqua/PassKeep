﻿// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SariphLib.Testing
{
    /// <summary>
    /// Private test implementation of ITimer since DispatcherTimer is
    /// hard to test as-is. This implementation uses Tasks.
    /// </summary>
    public class Timer : ITimer
    {
        private CancellationTokenSource cts;

        /// <summary>
        /// Constructs the timer with a default interval of one second.
        /// </summary>
        public Timer()
        {
            Interval = new TimeSpan(0, 0, 1);
        }

        /// <summary>
        /// The time between timer ticks.
        /// </summary>
        public TimeSpan Interval
        {
            get;
            set;
        }

        /// <summary>
        /// Fires whenever the timer ticks.
        /// </summary>
        public event EventHandler<object> Tick;

        /// <summary>
        /// Fires the Tick event.
        /// </summary>
        private void OnTick()
        {
            if (Tick != null)
            {
                Tick(this, null);
            }
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        public void Start()
        {
            this.cts = new CancellationTokenSource();
            Task.Run(async () => await TimerLoop());
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            if (this.cts != null)
            {
                this.cts.Cancel();
            }
        }

        /// <summary>
        /// Ticks continually until stopped.
        /// </summary>
        /// <returns>A Task representing the work.</returns>
        private async Task TimerLoop()
        {
            while(!this.cts.IsCancellationRequested)
            {
                await Task.Delay(Interval, this.cts.Token);
                OnTick();
            }
        }
    }
}
