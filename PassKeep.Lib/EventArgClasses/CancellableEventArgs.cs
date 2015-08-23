using System;
using System.Threading;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// Used for notifying clients of cancellable, long-running events being kicked off.
    /// </summary>
    public class CancellableEventArgs : EventArgs
    {
        /// <summary>
        /// Cancel the event using the associated callback
        /// </summary>
        public CancellationTokenSource Cts
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes the EventArgs.
        /// </summary>
        /// <param name="cts">A CancellationTokenSource allowing the operation to be cancelled.</param>
        public CancellableEventArgs(CancellationTokenSource cts)
        {
            this.Cts = cts;
        }
    }
}
