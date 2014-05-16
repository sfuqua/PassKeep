using PassKeep.Lib.KeePass.Dom;
using System;
using System.Diagnostics;

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Represents the result of a kdbx decryption operation
    /// </summary>
    public class KdbxDecryptionResult
    {
        private KdbxDocument kdbxDocument;

        // Internal constructor for initializing fields and checking edge cases
        private KdbxDecryptionResult(ReaderResult error, KdbxDocument document)
        {
            Debug.Assert(error != null);
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            if (error != ReaderResult.Success)
            {
                Debug.Assert(document == null);
                if (document != null)
                {
                    throw new ArgumentException("If error is defined, the other arguments must be null");
                }
            }
            else
            {
                // Result is guaranteed to be Success at this point
                Debug.Assert(document != null);
                if (document == null)
                {
                    throw new ArgumentNullException("document");
                }
            }

            this.Result = error;
            this.kdbxDocument = document;
        }

        /// <summary>
        /// Constructor for a failed decryption
        /// </summary>
        /// <param name="error">The failure case of the decryption - must not be none</param>
        public KdbxDecryptionResult(ReaderResult error)
            : this(error, null) { }

        /// <summary>
        /// Constructor for a successful decryption
        /// </summary>
        /// <param name="document">The decrypted XML - must not be null</param>
        public KdbxDecryptionResult(KdbxDocument document)
            : this(ReaderResult.Success, document) { }

        /// <summary>
        /// Accesses the <see cref="ReaderResult"/> object associated
        /// with this decryption.
        /// </summary>
        public ReaderResult Result
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the XML of the document if decryption was successful,
        /// else it throws.
        /// </summary>
        /// <returns>The decrypted <see cref="KdbxDocument"/></returns>
        public KdbxDocument GetDocument()
        {
            if (Result != ReaderResult.Success)
            {
                throw new InvalidOperationException("The decryption was not successful");
            }

            Debug.Assert(this.kdbxDocument != null);
            return this.kdbxDocument;
        }
    }
}
