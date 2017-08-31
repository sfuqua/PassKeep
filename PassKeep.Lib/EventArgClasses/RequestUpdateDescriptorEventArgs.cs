// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
            this.descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
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
