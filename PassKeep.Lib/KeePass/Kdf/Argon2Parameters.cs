using PassKeep.Lib.KeePass.Crypto;
using PassKeep.Lib.KeePass.IO;
using System;

namespace PassKeep.Lib.KeePass.Kdf
{
    /// <summary>
    /// An extension of <see cref="KdfParameters"/> with values specific to Argon2.
    /// </summary>
    public sealed class Argon2Parameters : KdfParameters
    {
        public static readonly Guid Argon2Uuid = new Guid(
            new byte[] {
                0xEF, 0x63, 0x6D, 0xDF, 0x8C, 0x29, 0x44, 0x4B,
                0x91, 0xF7, 0xA9, 0xA4, 0x03, 0xE3, 0x0A, 0x0C
            }
        );

        public static readonly string SaltKey = "S"; // Byte[]
        public static readonly string ParallelismKey = "P"; // UInt32
        public static readonly string BlockCountKey = "M"; // UInt64
        public static readonly string IterationsKey = "I"; // UInt64
        public static readonly string VersionKey = "V"; // UInt32
        public static readonly string SecretKey = "K"; // Byte[]
        public static readonly string DataKey = "A"; // Byte[]

        private readonly byte[] salt;
        private readonly byte[] secretKey;
        private readonly byte[] associatedData;
        private readonly uint parallelism;
        private readonly ulong blockCount;
        private readonly ulong iterations;

        /// <summary>
        /// Initializes the parameters given a dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        public Argon2Parameters(VariantDictionary dictionary)
            : base(dictionary)
        {
            this.salt = dictionary.GetValue(SaltKey) as byte[] ?? new byte[0];
            this.secretKey = dictionary.GetValue(SecretKey) as byte[] ?? new byte[0];
            this.associatedData = dictionary.GetValue(DataKey) as byte[] ?? new byte[0];

            uint? dictParallelism = dictionary.GetValue(ParallelismKey) as uint?;
            if (dictParallelism == null)
            {
                throw new FormatException("Could not parse parallelism uint from dictionary");
            }
            this.parallelism = dictParallelism.Value;
            if (this.parallelism > Argon2d.MaxParallelism || this.parallelism < Argon2d.MinParallelism)
            {
                throw new FormatException("Parsed parallelism is out of bounds");
            }

            ulong? dictByteCount = dictionary.GetValue(BlockCountKey) as ulong?;
            if (dictByteCount == null)
            {
                throw new FormatException("Could not parse memory ulong from dictionary");
            }
            this.blockCount = dictByteCount.Value / 1024;
            if (this.blockCount > (ulong)Argon2d.MaxMemorySize)
            {
                throw new FormatException("Parsed block count is higher than allowed");
            }

            ulong? dictIterations = dictionary.GetValue(IterationsKey) as ulong?;
            if (dictIterations == null)
            {
                throw new FormatException("Could not parse iterations ulong from dictionary");
            }
            this.iterations = dictIterations.Value;
            if (this.iterations < (ulong)Argon2d.MinIterations || this.iterations > (ulong)Argon2d.MaxIterations)
            {
                throw new FormatException("Parsed iterations is out of bounds");
            }
        }

        /// <summary>
        /// Instantiates a <see cref="Argon2KdfEngine"/> instance based on the 
        /// parameters.
        /// </summary>
        /// <returns>A cryptographic engine suitable for hashing a provided
        /// password.</returns>
        public override IKdfEngine CreateEngine()
        {
            return new Argon2KdfEngine(this);
        }

        /// <summary>
        /// Helper to create an Argon2d instance using the wrapped
        /// params, for the given message.
        /// </summary>
        /// <param name="message">Message to hash.</param>
        /// <returns>A crypto instance suitable for hashing.</returns>
        public Argon2d CreateArgonInstance(byte[] message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return new Argon2d(
                message,
                this.salt,
                this.secretKey,
                this.associatedData,
                (int)this.parallelism,
                32,
                (int)this.blockCount,
                (int)this.iterations
            );
        }
    }
}
