using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Represents the result of a kdbx decryption operation
    /// </summary>
    public class DecryptionResult
    {
        private XDocument _xmlDocument;

        /// <summary>
        /// Returns the XML of the document if decryption was successful,
        /// else it throws.
        /// </summary>
        /// <returns>The decrypted <see cref="XDocument"/></returns>
        public XDocument GetXmlDocument()
        {
            if (Error != KeePassError.None)
            {
                throw new InvalidOperationException("The decryption was not successful");
            }

            Debug.Assert(_xmlDocument != null);
            return _xmlDocument;
        }

        private IRandomNumberGenerator _masterRng;

        /// <summary>
        /// Returns a clone of the document's RNG if decryption was successful,
        /// else it throws.
        /// </summary>
        /// <returns>The <see cref="IRandomNumberGenerator"/> used by the document</returns>
        public IRandomNumberGenerator GetDocumentRng()
        {
            if (Error != KeePassError.None)
            {
                throw new InvalidOperationException("The decryption was not successful");
            }

            Debug.Assert(_masterRng != null);
            return _masterRng.Clone();
        }

        /// <summary>
        /// Accesses the <see cref="KeePassError"/> object associated
        /// with this decryption.
        /// </summary>
        public KeePassError Error
        {
            get;
            private set;
        }

        // Internal constructor for initializing fields and checking edge cases
        private DecryptionResult(KeePassError error, XDocument document, IRandomNumberGenerator rng)
        {
            Debug.Assert(error != null);
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            if (error != KeePassError.None)
            {
                Debug.Assert(document == null);
                Debug.Assert(rng == null);
                if (document != null || rng != null)
                {
                    throw new ArgumentException("If error is defined, the other arguments must be null");
                }
            }
            else
            {
                // Error is guaranteed to be None at this point
                Debug.Assert(document != null);
                if (document == null)
                {
                    throw new ArgumentNullException("document");
                }

                Debug.Assert(rng != null);
                if (rng == null)
                {
                    throw new ArgumentNullException("rng");
                }
            }

            Error = error;
            _xmlDocument = document;
            _masterRng = rng;
        }

        /// <summary>
        /// Constructor for a failed decryption
        /// </summary>
        /// <param name="error">The failure case of the decryption - must not be none</param>
        public DecryptionResult(KeePassError error)
            : this(error, null, null) { }

        /// <summary>
        /// Constructor for a successful decryption
        /// </summary>
        /// <param name="document">The decrypted XML - must not be null</param>
        /// <param name="rng">The RNG associated with the document - must not be null</param>
        public DecryptionResult(XDocument document, IRandomNumberGenerator rng)
            : this(KeePassError.None, document, rng) { }
    }
}
