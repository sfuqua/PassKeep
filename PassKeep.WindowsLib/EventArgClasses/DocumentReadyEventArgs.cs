using PassKeep.Lib.KeePass.Dom;
using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// An <see cref="EventArgs"/> extension that provides
    /// access to an <see cref="KdbxDocument"/> representing a KeePass
    /// database.
    /// </summary>
    public class DocumentReadyEventArgs : EventArgs
    {
        /// <summary>
        /// A decrypted XML document representing a KeePass database.
        /// </summary>
        public KdbxDocument Document { get; private set; }

        /// <summary>
        /// Initializes the EventArgs with the provided parameters.
        /// </summary>
        /// <param name="document">A model representing the decrypted XML database.</param>
        public DocumentReadyEventArgs(KdbxDocument document)
        {
            this.Document = document;
        }
    }
}
