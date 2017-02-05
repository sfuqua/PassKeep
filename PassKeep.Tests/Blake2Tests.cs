using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.KeePass.Crypto;
using System.Linq;
using System.Text;

namespace PassKeep.Tests
{
    /// <summary>
    /// Test cases from the BLAKE2 spec:
    /// <see cref="https://tools.ietf.org/html/rfc7693#appendix-A"/>.
    /// </summary>
    [TestClass]
    public class Blake2Tests
    {
        /// <summary>
        /// Basic "abc" test vector from the BLAKE2b spec.
        /// </summary>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc7693#appendix-A
        /// </remarks>
        [TestMethod]
        public void Blake2Hash()
        {
            CheckHash(
                Encoding.ASCII.GetBytes("abc"),
                new byte[0],
                new byte[64] {
                    0xBA, 0x80, 0xA5, 0x3F, 0x98, 0x1C, 0x4D, 0x0D, 0x6A, 0x27, 0x97, 0xB6, 0x9F, 0x12, 0xF6, 0xE9,
                    0x4C, 0x21, 0x2F, 0x14, 0x68, 0x5A, 0xC4, 0xB7, 0x4B, 0x12, 0xBB, 0x6F, 0xDB, 0xFF, 0xA2, 0xD1,
                    0x7D, 0x87, 0xC5, 0x39, 0x2A, 0xAB, 0x79, 0x2D, 0xC2, 0x52, 0xD5, 0xDE, 0x45, 0x33, 0xCC, 0x95,
                    0x18, 0xD3, 0x8A, 0xA8, 0xDB, 0xF1, 0x92, 0x5A, 0xB9, 0x23, 0x86, 0xED, 0xD4, 0x00, 0x99, 0x23
                }
            );
        }

        /// <summary>
        /// One of a few "arbitrary" BLAKE2b hash test vectors.
        /// It is copied from the BLAKE2 reference implementation (CC0 license), here:
        /// <see cref="https://github.com/BLAKE2/BLAKE2/blob/master/testvectors/blake2b-kat.txt"/>.
        /// </summary>
        [TestMethod]
        public void Blake2Arbitrary1()
        {
            CheckHash(
                new byte[1],
                Enumerable.Range(0, 64).Select(i => (byte)i).ToArray(),
                new byte[64] {
                    0x96, 0x1f, 0x6d, 0xd1, 0xe4, 0xdd, 0x30, 0xf6, 0x39, 0x01, 0x69, 0x0c, 0x51, 0x2e, 0x78, 0xe4, 0xb4,
                    0x5e, 0x47, 0x42, 0xed, 0x19, 0x7c, 0x3c, 0x5e, 0x45, 0xc5, 0x49, 0xfd, 0x25, 0xf2, 0xe4, 0x18, 0x7b,
                    0x0b, 0xc9, 0xfe, 0x30, 0x49, 0x2b, 0x16, 0xb0, 0xd0, 0xbc, 0x4e, 0xf9, 0xb0, 0xf3, 0x4c, 0x70, 0x03,
                    0xfa, 0xc0, 0x9a, 0x5e, 0xf1, 0x53, 0x2e, 0x69, 0x43, 0x02, 0x34, 0xce, 0xbd
                }
            );
        }

        /// <summary>
        /// One of a few "arbitrary" BLAKE2b hash test vectors.
        /// It is copied from the BLAKE2 reference implementation (CC0 license), here:
        /// <see cref="https://github.com/BLAKE2/BLAKE2/blob/master/testvectors/blake2b-kat.txt"/>.
        /// </summary>
        [TestMethod]
        public void Blake2Arbitrary2()
        {
            byte[] key = Enumerable.Range(0, 64).Select(i => (byte)i).ToArray();

            CheckHash(
                Enumerable.Range(0, 255).Select(i => (byte)i).ToArray(),
                Enumerable.Range(0, 64).Select(i => (byte)i).ToArray(),
                new byte[64] {
                    0x14, 0x27, 0x09, 0xd6, 0x2e, 0x28, 0xfc, 0xcc, 0xd0, 0xaf, 0x97, 0xfa, 0xd0, 0xf8, 0x46, 0x5b, 0x97,
                    0x1e, 0x82, 0x20, 0x1d, 0xc5, 0x10, 0x70, 0xfa, 0xa0, 0x37, 0x2a, 0xa4, 0x3e, 0x92, 0x48, 0x4b, 0xe1,
                    0xc1, 0xe7, 0x3b, 0xa1, 0x09, 0x06, 0xd5, 0xd1, 0x85, 0x3d, 0xb6, 0xa4, 0x10, 0x6e, 0x0a, 0x7b, 0xf9,
                    0x80, 0x0d, 0x37, 0x3d, 0x6d, 0xee, 0x2d, 0x46, 0xd6, 0x2e, 0xf2, 0xa4, 0x61
                }
            );
        }

        /// <summary>
        /// Helper to validate a BLAKE2 hash.
        /// </summary>
        /// <param name="data">Data to hash.</param>
        /// <param name="key">Key to hash.</param>
        /// <param name="expectedHash">Expected value.</param>
        private void CheckHash(byte[] data, byte[] key, byte[] expectedHash)
        {
            ulong[][] blocks = Blake2.GetDataBlocks(data, key);
            byte[] hash = Blake2.Hash(blocks, data.Length, (byte)key.Length, 64);

            for (int i = 0; i < 64; i++)
            {
                Assert.AreEqual(expectedHash[i], hash[i], $"hash byte #{i}");
            }
        }
    }
}
