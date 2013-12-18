using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using System;
using System.Threading;
using System.Xml.Linq;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.IO
{
    public abstract class KdbxFileHandler
    {
        public XDocument Document
        {
            get;
            set;
        }

        protected CancellationTokenSource cts
        {
            get;
            set;
        }

        public CompressionAlgorithm _compression;
        protected IBuffer _masterSeed;
        protected IBuffer _transformSeed;
        protected UInt64 _transformRounds;
        protected IBuffer _encryptionIV;

        protected IRandomNumberGenerator _masterRng;
        public IRandomNumberGenerator GetRng()
        {
            return _masterRng.Clone();
        }

        protected byte[] _protectedStreamKey;
        protected IBuffer _streamStartBytes;

        public const UInt32 KP1_SIG1 = 0x9AA2D903;
        public const UInt32 KP1_SIG2 = 0xB54BFB65;
        public const UInt32 KP2_PR_SIG1 = 0x9AA2D903;
        public const UInt32 KP2_PR_SIG2 = 0xB54BFB66;

        public const UInt32 SIG1 = 0x9AA2D903;
        public const UInt32 SIG2 = 0xB54BFB67;

        // The highest supported version of this parser (3.01 as of KP 2.20)
        public const UInt32 FileVersion32 = 0x00030002;
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

        public void Cancel()
        {
            if (cts == null)
            {
                return;
            }

            cts.Cancel();
            cts = null;
        }
    }
}
