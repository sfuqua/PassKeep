using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.Util;

namespace PassKeep.Tests
{
    /// <summary>
    /// Tests for the ChaCha20 stream cipher, based on RFC 7539.
    /// </summary>
    /// <remarks>
    /// https://tools.ietf.org/html/rfc7539
    /// </remarks>
    [TestClass]
    public class ChaCha20Tests
    {
        /// <summary>
        /// Tests for assumptions about how bitwise math operates
        /// in the ChaCha20 algorithms.
        /// </summary>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc7539#section-2.1
        /// </remarks>
        [TestMethod]
        public void BitMath()
        {
            // a is unused
            //uint a = 0x11111111;
            uint b = 0x01020304;
            uint c = 0x77777777;
            uint d = 0x01234567;

            unchecked
            {
                c += d;
            }
            Assert.AreEqual<uint>(0x789abcde, c, "c sum");

            unchecked
            {
                b ^= c;
            }
            Assert.AreEqual<uint>(0x7998bfda, b, "b xor");

            unchecked
            {
                b = ByteHelper.RotateLeft(b, 7);
            }
            Assert.AreEqual<uint>(0xcc5fed3c, b, "b leftshift");
        }

        /// <summary>
        /// Test vector for the quarter round algorithm, from the spec.
        /// </summary>
        /// <remarks>
        /// https, 0x//tools.ietf.org/html/rfc7539#section-2.1.1
        /// </remarks>
        [TestMethod]
        public void QuarterRoundTestVector()
        {
            uint[] input = new uint[]
            {
                0x11111111,
                0x01020304,
                0x9b8d6f43,
                0x01234567
            };

            ChaCha20.QuarterRound(input);
            
            Assert.AreEqual<uint>(0xea2a92f4, input[0], "a");
            Assert.AreEqual<uint>(0xcb1cf8ce, input[1], "b");
            Assert.AreEqual<uint>(0x4581472e, input[2], "c");
            Assert.AreEqual<uint>(0x5881c4bb, input[3], "d");
        }

        /// <summary>
        /// Test vector for the block function algorithm, from the spec.
        /// </summary>
        /// <remarks>
        /// https, 0x//tools.ietf.org/html/rfc7539#section-2.3.2
        /// </remarks>
        [TestMethod]
        public void BlockTestVector()
        {
            byte[] key = new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f
            };

            byte[] nonce = new byte[]
            {
                0x00, 0x00, 0x00, 0x09,
                0x00, 0x00, 0x00, 0x4a,
                0x00, 0x00, 0x00, 0x00
            };

            uint[] initialState = ChaCha20.ConstructState(key, nonce, 1);
            uint[] expectedState = new uint[]
            {
               0x61707865, 0x3320646e, 0x79622d32, 0x6b206574,
               0x03020100, 0x07060504, 0x0b0a0908, 0x0f0e0d0c,
               0x13121110, 0x17161514, 0x1b1a1918, 0x1f1e1d1c,
               0x00000001, 0x09000000, 0x4a000000, 0x00000000
            };
            for (int i = 0; i < expectedState.Length; i++)
            {
                Assert.AreEqual(expectedState[i], initialState[i], $"initialState[{i}]");
            }

            byte[] output = ChaCha20.Block(initialState);

            byte[] expectedOutput = new byte[]
            {
                0x10, 0xf1, 0xe7, 0xe4, 0xd1, 0x3b, 0x59, 0x15,
                0x50, 0x0f, 0xdd, 0x1f, 0xa3, 0x20, 0x71, 0xc4,
                0xc7, 0xd1, 0xf4, 0xc7, 0x33, 0xc0, 0x68, 0x03,
                0x04, 0x22, 0xaa, 0x9a, 0xc3, 0xd4, 0x6c, 0x4e,
                0xd2, 0x82, 0x64, 0x46, 0x07, 0x9f, 0xaa, 0x09,
                0x14, 0xc2, 0xd7, 0x05, 0xd9, 0x8b, 0x02, 0xa2,
                0xb5, 0x12, 0x9c, 0xd1, 0xde, 0x16, 0x4e, 0xb9,
                0xcb, 0xd0, 0x83, 0xe8, 0xa2, 0x50, 0x3c, 0x4e
            };

            for (int i = 0; i < expectedOutput.Length; i++)
            {
                Assert.AreEqual(expectedOutput[i], output[i], $"output[{i}]");
            }
        }

        /// <summary>
        /// Test vector for the entire cipher; in particular the generated keystream.
        /// </summary>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc7539#section-2.4.2
        /// </remarks>
        [TestMethod]
        public void KeyStreamTestVector()
        {
            byte[] key = new byte[]
            {
                0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
                0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f
            };

            byte[] nonce = new byte[]
            {
                0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x4a,
                0x00, 0x00, 0x00, 0x00
            };

            ChaCha20 cipher = new ChaCha20(key, nonce, 1);
            byte[] actualKeyBytes = cipher.GetBytes(114);

            byte[] expectedKeyBytes = new byte[]
            {
                0x22, 0x4f, 0x51, 0xf3, 0x40, 0x1b, 0xd9, 0xe1,
                0x2f, 0xde, 0x27, 0x6f, 0xb8, 0x63, 0x1d, 0xed,
                0x8c, 0x13, 0x1f, 0x82, 0x3d, 0x2c, 0x06, 0xe2,
                0x7e, 0x4f, 0xca, 0xec, 0x9e, 0xf3, 0xcf, 0x78,
                0x8a, 0x3b, 0x0a, 0xa3, 0x72, 0x60, 0x0a, 0x92,
                0xb5, 0x79, 0x74, 0xcd, 0xed, 0x2b, 0x93, 0x34,
                0x79, 0x4c, 0xba, 0x40, 0xc6, 0x3e, 0x34, 0xcd,
                0xea, 0x21, 0x2c, 0x4c, 0xf0, 0x7d, 0x41, 0xb7,
                0x69, 0xa6, 0x74, 0x9f, 0x3f, 0x63, 0x0f, 0x41,
                0x22, 0xca, 0xfe, 0x28, 0xec, 0x4d, 0xc4, 0x7e,
                0x26, 0xd4, 0x34, 0x6d, 0x70, 0xb9, 0x8c, 0x73,
                0xf3, 0xe9, 0xc5, 0x3a, 0xc4, 0x0c, 0x59, 0x45,
                0x39, 0x8b, 0x6e, 0xda, 0x1a, 0x83, 0x2c, 0x89,
                0xc1, 0x67, 0xea, 0xcd, 0x90, 0x1d, 0x7e, 0x2b,
                0xf3, 0x63
            };

            for (int i = 0; i < expectedKeyBytes.Length; i++)
            {
                Assert.AreEqual(expectedKeyBytes[i], actualKeyBytes[i], $"key byte[{i}]");
            }
        }
    }
}
