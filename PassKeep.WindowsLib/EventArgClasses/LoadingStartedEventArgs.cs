using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// Represents arguments for a cancelable event related to
    /// an event that tracks its own progress.
    /// </summary>
    public class LoadingStartedEventArgs : CancelableEventArgs
    {
        /// <summary>
        /// Whether the progress of this event is indeterminante
        /// </summary>
        public bool Indeterminate
        {
            get;
            set;
        }

        /// <summary>
        /// A label or caption for the loading event
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">A label or caption for this event</param>
        /// <param name="cancel">A callback to cancel the loading event</param>
        /// <param name="indeterminate">Whether this event has determinate progress</param>
        public LoadingStartedEventArgs(string text, Action cancel, bool indeterminate = true)
            : base(cancel)
        {
            Indeterminate = indeterminate;
            Text = text;
        }
    }
}
