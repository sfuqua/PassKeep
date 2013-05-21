using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;

namespace PassKeep.KeePassLib
{
    public class Salsa20Rng : KeePassRng
    {
        private static readonly byte[] keePassIv = new byte[] 
        {
            0xE8, 0x30, 0x09, 0x4B, 0x97, 0x20, 0x5D, 0x2A 
        };
        private byte[] nonce = new byte[16];
        private byte[] lowerKey = new byte[16];
        private byte[] upperKey = new byte[16];

        private byte[] lastSet = new byte[64];
        private int setIndex = 64;

        private UInt32[] state = new UInt32[16];
        private static readonly byte[][] sigma = new byte[4][]
        {
            new byte[4] {101, 120, 112, 97},
            new byte[4] {110, 100, 32, 51},
            new byte[4] {50, 45, 98, 121},
            new byte[4] {116, 101, 32, 107}
        };
        private static readonly byte[][] tau = new byte[4][]
        {
            new byte[4] {101, 120, 112, 97},
            new byte[4] {110, 100, 32, 49},
            new byte[4] {54, 45, 98, 121},
            new byte[4] {116, 101, 32, 107}
        };

        public Salsa20Rng(byte[] seed)
            : base(seed)
        {
#if DEBUG
            tests();
#endif

            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var hash = sha256.CreateHash();
            hash.Append(CryptographicBuffer.CreateFromByteArray(seed));

            byte[] key = hash.GetValueAndReset().ToArray();
            for (int i = 0; i < 4; i++)
            {
                if (i < 2)
                {
                    UInt32 subIv = littleendian(keePassIv, i * 4);
                    Array.Copy(littleendianInverse(subIv), 0, nonce, i * 4, 4);
                }

                UInt32 subkeyL = littleendian(key, i * 4);
                Array.Copy(littleendianInverse(subkeyL), 0, lowerKey, i * 4, 4);

                UInt32 subkeyU = littleendian(key, (i * 4) + 16);
                Array.Copy(littleendianInverse(subkeyU), 0, upperKey, i * 4, 4);
            }
        }

        public override KeePassRng Clone()
        {
            return new Salsa20Rng(Seed);
        }

        public static void tests()
        {
            testQuarterround(
                new uint[] { 0, 0, 0, 0 },
                new uint[] { 0, 0, 0, 0 }
            );
            testQuarterround(
                new uint[] { 1, 0, 0, 0 },
                new uint[] { 0x8008145, 0x80, 0x10200, 0x20500000 }
            );
            testQuarterround(
                new uint[] { 0, 1, 0, 0 },
                new uint[] { 0x88000100, 0x1, 0x200, 0x402000 }
            );
            testQuarterround(
                new uint[] { 0, 0, 1, 0 },
                new uint[] { 0x80040000, 0, 0x1, 0x2000 }
            );
            testQuarterround(
                new uint[] { 0, 0, 0, 1 },
                new uint[] { 0x48044, 0x80, 0x10000, 0x20100001 }
            );
            testQuarterround(
                new uint[] { 0xe7e8c006, 0xc4f9417d, 0x6479b4b2, 0x68c67137 },
                new uint[] { 0xe876d72b, 0x9361dfd5, 0xf1460244, 0x948541a3 }
            );
            testQuarterround(
                new uint[] { 0xd3917c5b, 0x55f1c407, 0x52a58a7a, 0x8f887a3b },
                new uint[] { 0x3e2f308c, 0xd90a8f36, 0x6ab2a923, 0x2883524c }
            );

            testRowround(
                new uint[] 
                {
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, 0, 0, 0
                },
                new uint[]
                {
                    0x08008145,0x00000080,0x00010200,0x20500000, 
                    0x20100001,0x00048044,0x00000080,0x00010000,
                    0x00000001,0x00002000,0x80040000,0x00000000,
                    0x00000001,0x00000200,0x00402000,0x88000100
                }
            );

            testRowround(
                new uint[]
                {
                    0x08521bd6,0x1fe88837,0xbb2aa576,0x3aa26365,
                    0xc54c6a5b,0x2fc74c2f,0x6dd39cc3,0xda0a64f6,
                    0x90a2f23d,0x067f95a6,0x06b35f61,0x41e4732e,
                    0xe859c100,0xea4d84b7,0x0f619bff,0xbc6e965a
                },
                new uint[]
                {
                    0xa890d39d,0x65d71596,0xe9487daa,0xc8ca6a86,
                    0x949d2192,0x764b7754,0xe408d9b9,0x7a41b4d1, 
                    0x3402e183,0x3c3af432,0x50669f96,0xd89ef0a8, 
                    0x0040ede5,0xb545fbce,0xd257ed4f,0x1818882d
                }
            );

            testColumnround(
                new uint[]
                {
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, 0, 0, 0,
                    1, 0, 0, 0
                },
                new uint[]
                {
                    0x10090288,0x00000000,0x00000000,0x00000000,
                    0x00000101,0x00000000,0x00000000,0x00000000, 
                    0x00020401,0x00000000,0x00000000,0x00000000,
                    0x40a04001,0x00000000,0x00000000,0x00000000
                }
            );

            testColumnround(
                new uint[]
                {
                    0x08521bd6,0x1fe88837,0xbb2aa576,0x3aa26365, 
                    0xc54c6a5b,0x2fc74c2f,0x6dd39cc3,0xda0a64f6,
                    0x90a2f23d,0x067f95a6,0x06b35f61,0x41e4732e,
                    0xe859c100,0xea4d84b7,0x0f619bff,0xbc6e965a
                },
                new uint[]
                {
                    0x8c9d190a,0xce8e4c90,0x1ef8e9d3,0x1326a71a, 
                    0x90a20123,0xead3c4f3,0x63a091a0,0xf0708d69, 
                    0x789b010c,0xd195a681,0xeb7d5504,0xa774135c,
                    0x481c2027,0x53a8e4b5,0x4c1f89c5,0x3f78c9c8
                }
            );
        }

