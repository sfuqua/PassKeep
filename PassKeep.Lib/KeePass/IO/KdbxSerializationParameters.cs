// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;

namespace PassKeep.Lib.KeePass.IO
{
    /// <summary>
    /// Configures how a KDBX file is parsed and written.
    /// </summary>
    public class KdbxSerializationParameters
    {
        private readonly KdbxVersion version;

        /// <summary>
        /// Configures the basic parameters based on the version being used.
        /// </summary>
        /// <param name="version">The current parser version.</param>
        public KdbxSerializationParameters(KdbxVersion version)
        {
            this.version = version;

            // Update parameters based on the parser version.
            switch (Version)
            {
                case KdbxVersion.Three:
                    HeaderFieldSizeBytes = 2;
                    UseExtensibleKdf = false;
                    UseHmacBlocks = false;
                    UseLegacyHashedBlocks = true;
                    UseInnerHeader = false;
                    UseXmlHeaderAuthentication = true;
                    UseInlineHeaderAuthentication = false;
                    UseBase64DateTimeEncoding = false;
                    BinariesInHeader = false;
                    break;

                // Default to modern (Four) behavior
                default:
                    HeaderFieldSizeBytes = 4;
                    UseExtensibleKdf = true;
                    UseHmacBlocks = true;
                    UseLegacyHashedBlocks = false;
                    UseInnerHeader = true;
                    UseXmlHeaderAuthentication = false;
                    UseInlineHeaderAuthentication = true;
                    UseBase64DateTimeEncoding = true;
                    BinariesInHeader = true;
                    break;
            }
        }

        /// <summary>
        /// Gets the major version of the current parser.
        /// </summary>
        public KdbxVersion Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// Number of bytes used to store the size of a header field's value.
        /// </summary>
        public uint HeaderFieldSizeBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether a <see cref="VariantDictionary"/> is used together with a 
        /// KDF parameters header field to control key derivation.
        /// </summary>
        public bool UseExtensibleKdf
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether AES is used for key derivation and two header fields provide its parameters.
        /// </summary>
        public bool UseLegacyKdfHeader
        {
            get { return !UseExtensibleKdf; }
        }

        /// <summary>
        /// Whether the XML DOM should store a hash of the header to validate
        /// integrity. 
        /// </summary>
        public bool UseXmlHeaderAuthentication
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether header authentication should be handled as part of the header
        /// itself via a plain hash followed by an HMAC value.
        /// </summary>
        public bool UseInlineHeaderAuthentication
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the database's ciphertext is authenticated using an HMAC
        /// block scheme, as implemented in <see cref="HmacBlockHandler"/>.
        /// </summary>
        public bool UseHmacBlocks
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the database's plaintext is authenticated using a hashed
        /// block scheme, as implemented in <see cref="HashedBlockParser"/> and
        /// <see cref="HashedBlockWriter"/>.
        /// </summary>
        public bool UseLegacyHashedBlocks
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the plaintext XML document is preceded by an "inner" header.
        /// </summary>
        public bool UseInnerHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// How (if) the entire plaintext (hashed or otherwise) is compressed.
        /// </summary>
        public CompressionAlgorithm Compression
        {
            get;
            set;
        }

        /// <summary>
        /// Whether binaries are serialized into the inner header.
        /// False indicates they are part of the KDBX Meta element.
        /// </summary>
        public bool BinariesInHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether binaries are serialized as part of the DOM in base64.
        /// False indicates they are part of the binary header.
        /// </summary>
        public bool BinariesInXml
        {
            get { return !BinariesInHeader; }
        }

        /// <summary>
        /// Whether DateTime's should be serialized as base64 encodings of the int64
        /// seconds passed since 0001-01-01 00:00 UTC.
        /// </summary>
        public bool UseBase64DateTimeEncoding
        {
            get;
            private set;
        }
    }
}
