using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Util;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace PassKeep.Lib.KeePass.Rng
{
    /// <summary>
    /// An implementation (based on the spec) of the Salsa20 cipher.
    /// </summary>
    public class Salsa20 : AbstractRng
    {
        // This is a KeePass-specific initialization vector
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

        public Salsa20(byte[] seed)
            : base(seed)
        {
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

        public override IRandomNumberGenerator Clone()
        {
            return new Salsa20(Seed);
        }

        public override RngAlgorithm Algorithm
        {
            get { return RngAlgorithm.Salsa20; }
        }

        #region Implementation
        private static UInt32 rotL(UInt32 x, int c)
        {
            return (x << c) | (x >> (32 - c));
        }

        public static UInt32[] QuarterRound(UInt32[] y, int offset = 0)
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

        public static UInt32[] RowRound(UInt32[] y)
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
                UInt32[] output = QuarterRound(input);
                for (int j = 0; j < 4; j++)
                {
                    z[offsets[i][j]] = output[j];
                }
            }

            return z;
        }

        public static UInt32[] ColumnRound(UInt32[] x)
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
                UInt32[] output = QuarterRound(toRound, 0);

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

            return RowRound(ColumnRound(x));
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
                    ByteHelper.Xor(lastSet, setIndex, m, mOffset, toCopy);
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

#endregion Implementation
    }
}
