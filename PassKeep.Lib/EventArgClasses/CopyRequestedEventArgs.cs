using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
using System;

namespace PassKeep.EventArgClasses
{
    /// <summary>
    /// EventArgs for an event indicating the user wishes to copy data to their clipboard.
    /// </summary>
    public class CopyRequestedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates an instance with the specified type of copy operation.
        /// </summary>
        /// <param name="type">The type of copy being performed (name vs password).</param>
        public CopyRequestedEventArgs(IKeePassEntry entry, ClipboardOperationType type)
        {
            Entry = entry;
            CopyType = type;
        }

        /// <summary>
        /// The entry whose data is being copied.
        /// </summary>
        public IKeePassEntry Entry
        {
            get;
            private set;
        }

        /// <summary>
        /// The type of copy operation being performed.
        /// </summary>
        public ClipboardOperationType CopyType
        {
            get;
            private set;
        }
    }
}
