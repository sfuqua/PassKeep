using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// An event indicating that a class wishes to perform a specific action.
    /// </summary>
    public class RequestedActionEventArgs : EventArgs
    {
        private Action allowCallback;

        /// <summary>
        /// Constructs the class around the specific callback.
        /// </summary>
        /// <param name="allowCallback"></param>
        public RequestedActionEventArgs(Action allowCallback)
        {
            this.allowCallback = allowCallback;
        }

        /// <summary>
        /// Whether this event has been permitted.
        /// </summary>
        public bool Permitted
        {
            get;
            private set;
        }

        /// <summary>
        /// Marks the event as permitted and runs the callback.
        /// </summary>
        public void Allow()
        {
            if (this.Permitted)
            {
                return;
            }

            this.Permitted = true;
            this.allowCallback();
        }
    }
}
