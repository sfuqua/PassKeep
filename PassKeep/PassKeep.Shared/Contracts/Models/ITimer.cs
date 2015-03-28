using System;

namespace PassKeep.Contracts.Models
{
    /// <summary>
    /// Represents an object that ticks at regular intervals.
    /// </summary>
    public interface ITimer
    {
        /// <summary>
        /// The time between timer ticks.
        /// </summary>
        TimeSpan Interval
        {
            get;
            set;
        }

        /// <summary>
        /// Fires whenever the timer ticks.
        /// </summary>
        event EventHandler<object> Tick;

        /// <summary>
        /// Starts the timer.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the timer.
        /// </summary>
        void Stop();
    }
}
