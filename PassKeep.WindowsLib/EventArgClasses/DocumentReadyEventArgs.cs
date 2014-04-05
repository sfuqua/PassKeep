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

        /// <summary>
        /// Initializes the EventArgs with the provided parameters.
        /// </summary>
        /// <param name="document">A cleartext (aside from protected string values) XDocument representing the database.</param>
        /// <param name="rng">A random number generator used for string protection.</param>
        public DocumentReadyEventArgs(XDocument document, IRandomNumberGenerator rng)
        {
            Document = document;
            Rng = rng;
        }
    }
}
