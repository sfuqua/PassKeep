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
