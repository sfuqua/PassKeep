// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
            this.descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
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
