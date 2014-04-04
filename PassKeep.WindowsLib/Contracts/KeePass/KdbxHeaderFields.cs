namespace PassKeep.Lib.Contracts.KeePass
{
    public enum KdbxHeaderField : byte
    {
        EndOfHeader = 0,
        Comment = 1,

        /// <summary>
        /// The encryption method of the database (e.g., AES).
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
        TransformSeed = 5,

        /// <summary>
        /// The number of times to transform the key.
        /// </summary>
        TransformRounds = 6,

        /// <summary>
        /// The initialization vector for the encryption algorithm.
        /// </summary>
        EncryptionIV = 7,

        /// <summary>
        /// Seed for the RNG used for string protection.
        /// </summary>
        ProtectedStreamKey = 8,

        /// <summary>
        /// Cleartext bytes of the decrypted file, for verification.
        /// </summary>
        StreamStartBytes = 9,
        
        /// <summary>
        /// The random number generation algorithm used for string protection.
        /// </summary>
        InnerRandomStreamID = 10
    }
}
