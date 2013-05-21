using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage.Streams;
using PassKeep.Models;

namespace PassKeep.KeePassLib
{
    public abstract class KdbxHandler
    {
        public XDocument Document
        {
            get;
            set;
        }

        public CompressionAlgorithm _compression;
        protected IBuffer _masterSeed;
        protected IBuffer _transformSeed;
        protected UInt64 _transformRounds;
        protected IBuffer _encryptionIV;

        protected KeePassRng _masterRng;
        public KeePassRng GetRng()
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

        public enum KdbxHeaderField : byte
        {
            EndOfHeader = 0,
            Comment = 1,
            CipherID = 2,
            CompressionFlags = 3,
            MasterSeed = 4,
            TransformSeed = 5,
            TransformRounds = 6,
            EncryptionIV = 7,
            ProtectedStreamKey = 8,
            StreamStartBytes = 9,
            InnerRandomStreamID = 10
        }

        public enum CompressionAlgorithm : int
        {
            None = 0,
            GZip = 1
        }

        public enum RngAlgorithm : int
        {
            ArcFourVariant = 1,
            Salsa20 = 2
        }

        public KdbxHandler()
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
