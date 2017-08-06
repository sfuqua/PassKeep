// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using SariphLib.Infrastructure;
using System;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Represents the result of a kdbx decryption operation
    /// </summary>
    public class KdbxDecryptionResult
    {
        private KdbxSerializationParameters kdbxParameters;
        private KdbxDocument kdbxDocument;
        private IBuffer rawKey;

        // Internal constructor for initializing fields and checking edge cases
        private KdbxDecryptionResult(ReaderResult error, KdbxSerializationParameters kdbxParameters, KdbxDocument document, IBuffer rawKey)
        {
            Dbg.Assert(error != null);
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            if (error != ReaderResult.Success)
            {
                Dbg.Assert(document == null);
                if (document != null)
                {
                    throw new ArgumentException("If error is defined, the other arguments must be null");
                }
            }
            else
            {
                // Result is guaranteed to be Success at this point
                Dbg.Assert(document != null);
                if (document == null)
                {
                    throw new ArgumentNullException(nameof(document));
                }

                if (rawKey == null)
                {
                    throw new ArgumentNullException(nameof(rawKey));
                }
            }

            Result = error;
            this.kdbxParameters = kdbxParameters;
            this.kdbxDocument = document;
            this.rawKey = rawKey;
        }

        /// <summary>
        /// Constructor for a failed decryption
        /// </summary>
        /// <param name="error">The failure case of the decryption - must not be none</param>
        public KdbxDecryptionResult(ReaderResult error)
            : this(error, null, null, null) { }

        /// <summary>
        /// Constructor for a successful decryption
        /// </summary>
        /// <param name="kdbxParams">Parameters parsed from the KDBX header.</param>
        /// <param name="document">The decrypted XML - must not be null</param>
        /// <param name="rawDecryptionKey">The raw key used to decrypt the database,
        /// before transformation</param>
        public KdbxDecryptionResult(KdbxSerializationParameters kdbxParams, KdbxDocument document, IBuffer rawDecryptionKey)
            : this(ReaderResult.Success, kdbxParams, document, rawDecryptionKey) { }

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
        /// Accesses the parameters that were parsed during deserialization. 
        /// </summary>
        public KdbxSerializationParameters Parameters
        {
            get { return this.kdbxParameters; }
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

            Dbg.Assert(this.kdbxDocument != null);
            return this.kdbxDocument;
        }

        /// <summary>
        /// Returns the raw buffer used to decrypt the database if decryption was successful,
        /// else it throws.
        /// </summary>
        /// <returns>The <see cref="IBuffer"/> that was transformed to decrypt</returns>
        public IBuffer GetRawKey()
        {
            if (Result != ReaderResult.Success)
            {
                throw new InvalidOperationException("The decryption was not successful");
            }

            Dbg.Assert(this.rawKey != null);
            return this.rawKey;
        }
    }
}
