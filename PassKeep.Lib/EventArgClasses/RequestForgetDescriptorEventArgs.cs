using PassKeep.Models;
using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// EventArgs for requesting to forget a specific descriptor.
    /// </summary>
    public class RequestForgetDescriptorEventArgs : ConsentRequestedEventArgs
    {
        private readonly StoredFileDescriptor descriptor;

        /// <summary>
        /// Initializes the event args.
        /// </summary>
        /// <param name="descriptor">The descriptor being deleted.</param>
        public RequestForgetDescriptorEventArgs(StoredFileDescriptor descriptor)
            : base(true)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            this.descriptor = descriptor;
        }

        /// <summary>
        /// Gets the descriptor being deleted.
        /// </summary>
        public StoredFileDescriptor Descriptor
        {
            get { return this.descriptor; }
        }
    }
}
