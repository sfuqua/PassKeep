// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
