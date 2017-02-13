using System;

namespace PassKeep.Lib.KeePass.DatabaseCiphers
{
    /// <summary>
    /// Common properties for ciphers used to encrypt/decrypt
    /// database plaintext.
    /// </summary>
    public interface ICipher
    {
        /// <summary>
        /// Unique identifier for the cipher.
        /// </summary>
        Guid Uuid
        {
            get;
        }

        /// <summary>
        /// Length of the cipher's initialization vector,
        /// in bytes.
        /// </summary>
        uint IvLength
        {
            get;
        }

        /// <summary>
        /// Length of the cipher's key, in bytes.
        /// </summary>
        uint KeyLength
        {
            get;
        }
    }
}
