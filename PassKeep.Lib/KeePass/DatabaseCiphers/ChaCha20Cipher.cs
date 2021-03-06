﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Lib.KeePass.DatabaseCiphers
{
    public sealed class ChaCha20Cipher
    {
        public static readonly uint IvBytes = 12;

        /// <summary>
        /// Unique identifier for the ChaCha20 database cipher.
        /// </summary>
        public static readonly Guid Uuid = new Guid(
            new byte[] {
                0xD6, 0x03, 0x8A, 0x2B, 0x8B, 0x6F, 0x4C, 0xB5,
                0xA5, 0x24, 0x33, 0x9A, 0x31, 0xDB, 0xB5, 0x9A
            }
        );
    }
}
