using PassKeep.Lib.KeePass.Kdf;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Represents the header data of a partially parsed/validated KDBX document file.
    /// </summary>
    public class KdbxHeaderData
    {
        public KdbxHeaderData()
        {
            ProtectedBinaries = new List<ProtectedBinary>();
        }

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
        /// The entire header's data as a buffer.
        /// </summary>
        public IBuffer FullHeader
        {
            get;
            set;
        }

        /// <summary>
        /// Encryption method of the document (e.g., AES).
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
        public byte[] InnerRandomStreamKey
        {
            get;
            set;
        }

        /// <summary>
        /// Parameters for the key derivation function.
        /// </summary>
        public KdfParameters KdfParameters
        {
            get;
            set;
        }

        /// <summary>
        /// Protected binaries encoded in the inner header.
        /// </summary>
        public IList<ProtectedBinary> ProtectedBinaries
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new instance of the appropriate type of IRandomNumberGenerator.
        /// </summary>
        /// <returns>A newly initialize IRandomNumberGenerator for string protection/decryption.</returns>
        public IRandomNumberGenerator GenerateRng()
        {
            switch (InnerRandomStream)
            {
                case RngAlgorithm.ArcFourVariant:
                    return new ArcFourVariant(InnerRandomStreamKey);
                case RngAlgorithm.Salsa20:
                    return new Salsa20(InnerRandomStreamKey);
                case RngAlgorithm.ChaCha20:
                    HashAlgorithmProvider sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
                    CryptographicHash hash = sha512.CreateHash();
                    hash.Append(InnerRandomStreamKey.AsBuffer());
                    IBuffer hashed = hash.GetValueAndReset();

                    return new ChaCha20(hashed.ToArray(0, 32), hashed.ToArray(32, 12), 0);
                default:
                    throw new InvalidOperationException(String.Format("Unknown RngAlgorithm: {0}", InnerRandomStream));
            }
        }
    }
}
