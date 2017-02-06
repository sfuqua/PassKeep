using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PassKeep.Lib.Util.ByteHelper;

namespace PassKeep.Lib.KeePass.Crypto
{
    /// <summary>
    /// Implementation of the Argon2d variant of the Argon2 key derivation function.
    /// <see cref="https://www.cryptolux.org/images/0/0d/Argon2.pdf"/>
    /// </summary>
    public class Argon2d
    {
        /// <summary>
        /// Comes from the spec.
        /// </summary>
        public static readonly byte VersionNumber = 0x13;

        /// <summary>
        /// From the spec; 0 for Argon2d, 1 for Argon2i.
        /// </summary>
        public static readonly byte Type = 0;

        public static readonly int MinParallelism = 1;
        public static readonly int MaxParallelism = (int)Math.Pow(2, 24) - 1;

        public static readonly int MinTagLength = 4;
        public static readonly int MaxTagLength = int.MaxValue;

        public static readonly int MaxMemorySize = int.MaxValue;

        public static readonly int MinIterations = 1;
        public static readonly int MaxIterations = int.MaxValue;

        private static readonly int[][] PermutationIndices;

        private readonly byte[] message;
        private readonly byte[] nonce;
        private readonly byte[] secretValue;
        private readonly byte[] associatedData;

        private int parallelism;
        private int tagLength;
        private int memorySize;
        private int iterations;

        private byte[] memory;

        static Argon2d()
        {
            PermutationIndices = new int[16][];

            // Rows:
            // 0 .. 7
            // 8 .. 15
            // ..
            // 56 .. 63
            for (int i = 0; i < 8; i++)
            {
                PermutationIndices[i] = new int[8];
                int baseVal = i * 8;

                for (int j = 0; j < 8; j++)
                {
                    PermutationIndices[i][j] = baseVal + j;
                }
            }

            // Columns:
            // 0, 8, 16 .. 56
            // 1, 9, 17 .. 57
            // ..
            // 7, 15, 23 .. 63
            for (int i = 0; i < 8; i++)
            {
                PermutationIndices[i + 8] = new int[8];
                for (int j = 0; j < 8; j++)
                {
                    PermutationIndices[i][j] = i + (j * 8);
                }
            }
        }

        /// <summary>
        /// Initializes an engine that can be used to exercise the Argon2d algorithm.
        /// </summary>
        /// <param name="message">The password, from 0 to 2^32 - 1 bytes.</param>
        /// <param name="nonce">The salt, from 8 to 2^32 -1 bytes. 16 recommended.</param>
        /// <param name="secretValue">Secret key for additional entropy.</param>
        /// <param name="data">Additional data; e.g. a username.</param>
        public Argon2d(byte[] message, byte[] nonce, byte[] secretValue, byte[] data)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (nonce == null)
            {
                throw new ArgumentNullException(nameof(nonce));
            }

            if (nonce.Length < 8)
            {
                throw new ArgumentOutOfRangeException(nameof(nonce), "nonce must be at least 8 bytes long");
            }

            if (secretValue == null)
            {
                secretValue = new byte[0];
            }

            if (secretValue.Length > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(secretValue), "The max key length is 32 bytes");
            }

            if (data == null)
            {
                data = new byte[0];
            }

            this.message = new byte[message.Length];
            Array.Copy(message, this.message, message.Length);

            this.nonce = new byte[nonce.Length];
            Array.Copy(nonce, this.nonce, nonce.Length);

            this.secretValue = new byte[secretValue.Length];
            Array.Copy(secretValue, this.secretValue, secretValue.Length);

