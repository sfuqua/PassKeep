// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.KeePass.IO;
using System;

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Fields found in the plaintext "outer header" of a KDBX file.
    /// </summary>
    public enum OuterHeaderField : byte
    {
        /// <summary>
        /// Signifies that the header is over.
        /// </summary>
        EndOfHeader = 0,

        [Optional]
        Comment = 1,

        /// <summary>
        /// Guid indicating encryption method of the document (e.g., AES, ChaCha20).
        /// </summary>
        CipherID = 2,

        /// <summary>
        /// How the decrypted is compressed.
        /// </summary>
        CompressionFlags = 3,

        /// <summary>
        /// A seed used for the initial hash for key derivation.
        /// </summary>
        MasterSeed = 4,

        /// <summary>
        /// A seed used for the key transformation algorithm.
        /// Superceded by <see cref="KdfParameters"/>.
        /// </summary>
        [KdbxVersionSupport(Max = KdbxVersion.Three)]
        TransformSeed = 5,

        /// <summary>
        /// The number of times to transform the key.
        /// Superceded by <see cref="KdfParameters"/>.
        /// </summary>
        [KdbxVersionSupport(Max = KdbxVersion.Three)]
        TransformRounds = 6,

        /// <summary>
        /// The initialization vector for the encryption algorithm.
        /// </summary>
        EncryptionIV = 7,

        /// <summary>
        /// Seed for the RNG used for string protection.
        /// Superceded by <see cref="InnerHeaderField.InnerRandomStreamKey"/>.
        /// </summary>
        [KdbxVersionSupport(Max = KdbxVersion.Three)]
        ProtectedStreamKey = 8,

        /// <summary>
        /// Cleartext bytes of the decrypted file, for verification.
        /// Superceded by HMAC data authentication blocks.
        /// </summary>
        [KdbxVersionSupport(Max = KdbxVersion.Three)]
        StreamStartBytes = 9,

        /// <summary>
        /// The random number generation algorithm used for string protection.
        /// Superceded by <see cref="InnerHeaderField.InnerRandomStreamID"/>.
        /// </summary>
        [KdbxVersionSupport(Max = KdbxVersion.Three)]
        InnerRandomStreamID = 10,

        /// <summary>
        /// Parameters used to initialize the key derivation function.
        /// </summary>
        [KdbxVersionSupport(Min = KdbxVersion.Four)]
        KdfParameters = 11,

        /// <summary>
        /// Allows plugins to keep custom plaintext in the header.
        /// </summary>
        [KdbxVersionSupport(Min = KdbxVersion.Four), Optional]
        PublicCustomData = 12
    }

    /// <summary>
    /// Fields found in the encrypted "inner header" of a KDBX file.
    /// </summary>
    [KdbxVersionSupport(Min = KdbxVersion.Four)]
    public enum InnerHeaderField : byte
    {
        /// <summary>
        /// Indicates the end of the inner header.
        /// </summary>
        EndOfHeader = 0,

        /// <summary>
        /// Identifier GUID for the PRNG algorithm (Salsa20, ChaCha20, etc).
        /// Supercedes <see cref="OuterHeaderField.InnerRandomStreamID"/>.
        /// </summary>
        InnerRandomStreamID = 1,

        /// <summary>
        /// Seed bytes for the PRNG algorithm.
        /// Supercedes <see cref="OuterHeaderField.ProtectedStreamKey"/>.
        /// </summary>
        InnerRandomStreamKey = 2,

        /// <summary>
        /// Indicates a binary attachment.
        /// </summary>
        Binary = 3
    }

    /// <summary>
    /// Indicates that a specific header field is not required to succeed
    /// a parse.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class OptionalAttribute : Attribute { }
}
