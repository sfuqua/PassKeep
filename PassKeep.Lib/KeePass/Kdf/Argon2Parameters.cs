// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.KeePass.Crypto;
using PassKeep.Lib.KeePass.IO;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;

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
        public static readonly string ByteCountKey = "M"; // UInt64
        public static readonly string IterationsKey = "I"; // UInt64
        public static readonly string VersionKey = "V"; // UInt32
        public static readonly string SecretKey = "K"; // Byte[]
        public static readonly string DataKey = "A"; // Byte[]

        private readonly byte[] salt;
        private readonly byte[] secretKey;
        private readonly byte[] associatedData;

        private ulong iterations;
        private ulong blockCount;
        private uint parallelism;

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

            ulong? dictByteCount = dictionary.GetValue(ByteCountKey) as ulong?;
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

            uint? dictVersion = dictionary.GetValue(VersionKey) as uint?;
            if (dictVersion == null)
            {
                throw new FormatException("Argon2 version was not present");
            }

            if (dictVersion.Value != Argon2d.VersionNumber)
            {
                throw new FormatException($"Unsupported Argon2 version: {dictVersion.Value}");
            }
        }

        /// <summary>
        /// Initializes a parameters instance with the specified values, a random salt,
        /// no secret key and no associated data.
        /// </summary>
        /// <param name="parallelism"></param>
        /// <param name="blockCount"></param>
        /// <param name="iterations"></param>
        public Argon2Parameters(uint parallelism, ulong blockCount, ulong iterations)
            : this(parallelism, blockCount, iterations, new byte[0], new byte[0])
        { }

        /// <summary>
        /// Private constructor that validates all fields - salt is a 32-byte random value.
        /// </summary>
        /// <param name="parallelism"></param>
        /// <param name="blockCount"></param>
        /// <param name="iterations"></param>
        /// <param name="secretKey"></param>
        /// <param name="data"></param>
        private Argon2Parameters(
            uint parallelism,
            ulong blockCount,
            ulong iterations,
            byte[] secretKey,
            byte[] data
        ) : base(Argon2Uuid)
        {
            secretKey = secretKey ?? new byte[0];
            data = data ?? new byte[0];
            
            if (parallelism > Argon2d.MaxParallelism)
            {
                throw new ArgumentOutOfRangeException(nameof(parallelism));
            }

            if (blockCount > (ulong)Argon2d.MaxMemorySize)
            {
                throw new ArgumentOutOfRangeException(nameof(blockCount));
            }

            if (blockCount < 8 * parallelism)
            {
                throw new ArgumentOutOfRangeException(nameof(blockCount));
            }

            if (iterations < (ulong)Argon2d.MinIterations || iterations > (ulong)Argon2d.MaxIterations)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations));
            }

            this.parallelism = parallelism;
            this.blockCount = blockCount;
            this.iterations = iterations;

            this.secretKey = new byte[secretKey.Length];
            if (secretKey.Length > 0)
            {
                Buffer.BlockCopy(secretKey, 0, this.secretKey, 0, secretKey.Length);
            }

            this.associatedData = new byte[data.Length];
            if (data.Length > 0)
            {
                Buffer.BlockCopy(data, 0, this.associatedData, 0, data.Length);
            }

            this.salt = CryptographicBuffer.GenerateRandom(32).ToArray();
        }

        /// <summary>
        /// The number of Argon2 iterations to run during the KDF.
        /// </summary>
        public ulong Iterations
        {
            get => this.iterations;
            set => this.iterations = value;
        }

        /// <summary>
        /// The number of kilobyte blocks to use for key transformation.
        /// </summary>
        public ulong BlockCount
        {
            get => this.blockCount;
            set => this.blockCount = value;
        }

        /// <summary>
        /// The number of threads to use for deriving the key.
        /// </summary>
        public uint Parallelism
        {
            get => this.parallelism;
            set => this.parallelism = value;
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

        /// <summary>
        /// Generates a new <see cref="Argon2Parameters"/> instance with all the same values
        /// except for a new random salt.
        /// </summary>
        /// <returns>An reseeded <see cref="Argon2Parameters"/> instance.</returns>
        public override KdfParameters Reseed()
        {
            return new Argon2Parameters(
                this.parallelism,
                this.blockCount,
                this.iterations,
                this.secretKey,
                this.associatedData
            );
        }

        /// <summary>
        /// Saves the various Argon2 parameters to a dictionary.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, VariantValue> ToDictionary()
        {
            var dict = base.ToDictionary();
            dict[ParallelismKey] = new VariantValue(this.parallelism);
            dict[ByteCountKey] = new VariantValue(this.blockCount * 1024);
            dict[IterationsKey] = new VariantValue(this.iterations);
            dict[SecretKey] = new VariantValue(this.secretKey);
            dict[DataKey] = new VariantValue(this.associatedData);
            dict[SaltKey] = new VariantValue(this.salt);
            dict[VersionKey] = new VariantValue((uint)Argon2d.VersionNumber);

            return dict;
        }

        /// <summary>
        /// Evaluates whether the provided object is an <see cref="Argon2Parameters"/>
        /// instance with the same settings.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Argon2Parameters other))
            {
                return false;
            }

            return other.BlockCount == BlockCount &&
                other.Parallelism == Parallelism &&
                other.Iterations == Iterations &&
                other.secretKey.SequenceEqual(this.secretKey) &&
                other.associatedData.SequenceEqual(this.associatedData);
        }

        /// <summary>
        /// Generated override.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = 1350692970;
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(this.secretKey);
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(this.associatedData);
            hashCode = hashCode * -1521134295 + Iterations.GetHashCode();
            hashCode = hashCode * -1521134295 + BlockCount.GetHashCode();
            hashCode = hashCode * -1521134295 + Parallelism.GetHashCode();
            return hashCode;
        }
    }
}
