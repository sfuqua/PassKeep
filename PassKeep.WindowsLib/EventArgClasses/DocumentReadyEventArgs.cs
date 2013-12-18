using PassKeep.Lib.Contracts.KeePass;
using System;
using System.Xml.Linq;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// An <see cref="EventArgs"/> extension that provides
    /// access to an <see cref="XDocument"/> representing a KeePass
    /// database, alonng with the RNG needed to decrypt its protected strings.
    /// </summary>
    public class DocumentReadyEventArgs : EventArgs
    {
        /// <summary>
        /// A decrypted XML document representing a KeePass database
        /// </summary>
        public XDocument Document { get; private set; }

        /// <summary>
        /// A random number generator used to deprotect strings for the database
        /// </summary>
        public IRandomNumberGenerator Rng { get; private set; }

        public DocumentReadyEventArgs(XDocument document, IRandomNumberGenerator rng)
        {
            Document = document;
            Rng = rng;
        }
    }
}