        private static void testQuarterround(uint[] input, uint[] output)
        {
            uint[] result = quarterround(input);
            for (int i = 0; i < 4; i++)
            {
                Debug.Assert(output[i] == result[i]);
            }
        }

        private static void testRowround(uint[] input, uint[] output)
        {
            uint[] result = rowround(input);
            bool flag = false;

            for (int i = 0; i < 16; i++)
            {
                Debug.Assert(output[i] == result[i]);
                if (output[i] != result[i])
                {
                    flag = true;
                }
            }

            if (flag)
            {
                Debug.Assert(false);
            }
        }

        private static void testColumnround(uint[] input, uint[] output)
        {
            uint[] result = columnround(input);
            bool flag = false;

            for (int i = 0; i < 16; i++)
            {
                Debug.Assert(output[i] == result[i]);
                if (output[i] != result[i])
                {
                    flag = true;
                }
            }

            if (flag)
            {
                Debug.Assert(false);
            }
        }

        public override byte[] GetBytes(uint numBytes)
        {
            if (numBytes == 0)
            {
                return new byte[0];
            }

            byte[] bytes = new byte[numBytes];
            encrypt(bytes, false);
            return bytes;
        }

        private static UInt32 rotL(UInt32 x, int c)
        {
            return (x << c) | (x >> (32 - c));
        }

        private static UInt32[] quarterround(UInt32[] y, int offset = 0)
        {
            Debug.Assert(y != null);
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            Debug.Assert(y.Length >= offset + 4);
            if (y.Length < offset + 4)
            {
                throw new ArgumentException("input not long enough to quarterround", "y");
            }

            UInt32[] z = new UInt32[4];
            unchecked
            {
                z[1] = y[1 + offset] ^ rotL((y[0 + offset] + y[3 + offset]), 7);
                z[2] = y[2 + offset] ^ rotL((z[1] + y[0 + offset]), 9);
                z[3] = y[3 + offset] ^ rotL((z[2] + z[1]), 13);
                z[0] = y[0 + offset] ^ rotL((z[3] + z[2]), 18);
            }

            return z;
        }

        private static UInt32[] rowround(UInt32[] y)
        {
            Debug.Assert(y != null);
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            Debug.Assert(y.Length == 16);
            if (y.Length != 16)
            {
                throw new ArgumentException("y must be 16 words long", "y");
            }

            UInt32[] z = new UInt32[16];
            int[][] offsets = new int[4][]
            {
                new int[] {0, 1, 2, 3},
                new int[] {5, 6, 7, 4},
                new int[] {10, 11, 8, 9},
                new int[] {15, 12, 13, 14},
            };

            for (int i = 0; i < 4; i++)
            {
                UInt32[] input = new UInt32[4] 
                {
                    y[offsets[i][0]], y[offsets[i][1]], y[offsets[i][2]], y[offsets[i][3]]
                };
                UInt32[] output = quarterround(input);
                for (int j = 0; j < 4; j++)
                {
                    z[offsets[i][j]] = output[j];
                }
            }

            return z;
        }

        private static UInt32[] columnround(UInt32[] x)
        {
            Debug.Assert(x != null);
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            Debug.Assert(x.Length == 16);
            if (x.Length != 16)
            {
                throw new ArgumentException("x must be 16 words long", "x");
            }

            UInt32[] y = new UInt32[16];
            int[] offsets = new int[4] { 0, 4, 8, 12 };
            for (int i = 0; i < 4; i++)
            {
                UInt32[] toRound = new UInt32[] 
                {
                    x[offsets[0]], x[offsets[1]], x[offsets[2]], x[offsets[3]]
                };
                UInt32[] output = quarterround(toRound, 0);

                for (int j = 0; j < 4; j++)
                {
                    y[offsets[j]] = output[j];
                    offsets[j] = (offsets[j] + 5) % 16;
                }
            }

            return y;
        }

