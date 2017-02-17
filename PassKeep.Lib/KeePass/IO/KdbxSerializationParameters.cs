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
                    UseHmacBlocks = false;
                    UseLegacyHashedBlocks = true;
                    UseInnerHeader = false;
                    UseXmlHeaderAuthentication = true;
                    UseInlineHeaderAuthentication = false;
                    UseBase64DateTimeEncoding = false;
                    break;

                // Default to modern (Four) behavior
                default:
                    HeaderFieldSizeBytes = 4;
                    UseHmacBlocks = true;
                    UseLegacyHashedBlocks = false;
                    UseInnerHeader = true;
                    UseXmlHeaderAuthentication = false;
                    UseInlineHeaderAuthentication = true;
                    UseBase64DateTimeEncoding = true;
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
            set;
        }

        /// <summary>
        /// Whether the XML DOM should store a hash of the header to validate
        /// integrity. 
        /// </summary>
        public bool UseXmlHeaderAuthentication
        {
            get;
            set;
        }

        /// <summary>
        /// Whether header authentication should be handled as part of the header
        /// itself via a plain hash followed by an HMAC value.
        /// </summary>
        public bool UseInlineHeaderAuthentication
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the database's ciphertext is authenticated using an HMAC
        /// block scheme, as implemented in <see cref="HmacBlockHandler"/>.
        /// </summary>
        public bool UseHmacBlocks
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the database's plaintext is authenticated using a hashed
        /// block scheme, as implemented in <see cref="HashedBlockParser"/> and
        /// <see cref="HashedBlockWriter"/>.
        /// </summary>
        public bool UseLegacyHashedBlocks
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the plaintext XML document is preceded by an "inner" header.
        /// </summary>
        public bool UseInnerHeader
        {
            get;
            set;
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
        /// Whether DateTime's should be serialized as base64 encodings of the int64
        /// seconds passed since 0001-01-01 00:00 UTC.
        /// </summary>
        public bool UseBase64DateTimeEncoding
        {
            get;
            set;
        }
    }
}
