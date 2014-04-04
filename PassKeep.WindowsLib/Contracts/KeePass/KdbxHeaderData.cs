using PassKeep.Lib.KeePass.Rng;
using System;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Represents the header data of a partially parsed/validated KDBX database file.
    /// </summary>
    public class KdbxHeaderData
    {
        /// <summary>
        /// The length of the header, in bytes.
        /// </summary>
        public ulong Size
        {
            get;
            set;
        }

        /// <summary>
        /// A hashedData string representing the entire header, for validation purposes.
        /// </summary>
        public string HeaderHash
        {
            get;
            set;
        }

        /// <summary>
        /// Encryption method of the database (e.g., AES).
        /// </summary>
        public EncryptionAlgorithm Cipher
        {
            get;
            set;
        }

        /// <summary>
        /// How the data is compressed.
        /// </summary>
        public CompressionAlgorithm Compression
        {
            get;
            set;
        }

        /// <summary>
        /// A seed used for the initial hash.
        /// </summary>
        public IBuffer MasterSeed
        {
            get;
            set;
        }

        /// <summary>
        /// A seed used for the key transformation algorithm.
        /// </summary>
        public IBuffer TransformSeed
        {
            get;
            set;
        }

        /// <summary>
        /// The number of times to transform the key.
        /// </summary>
        public UInt64 TransformRounds
        {
            get;
            set;
        }

        /// <summary>
        /// The initialization vector for the encryption algorithm.
        /// </summary>
        public IBuffer EncryptionIV
        {
            get;
            set;
        }

        /// <summary>
        /// Cleartext bytes of the decrypted file, for verification.
        /// </summary>
        public IBuffer StreamStartBytes
        {
            get;
            set;
        }

        /// <summary>
        /// The random number generation algorithm used for string protection.
        /// </summary>
        public RngAlgorithm InnerRandomStream
        {
            get;
            set;
        }

        /// <summary>
        /// Seed for the RNG used for string protection.
        /// </summary>
        public byte[] ProtectedStreamKey
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of the appropriate type of IRandomNumberGenerator.
        /// </summary>
        /// <returns>A newly initialize IRandomNumberGenerator for string protection/decryption.</returns>
        public IRandomNumberGenerator GenerateRng()
        {
            switch (this.InnerRandomStream)
            {
                case RngAlgorithm.ArcFourVariant:
                    return new ArcFourVariant(this.ProtectedStreamKey);
                case RngAlgorithm.Salsa20:
                    return new Salsa20(this.ProtectedStreamKey);
                default:
                    throw new InvalidOperationException(String.Format("Unknown RngAlgorithm: {0}", this.InnerRandomStream));
            }
        }
    }
}