        private static UInt32[] doubleround(UInt32[] x)
        {
            Debug.Assert(x != null);
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            Debug.Assert(x.Length == 16);
            if (x.Length != 16)
            {
                throw new ArgumentException("x must be 16 words long", "x");
            }

            return rowround(columnround(x));
        }

        private static UInt32 littleendian(byte[] b, int offset)
        {
            Debug.Assert(b != null);
            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            Debug.Assert(b.Length >= offset + 4);
            if (b.Length < offset + 4)
            {
                throw new ArgumentException("input not long enough to quarterround", "b");
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt32(b, offset);
            }
            return (UInt32)(b[0] + rotL(b[1], 8) + rotL(b[2], 16) + rotL(b[3], 24));
        }

        private static byte[] littleendianInverse(UInt32 z)
        {
            return new byte[4] 
            {
                (byte)(z & 0xFF),
                (byte)((z >> 8) & 0xFF),
                (byte)((z >> 16) & 0xFF),
                (byte)((z >> 24) & 0xFF)
            };
        }

        private static byte[] salsa20(byte[] x)
        {
            Debug.Assert(x != null);
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            Debug.Assert(x.Length == 64);
            if (x.Length != 64)
            {
                throw new ArgumentException("x must be 64 bytes long", "x");
            }

            UInt32[] x_i = new UInt32[16];
            for (int i = 0; i < x_i.Length; i++)
            {
                x_i[i] = littleendian(x, i * 4);
            }

            UInt32[] z = doubleround(x_i);
            for (int i = 0; i < 9; i++)
            {
                z = doubleround(z);
            }

            byte[] retVal = new byte[64];
            for (int i = 0; i < 16; i++)
            {
                unchecked
                {
                    byte[] bytes = littleendianInverse(z[i] + x_i[i]);
                    Array.Copy(bytes, 0, retVal, i * 4, 4);
                }
            }

            return retVal;
        }

        private static byte[] salsa20(byte[] k0, byte[] k1, byte[] n)
        {
            Debug.Assert(k0 != null);
            if (k0 == null)
            {
                throw new ArgumentNullException("k0");
            }

            Debug.Assert(k0.Length == 16);
            if (k0.Length != 16)
            {
                throw new ArgumentException("k0 must be 16 bytes long", "k0");
            }

            byte[][] exp = sigma;
            if (k1 != null)
            {
                Debug.Assert(k1.Length == 16);
                if (k1.Length != 16)
                {
                    throw new ArgumentException("k1 must be 16 bytes long if defined", "k1");
                }
            }
            else
            {
                k1 = k0;
                exp = tau;
            }

            Debug.Assert(n != null);
            if (n == null)
            {
                throw new ArgumentNullException("n");
            }

            Debug.Assert(n.Length == 16);
            if (n.Length != 16)
            {
                throw new ArgumentException("n must be 16 bytes long", "n");
            }

            byte[] input = new byte[64];
            Array.Copy(exp[0], input, exp[0].Length); // Ends at 4
            Array.Copy(k0, 0, input, 4, k0.Length); // Ends at 4 + 16 = 20
            Array.Copy(exp[1], 0, input, 20, exp[1].Length); // Ends at 20 + 4 = 24
            Array.Copy(n, 0, input, 24, n.Length); // Ends at 24 + 16 = 40
            Array.Copy(exp[2], 0, input, 40, exp[2].Length); // Ends at 40 + 4 = 44
            Array.Copy(k1, 0, input, 44, k1.Length); // Ends at 44 + 16 = 60
            Array.Copy(exp[3], 0, input, 60, exp[3].Length);

            return salsa20(input);
        }

        private void encrypt(byte[] m, bool xor = true)
        {
            Debug.Assert(m != null);
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }

            int bytesRemaining = m.Length;
            int mOffset = 0;
            while (bytesRemaining > 0)
            {
                if (setIndex == 64)
                {
                    setIndex = 0;
                    lastSet = salsa20(lowerKey, upperKey, nonce);
                    for (int i = 8; i < 16; i++)
                    {
                        nonce[i]++;
                        if (nonce[i] != 0)
                            break;
                    }
                }

                int toCopy = Math.Min(bytesRemaining, 64 - setIndex);

                if (xor)
                {
                    KeePassHelper.Xor(lastSet, setIndex, m, mOffset, toCopy);
                }
                else
                {
                    Array.Copy(lastSet, setIndex, m, mOffset, toCopy);
                }

                setIndex += toCopy;
                mOffset += toCopy;
                bytesRemaining -= toCopy;
            }
            // v <-- nonce, 8 bytes
            // l <-- m.Length
            
            // i == UInt64 (i[0] == LSB)

        }

        public override KdbxHandler.RngAlgorithm Algorithm
        {
            get { return KdbxHandler.RngAlgorithm.Salsa20; }
        }
    }
}
