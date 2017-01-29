using PassKeep.Lib.Contracts.Models;
using System;

namespace PassKeep.Lib.KeePass.IO
{
    public abstract class KdbxFileHandler
    {
        // Old (invalid) KeePass signature data
        public const UInt32 KP1_SIG1 = 0x9AA2D903;
        public const UInt32 KP1_SIG2 = 0xB54BFB65;
        public const UInt32 KP2_PR_SIG1 = 0x9AA2D903;
        public const UInt32 KP2_PR_SIG2 = 0xB54BFB66;

        // Current (valid) KeePass signature data
        public const UInt32 SIG1 = 0x9AA2D903;
        public const UInt32 SIG2 = 0xB54BFB67;

        /// <summary>
        /// The highest supported version of the legacy parser (3.01 as of KP 2.20).
        /// </summary>
        public static readonly UInt32 FileVersion32_3 = 0x00030002;

        /// <summary>
        /// Initial release of KDBX v4
        /// </summary>
        public static readonly UInt32 FileVersion32_4 = 0x00040000;

        // Mask out the top 4 bytes to get the "major" version of the KDBX format
        public const UInt32 FileVersionMask = 0xFFFF0000;

        // This is currently the only supported cipher UUID.
        // It's AES in CBC mode with PKCS7 padding.
        // We require a 256 bit (32 byte) k with 128 bit (16 byte) IV.
        public readonly KeePassUuid AesUuid;

        public KdbxFileHandler()
        {
            byte[] aesBytes = new byte[]
            {
                0x31, 0xC1, 0xF2, 0xE6, 0xBF, 0x71, 0x43, 0x50,
			    0xBE, 0x58, 0x05, 0x21, 0x6A, 0xFC, 0x5A, 0xFF
            };

            Guid aesGuid = new Guid(aesBytes);
            AesUuid = new KeePassUuid(aesGuid);
        }
    }
}
