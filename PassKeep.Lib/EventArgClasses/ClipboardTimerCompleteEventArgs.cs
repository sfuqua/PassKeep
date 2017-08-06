// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
        public ClipboardTimerCompleteEventArgs(ClipboardOperationType timerType)
        {
            TimerType = timerType;
            Handled = false;
        }

        /// <summary>
        /// The type of timer (e.g., username, password) associated with this event.
        /// </summary>
        public ClipboardOperationType TimerType
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
