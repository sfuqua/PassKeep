using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// An <see cref="EventArgs"/> extension that provides
    /// access to an <see cref="KdbxDocument"/> representing a KeePass
    /// document.
    /// </summary>
    public class DocumentReadyEventArgs : EventArgs
    {
        /// <summary>
        /// A decrypted XML document representing a KeePass document.
        /// </summary>
        public KdbxDocument Document { get; private set; }

        /// <summary>
        /// An object capable of writing to the new document.
        /// </summary>
        public IKdbxWriter Writer { get; private set; }

        /// <summary>
        /// Initializes the EventArgs with the provided parameters.
        /// </summary>
        /// <param name="document">A model representing the decrypted XML document.</param>
        /// <param name="writer">An instance of IKdbxWriter that can write to the document.</param>
        public DocumentReadyEventArgs(KdbxDocument document, IKdbxWriter writer)
        {
            this.Document = document;
            this.Writer = writer;
        }
    }
}
