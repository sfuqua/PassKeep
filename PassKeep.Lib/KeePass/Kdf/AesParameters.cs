using PassKeep.Lib.KeePass.IO;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.Kdf
{
    /// <summary>
    /// An extension of <see cref="KdfParameters"/> with values specific to AES.
    /// </summary>
    public sealed class AesParameters : KdfParameters
    {
        public static readonly Guid AesUuid = new Guid(
            new byte[]
            {
                0xC9, 0xD9, 0xF3, 0x9A, 0x62, 0x8A, 0x44, 0x60,
                0xBF, 0x74, 0x0D, 0x08, 0xC1, 0x8A, 0x4F, 0xEA
            }
        );

        public static readonly string RoundsKey = "R";
        public static readonly string SeedKey = "S";

        public static readonly ulong DefaultRounds = 6000;

        private readonly ulong rounds;
        private readonly IBuffer seed;

        /// <summary>
        /// Initializes the parameters given a dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        public AesParameters(VariantDictionary dictionary)
            : base(dictionary)
        {
            if (Uuid != AesUuid)
            {
                throw new FormatException("Wrong UUID for AesParameters");
            }

            ulong? dictRounds = dictionary.GetValue(RoundsKey) as ulong?;
            this.rounds = dictRounds ?? DefaultRounds;

            byte[] dictSeed = dictionary.GetValue(SeedKey) as byte[];
            if (dictSeed == null)
            {
                throw new FormatException("AesParameters requires a byte[] seed value");
            }

            this.seed = dictSeed.AsBuffer();
        }

        /// <summary>
        /// Initializes the parameters with specific values instead of a dictionary.
        /// </summary>
        /// <param name="rounds">Number of times to transform the key.</param>
        /// <param name="seed">Seed to use for key transformation.</param>
        public AesParameters(ulong rounds, IBuffer seed)
            : base(AesUuid)
        {
            if (seed == null)
            {
                throw new ArgumentNullException(nameof(seed));
            }

            this.rounds = rounds;
            this.seed = seed;
        }

        /// <summary>
        /// Number of rounds to run AES when transforming the key.
        /// </summary>
        public ulong Rounds
        {
            get { return this.rounds; }
        }

        /// <summary>
        /// Seed to use for AES.
        /// </summary>
        public IBuffer Seed
        {
            get { return this.seed; }
        }

        /// <summary>
        /// Creates an AES engine for transforming a user's key.
        /// </summary>
        /// <returns>A cryptographic engine suitable for hashing a provided
        /// password.</returns>
        public override IKdfEngine CreateEngine()
        {
            return new AesKdfEngine(this);
        }
    }
}
