// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Lib.KeePass.DatabaseCiphers
{
    /// <summary>
    /// AES in CBC mode with PKCS7 padding.
    /// We require a 256 bit (32 byte) key with 128 bit (16 byte) IV.
    /// </summary>
    public sealed class AesCipher
    {
        public static readonly uint IvBytes = 16;

        /// <summary>
        /// Unique identifier for the AES database cipher.
        /// </summary>
        public static readonly Guid Uuid = new Guid(
            new byte[] {
                0x31, 0xC1, 0xF2, 0xE6, 0xBF, 0x71, 0x43, 0x50,
                0xBE, 0x58, 0x05, 0x21, 0x6A, 0xFC, 0x5A, 0xFF
            }
        );
    }
}
