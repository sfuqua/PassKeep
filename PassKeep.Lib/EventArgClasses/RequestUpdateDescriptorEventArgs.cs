using PassKeep.Models;
using SariphLib.Eventing;
using SariphLib.Files;
using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// EventArgs for requesting to update the file backing a specific descriptor.
    /// </summary>
    public class RequestUpdateDescriptorEventArgs : DeferrableEventArgs
    {
        private readonly StoredFileDescriptor descriptor;

        /// <summary>
        /// Initializes the event args.
        /// </summary>
        /// <param name="descriptor">The descriptor being deleted.</param>
        public RequestUpdateDescriptorEventArgs(StoredFileDescriptor descriptor)
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

        /// <summary>
        /// Gets or sets the file that should be used to update the file
        /// behind <see cref="Descriptor"/>.
        /// </summary>
        public ITestableFile File
        {
            get;
            set;
        }
    }
}