            this.associatedData = new byte[data.Length];
            Array.Copy(data, this.associatedData, data.Length);
        }

        public int MinMemorySize
        {
            get { return 8 * Parallelism; }
        }

        /// <summary>
        /// How many independent, but synchronizing, computational chains can be run. 1 to 2^24 - 1.
        /// </summary>
        public int Parallelism
        {
            get { return Math.Max(Math.Min(this.parallelism, MaxParallelism), MinParallelism); }
            set
            {
                if (value < MinParallelism || value > MaxParallelism)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.parallelism = value;
            }
        }

        public int TagLength
        {
            get { return Math.Max(Math.Min(this.tagLength, MaxTagLength), MinTagLength); }
            set
            {
                if (value < MinTagLength || value > MaxTagLength)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.tagLength = value;
            }
        }

        /// <summary>
        /// Desired number of kilobytes to operate on.
        /// Actual memory used is rounded down to the nearest multiple of
        /// 4 * <see cref="Parallelism"/>.
        /// </summary>
        public int MemorySize
        {
            get { return Math.Max(Math.Min(this.memorySize, MaxMemorySize), MinMemorySize); }
            set
            {
                if (value < MinMemorySize || value > MaxMemorySize)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.memorySize = value;
            }
        }

        /// <summary>
        /// Number of passes to make over memory, to tune running time.
        /// </summary>
        public int Iterations
        {
            get { return Math.Max(Math.Min(this.iterations, MaxIterations), MinIterations); }
            set  
            {
                if (value < MinIterations || value > MaxIterations)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.iterations = value;
            }
        }

        /// <summary>
        /// Generates a buffer used for entropy based on all six static parameters
        /// appended to the four variable length parameters (prepended by their lengths).
        /// </summary>
        /// <returns>A variable-length buffer based on the algorithm's parameters.</returns>
        public byte[] GetHashParameters()
        {
            // 10 4-byte integers
            // 4 variable-length values
            int hashInputBytes = 40 + this.message.Length + this.nonce.Length
                + this.secretValue.Length + this.associatedData.Length;

            byte[] parameterBuffer = new byte[hashInputBytes];
            GetLittleEndianBytes((uint)Parallelism, parameterBuffer, 0);
            GetLittleEndianBytes((uint)TagLength, parameterBuffer, 4);
            GetLittleEndianBytes((uint)MemorySize, parameterBuffer, 8);
            GetLittleEndianBytes((uint)Iterations, parameterBuffer, 12);
            GetLittleEndianBytes((uint)VersionNumber, parameterBuffer, 16);
            GetLittleEndianBytes((uint)Type, parameterBuffer, 20);

            GetLittleEndianBytes((uint)this.message.Length, parameterBuffer, 24);
            Array.Copy(this.message, 0, parameterBuffer, 28, this.message.Length);
            int offset = 28 + this.message.Length;

            GetLittleEndianBytes((uint)this.nonce.Length, parameterBuffer, offset);
            offset += 4;
            Array.Copy(this.nonce, 0, parameterBuffer, offset, this.nonce.Length);
            offset += this.nonce.Length;

            GetLittleEndianBytes((uint)this.secretValue.Length, parameterBuffer, offset);
            offset += 4;
            Array.Copy(this.secretValue, 0, parameterBuffer, offset, this.secretValue.Length);
            offset += this.secretValue.Length;

            GetLittleEndianBytes((uint)this.associatedData.Length, parameterBuffer, offset);
            offset += 4;
            Array.Copy(this.associatedData, 0, parameterBuffer, offset, this.associatedData.Length);

            Dbg.Assert(offset + this.associatedData.Length == parameterBuffer.Length);
            return parameterBuffer;
        }

        /// <summary>
        /// Argon2d works on floor(m / 4p) * 4p 1024-byte blocks.
        /// </summary>
        /// <returns>The number of blocks to use.</returns>
        public int GetKbCount()
        {
            int pFactor = 4 * Parallelism;
            return (MemorySize / pFactor) * pFactor;
        }

        /// <summary>
        /// The Argon2 variable-length hash function wraps a hash function
        /// with x-bytes output (BLAKE2b, with x in [1, 64]).
        /// 
        /// It operates on 64-byte blocks (V) where A is defined to be the first
        /// 32 bytes of the corresponding V.
        /// </summary>
        /// <param name="input">The data to hash.</param>
        /// <returns>Hashed output.</returns>
        public byte[] Hash(byte[] input)
        {
            // Blake2b with hash length TagLength
            // of TagLength || input
            byte[] hashInput = new byte[4 + input.Length];
            GetLittleEndianBytes((uint)TagLength, hashInput);
            Array.Copy(input, 0, hashInput, 4, input.Length);

            if (TagLength <= 64)
            {
                return Blake2b.Hash(
                    Blake2b.GetDataBlocks(hashInput, null),
                    hashInput.Length,
                    0,
                    (byte)TagLength
                );
            }
            else
            {
                // r = ceil(TagLength / 32) - 2
                // q = 1 + ((x - 1) / y) will ceil(x/y) for positive nonzero
                // integers
                int r = ((TagLength - 1) / 32) - 1;
                byte[] returnValue = new byte[(r * 32) + (TagLength - (r * 32))];

                byte[] v = Blake2b.Hash(
                    Blake2b.GetDataBlocks(hashInput, null),
                    hashInput.Length,
                    0,
                    64
                );

                Array.Copy(v, returnValue, 32);

                for (int i = 1; i < r; i++)
                {
                    // OPTIMIZATION OPPORTUNITY:
                    // Reuse DataBlocks buffer
                    v = Blake2b.Hash(
                        Blake2b.GetDataBlocks(v, null),
                        64,
                        0,
                        64
                    );

                    Array.Copy(v, 0, returnValue, 32 * i, 32);
                }

                v = Blake2b.Hash(
                    Blake2b.GetDataBlocks(v, null),
                    64,
                    0,
                    (byte)(TagLength - (32 * r))
                );

                Array.Copy(v, 0, returnValue, r * 32, v.Length);
                return returnValue;
            }
        }

        /// <summary>
        /// The Argon2 compression function acts on two blocks, X and Y.
        /// They are XOR'd to create R, which is then viewed as an 8x8 matrix
        /// of 16 bytes.
        /// This matrix is permuted row-wise and then column-wise, and then 
        /// XOR'd again with R.
        /// </summary>
        /// <param name="memory">The working state to act on.</param>
        /// <param name="blockXOffset">Offset into <paramref name="memory"/> for X.</param>
        /// <param name="blockYOffset">Offset into <paramref name="memory"/> for Y.</param>
        /// <returns>A new block value representing the compressed memory.</returns>
        public byte[] Compress(int blockXOffset, int blockYOffset)
        {
            byte[] blockR = new byte[1024];
            Array.Copy(this.memory, blockXOffset, blockR, 0, 1024);
            Xor(blockR, 0, this.memory, blockYOffset, 1024);

            byte[] blockZ = new byte[1024];
            Array.Copy(blockR, blockZ, 1024);
            // R is viewed as an 8x8 matrix of 16-byte registers.
            // R0 .. R63
            // We permute rows, and then columns
            for (int i = 0; i < PermutationIndices.Length; i++)
            {
                Permute(blockZ, PermutationIndices[i]);
            }

            //output Z XOR R
            Xor(blockR, 0, blockZ, 0, 1024);

            return blockR;
        }

        public byte[] HashAsync()
        {
            if (this.memory != null)
            {
                throw new InvalidOperationException();
            }

            byte[] h0HashParams = GetHashParameters();
            byte[] h0 = Blake2b.Hash(
                Blake2b.GetDataBlocks(h0HashParams, null),
                h0HashParams.Length,
                0,
                64
            );

            int numBlocks = GetKbCount();
            int numCols = numBlocks / Parallelism;

            this.memory = new byte[numBlocks * 1024];

            /*
             * B[i][0] = H'(H0 || 0 || i)
B[i][1] = H'(H0 || 1 || i)
B[i][j] = G(B[i][j-1], B[i'][j']), 0 <= i < p, 2 <= j < q.
*/
            byte[] hashParams = new byte[72];
            Array.Copy(h0, hashParams, 64);

            int bytesPerRow = 1024 * numCols;

            for (int i = 0; i < Parallelism; i++)
            {
                int blockStart = i * bytesPerRow;

                hashParams[68] = 0;
                byte[] blockValue = Hash(hashParams);
                Array.Copy(blockValue, 0, memory, blockStart, blockValue.Length);

                hashParams[68] = 1;
                blockValue = Hash(hashParams);
                Array.Copy(blockValue, 0, memory, blockStart + 1024, blockValue.Length);
            }

            for (int i = 0; i < Parallelism; i++)
            {
                for (int j = 2; j < numCols; j++)
                {
                    int xOffset = (i * bytesPerRow) * 1024 * (j - 1);
                    int yOffset = 0;
                    byte[] block = Compress(xOffset, yOffset);
                    Array.Copy(block, 0, memory, (i * bytesPerRow) + (1024 * j), 1024);
                }
            }

            this.memory = null;
        }

        public void GetCoordinates()
        {
            // If L is the current lane:
            // We can pick any completed block from this segment,
            // or any block from other segments (only previous for pass 0)

            // Otherwise:
            // Any block from other segments

            // Implement by computing a length of the search area,
            // picking a location within that length (relative to 0),
            // then finding an anchor point and %'ing it.
        }

        /// <summary>
        /// The BLAKE2b "mixing function", G, mixes two words 'x" and "y"
        /// into the words indexed by the other parameters in the vector <paramref name="state"/>.
        /// 
        /// Argon2 modifies G such that the modular additions are combined with 64-bit multiplication 
        /// steps.
        /// </summary>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc7693#section-3.1
        /// </remarks>
        /// <param name="state">The working set containing data indexed by other params.</param>
        /// <param name="ai">First mixing index.</param>
        /// <param name="bi">Second mixing index.</param>
        /// <param name="ci">Third mixing index.</param>
        /// <param name="di">Fourth mixing index.</param>
        /// <param name="x">First word to mix.</param>
        /// <param name="y">Second word to mix.</param>
        public static void ModifiedBlake2Mix(ulong[] state, int ai, int bi, int ci, int di)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            // The Argon2 permutation function relies on the lower 32-bits of
            // the indexed values.
            uint aL = (uint)state[ai];
            uint bL = (uint)state[bi];
            uint cL = (uint)state[ci];
            uint dL = (uint)state[di];

            unchecked
            {
                state[ai] = state[ai] + state[bi] + 2 * aL * bL;
                state[di] = RotateRight(state[di] ^ state[ai], 32);
                state[ci] = state[ci] + state[di] + 2 * cL + dL;
                state[bi] = RotateRight(state[bi] ^ state[ci], 24);
                state[ai] = state[ai] + state[bi] + 2 * aL * bL;
                state[di] = RotateRight(state[di] ^ state[ai], 16);
                state[ci] = state[ci] + state[di] + 2 * cL + dL;
                state[bi] = RotateRight(state[bi] ^ state[ci], 63);
            }
        }

        /// <summary>
        /// The input of 8 16-byte registers is viewed as a matrix of
        /// 16 (4x4) 64-byte words.
        /// 
        /// S0 = v1 || v0
        /// S1 = v3 || v2
        /// S2 = v5 || v4
        /// ...
        /// S7 = v15 || v14
        /// 
        /// We then do BLAKE2b mixes on the matrix v.
        /// </summary>
        /// <param name="state">The state to permute.</param>
        /// <param name="offsets">8 offsets into <paramref name="state"/>.</param>
        public static void Permute(byte[] state, int[] offsets)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (offsets == null)
            {
                throw new ArgumentNullException(nameof(offsets));
            }

            if (offsets.Length != 8)
            {
                throw new ArgumentOutOfRangeException(nameof(offsets));
            }

            // Map the state buffer to a matrix of words
            ulong[] v = new ulong[16]
            {
                BufferToLittleEndianUInt64(state, offsets[0] + 8),
                BufferToLittleEndianUInt64(state, offsets[0]),
                BufferToLittleEndianUInt64(state, offsets[1] + 8),
                BufferToLittleEndianUInt64(state, offsets[1]),

                BufferToLittleEndianUInt64(state, offsets[2] + 8),
                BufferToLittleEndianUInt64(state, offsets[2]),
                BufferToLittleEndianUInt64(state, offsets[3] + 8),
                BufferToLittleEndianUInt64(state, offsets[3]),

                BufferToLittleEndianUInt64(state, offsets[4] + 8),
                BufferToLittleEndianUInt64(state, offsets[4]),
                BufferToLittleEndianUInt64(state, offsets[5] + 8),
                BufferToLittleEndianUInt64(state, offsets[5]),

                BufferToLittleEndianUInt64(state, offsets[6] + 8),
                BufferToLittleEndianUInt64(state, offsets[6]),
                BufferToLittleEndianUInt64(state, offsets[7] + 8),
                BufferToLittleEndianUInt64(state, offsets[7])
            };

            // Mix the matrix
            ModifiedBlake2Mix(v, 0, 4, 8, 12);
            ModifiedBlake2Mix(v, 1, 5, 9, 13);
            ModifiedBlake2Mix(v, 2, 6, 10, 14);
            ModifiedBlake2Mix(v, 3, 7, 11, 15);

            ModifiedBlake2Mix(v, 0, 5, 10, 15);
            ModifiedBlake2Mix(v, 1, 6, 11, 12);
            ModifiedBlake2Mix(v, 2, 7, 8, 13);
            ModifiedBlake2Mix(v, 3, 4, 9, 14);

            // Copy the tweaked memory back to where it should be
            for (int i = 0; i < 16; i++)
            {
                int offset = offsets[i / 2];
                if (i % 2 == 0)
                {
                    offset += 8;
                }

                GetLittleEndianBytes(v[i], state, offset);
            }
        }
    }
}
