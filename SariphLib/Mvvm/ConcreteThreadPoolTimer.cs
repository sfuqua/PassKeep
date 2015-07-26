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
                thisTimer = ThreadPoolTimer.CreatePeriodicTimer(ThreadPoolTimerElapsed, this.Interval);
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
