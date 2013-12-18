using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// Represents an EventArgs instance with a callback.
    /// </summary>
    /// <remarks>
    /// The callback is intended to be a means to cancel the event.
    /// </remarks>
    public class CancelableEventArgs : EventArgs
    {
        /// <summary>
        /// Cancel the event using the associated callback
        /// </summary>
        public Action Cancel
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancel">A callback to cancel the associated event</param>
        public CancelableEventArgs(Action cancel)
        {
            Cancel = cancel;
        }
    }
}
