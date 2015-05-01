using PassKeep.Lib.Contracts.Enums;
using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// Represents arguments for an event that should fire when a clipboard clear timer terminates.
    /// </summary>
    public class ClipboardTimerCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes the EventArgs instance.
        /// </summary>
        /// <param name="timerType">The type of timer (e.g., username, password) associated with this event.</param>
        public ClipboardTimerCompleteEventArgs(ClipboardTimerType timerType)
        {
            this.TimerType = timerType;
            this.Handled = false;
        }

        /// <summary>
        /// The type of timer (e.g., username, password) associated with this event.
        /// </summary>
        public ClipboardTimerType TimerType
        {
            get;
            set;
        }

        /// <summary>
        /// Whether this event has been handled.
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }
    }
}
