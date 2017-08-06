// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Flags corresponding to a binary encoded by
    /// <see cref="InnerHeaderField.Binary"/>.
    /// </summary>
    [Flags]
    public enum KdbxBinaryFlags : byte
    {
        None = 0,

        /// <summary>
        /// This binary should be protected in memory.
        /// </summary>
        MemoryProtected = 0x1
    }
}
