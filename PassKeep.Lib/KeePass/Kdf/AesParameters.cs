// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.KeePass.IO;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using System.Collections.Generic;

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
            Rounds = dictRounds ?? DefaultRounds;

            if (!(dictionary.GetValue(SeedKey) is byte[] dictSeed))
            {
                throw new FormatException("AesParameters requires a byte[] seed value");
            }

            Seed = dictSeed.AsBuffer();
        }

        /// <summary>
        /// Initializes the parameters with specific values instead of a dictionary.
        /// </summary>
        /// <param name="rounds">Number of times to transform the key.</param>
        /// <param name="seed">Seed to use for key transformation.</param>
        public AesParameters(ulong rounds, IBuffer seed)
            : base(AesUuid)
        {
            Rounds = rounds;
            Seed = seed ?? throw new ArgumentNullException(nameof(seed));
        }

        /// <summary>
        /// Initializes the parameters with a specific number of transform rounds
        /// and a random seed.
        /// </summary>
        /// <param name="rounds">Number of times to transform the key.</param>
        public AesParameters(ulong rounds)
            : base(AesUuid)
        {
            Rounds = rounds;
            Seed = CryptographicBuffer.GenerateRandom(32);
        }

        /// <summary>
        /// Number of rounds to run AES when transforming the key.
        /// </summary>
        public ulong Rounds { get; set; }

        /// <summary>
        /// Seed to use for AES.
        /// </summary>
        public IBuffer Seed { get; }

        /// <summary>
        /// Creates an AES engine for transforming a user's key.
        /// </summary>
        /// <returns>A cryptographic engine suitable for hashing a provided
        /// password.</returns>
        public override IKdfEngine CreateEngine()
        {
            return new AesKdfEngine(this);
        }

        /// <summary>
        /// Randomizes <see cref="Seed"/> into a new AES instance.
        /// </summary>
        /// <returns>An reseeded <see cref="AesParameters"/> instance.</returns>
        public override KdfParameters Reseed()
        {
            return new AesParameters(Rounds);
        }

        /// <summary>
        /// Saves transform rounds and seed to a dictionary.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, VariantValue> ToDictionary()
        {
            var dict = base.ToDictionary();
            dict[RoundsKey] = new VariantValue(Rounds);
            dict[SeedKey] = new VariantValue(Seed.ToArray());

            return dict;
        }

        /// <summary>
        /// Evaluates whether the provided object is an <see cref="AesParameters"/>
        /// instance with the same number of KDF rounds.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is AesParameters other))
            {
                return false;
            }

            return other.Rounds == Rounds;
        }

        /// <summary>
        /// Generated override.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return -1930869704 + Rounds.GetHashCode();
        }
    }
}
