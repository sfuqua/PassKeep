// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using SariphLib.Infrastructure;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using static PassKeep.Lib.Util.ByteHelper;

namespace PassKeep.Lib.KeePass.Rng
{
    /// <summary>
    /// An implementation (based on the spec) of the Salsa20 cipher.
    /// <see cref="http://cr.yp.to/snuffle/spec.pdf"/>.
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

        private uint[] state = new uint[16];

        private static readonly byte[] sigma = Encoding.ASCII.GetBytes("expand 32-byte k");
        private static readonly byte[] tau = Encoding.ASCII.GetBytes("expand 16-byte k");

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
                    uint subIv = BufferToLittleEndianUInt32(keePassIv, i * 4);
                    Array.Copy(GetLittleEndianBytes(subIv), 0, nonce, i * 4, 4);
                }

                uint subkeyL = BufferToLittleEndianUInt32(key, i * 4);
                Array.Copy(GetLittleEndianBytes(subkeyL), 0, lowerKey, i * 4, 4);

                uint subkeyU = BufferToLittleEndianUInt32(key, (i * 4) + 16);
                Array.Copy(GetLittleEndianBytes(subkeyU), 0, upperKey, i * 4, 4);
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

        public static uint[] QuarterRound(uint[] y, int offset = 0)
        {
            Dbg.Assert(y != null);
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            Dbg.Assert(y.Length >= offset + 4);
            if (y.Length < offset + 4)
            {
                throw new ArgumentException("input not long enough to quarterround", "y");
            }

            uint[] z = new uint[4];
            unchecked
            {
                z[1] = y[1 + offset] ^ RotateLeft((y[0 + offset] + y[3 + offset]), 7);
                z[2] = y[2 + offset] ^ RotateLeft((z[1] + y[0 + offset]), 9);
                z[3] = y[3 + offset] ^ RotateLeft((z[2] + z[1]), 13);
                z[0] = y[0 + offset] ^ RotateLeft((z[3] + z[2]), 18);
            }

            return z;
        }

        public static uint[] RowRound(uint[] y)
        {
            Dbg.Assert(y != null);
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            Dbg.Assert(y.Length == 16);
            if (y.Length != 16)
            {
                throw new ArgumentException("y must be 16 words long", "y");
            }

            uint[] z = new uint[16];
            int[][] offsets = new int[4][]
            {
                new int[] {0, 1, 2, 3},
                new int[] {5, 6, 7, 4},
                new int[] {10, 11, 8, 9},
                new int[] {15, 12, 13, 14},
            };

            for (int i = 0; i < 4; i++)
            {
                uint[] input = new uint[4] 
                {
                    y[offsets[i][0]], y[offsets[i][1]], y[offsets[i][2]], y[offsets[i][3]]
                };
                uint[] output = QuarterRound(input);
                for (int j = 0; j < 4; j++)
                {
                    z[offsets[i][j]] = output[j];
                }
            }

            return z;
        }

        public static uint[] ColumnRound(uint[] x)
        {
            Dbg.Assert(x != null);
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            Dbg.Assert(x.Length == 16);
            if (x.Length != 16)
            {
                throw new ArgumentException("x must be 16 words long", "x");
            }

            uint[] y = new uint[16];
            int[] offsets = new int[4] { 0, 4, 8, 12 };
            for (int i = 0; i < 4; i++)
            {
                uint[] toRound = new uint[] 
                {
                    x[offsets[0]], x[offsets[1]], x[offsets[2]], x[offsets[3]]
                };
                uint[] output = QuarterRound(toRound, 0);

                for (int j = 0; j < 4; j++)
                {
                    y[offsets[j]] = output[j];
                    offsets[j] = (offsets[j] + 5) % 16;
                }
            }

            return y;
        }

        private static uint[] doubleround(uint[] x)
        {
            Dbg.Assert(x != null);
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            Dbg.Assert(x.Length == 16);
            if (x.Length != 16)
            {
                throw new ArgumentException("x must be 16 words long", "x");
            }

            return RowRound(ColumnRound(x));
        }

        private static byte[] salsa20(byte[] x)
        {
            Dbg.Assert(x != null);
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            Dbg.Assert(x.Length == 64);
            if (x.Length != 64)
            {
                throw new ArgumentException("x must be 64 bytes long", "x");
            }

            uint[] x_i = new uint[16];
            for (int i = 0; i < x_i.Length; i++)
            {
                x_i[i] = BufferToLittleEndianUInt32(x, i * 4);
            }

            uint[] z = doubleround(x_i);
            for (int i = 0; i < 9; i++)
            {
                z = doubleround(z);
            }

            byte[] retVal = new byte[64];
            for (int i = 0; i < 16; i++)
            {
                unchecked
                {
                    byte[] bytes = GetLittleEndianBytes(z[i] + x_i[i]);
                    Array.Copy(bytes, 0, retVal, i * 4, 4);
                }
            }

            return retVal;
        }

        private static byte[] salsa20(byte[] k0, byte[] k1, byte[] n)
        {
            Dbg.Assert(k0 != null);
            if (k0 == null)
            {
                throw new ArgumentNullException("k0");
            }

            Dbg.Assert(k0.Length == 16);
            if (k0.Length != 16)
            {
                throw new ArgumentException("k0 must be 16 bytes long", "k0");
            }

            byte[] exp = sigma;
            if (k1 != null)
            {
                Dbg.Assert(k1.Length == 16);
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

            Dbg.Assert(n != null);
            if (n == null)
            {
                throw new ArgumentNullException("n");
            }

            Dbg.Assert(n.Length == 16);
            if (n.Length != 16)
            {
                throw new ArgumentException("n must be 16 bytes long", "n");
            }

            byte[] input = new byte[64];
            Array.Copy(exp, 0, input, 0, 4); // Ends at 4
            Array.Copy(k0, 0, input, 4, k0.Length); // Ends at 4 + 16 = 20
            Array.Copy(exp, 4, input, 20, 4); // Ends at 20 + 4 = 24
            Array.Copy(n, 0, input, 24, n.Length); // Ends at 24 + 16 = 40
            Array.Copy(exp, 8, input, 40, 4); // Ends at 40 + 4 = 44
            Array.Copy(k1, 0, input, 44, k1.Length); // Ends at 44 + 16 = 60
            Array.Copy(exp, 12, input, 60, 4);

            return salsa20(input);
        }

        private void encrypt(byte[] m, bool xor = true)
        {
            Dbg.Assert(m != null);
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
                    Xor(lastSet, setIndex, m, mOffset, toCopy);
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
