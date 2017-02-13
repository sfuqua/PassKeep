using PassKeep.Lib.KeePass.IO;
using System;

namespace PassKeep.Lib.Contracts.KeePass
{
    public enum KdbxHeaderField : byte
    {
        EndOfHeader = 0,

        [Optional]
        Comment = 1,

        /// <summary>
        /// The encryption method of the document (e.g., AES).
        /// </summary>
        CipherID = 2,

        /// <summary>
        /// How the data is compressed.
        /// </summary>
        CompressionFlags = 3,

        /// <summary>
        /// A seed used for the initial hash.
        /// </summary>
        MasterSeed = 4,

        /// <summary>
        /// A seed used for the key transformation algorithm.
        /// </summary>
        [KdbxVersionSupport(Max = KdbxVersion.Three)]
        TransformSeed = 5,

        /// <summary>
        /// The number of times to transform the key.
        /// </summary>
        [KdbxVersionSupport(Max = KdbxVersion.Three)]
        TransformRounds = 6,

        /// <summary>
        /// The initialization vector for the encryption algorithm.
        /// </summary>
        EncryptionIV = 7,

        /// <summary>
        /// Seed for the RNG used for string protection.
        /// </summary>
        [KdbxVersionSupport(Max = KdbxVersion.Three)]
        ProtectedStreamKey = 8,

        /// <summary>
        /// Cleartext bytes of the decrypted file, for verification.
        /// </summary>
        [KdbxVersionSupport(Max = KdbxVersion.Three)]
        StreamStartBytes = 9,

        /// <summary>
        /// The random number generation algorithm used for string protection.
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
    /// Indicates that a specific header field is not required to succeed
    /// a parse.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class OptionalAttribute : Attribute { }
}
