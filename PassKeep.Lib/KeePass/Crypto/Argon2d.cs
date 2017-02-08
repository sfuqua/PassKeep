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

        /// <summary>
        /// Number of slices per lane of the memory matrix.
        /// </summary>
        public static readonly int NumSlices = 4;

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
                    PermutationIndices[i + 8][j] = i + (j * 8);
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
        public Argon2d(byte[] message, byte[] nonce, byte[] secretValue, byte[] data,
            int parallelism, int tagLength, int memorySize, int iterations)
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

            Parallelism = parallelism;
            TagLength = tagLength;
            MemorySize = memorySize;
            Iterations = iterations;
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
            private set
            {
                if (value < MinParallelism || value > MaxParallelism)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.parallelism = value;
            }
        }

        /// <summary>
        /// Number of bytes to finally output.
        /// </summary>
        public int TagLength
        {
            get { return Math.Max(Math.Min(this.tagLength, MaxTagLength), MinTagLength); }
            private set
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
            private set
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
            private set
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
        public byte[] Hash(byte[] input, uint outBytes)
        {
            // Blake2b with hash length TagLength
            // of TagLength || input
            byte[] hashInput = new byte[4 + input.Length];
            GetLittleEndianBytes(outBytes, hashInput);
            Array.Copy(input, 0, hashInput, 4, input.Length);

            if (outBytes <= 64)
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
                int r = (int)(((outBytes - 1) / 32) - 1);
                byte[] returnValue = new byte[(r * 32) + (outBytes - (r * 32))];

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
                    (byte)(outBytes - (32 * r))
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
            Xor(this.memory, blockYOffset, blockR, 0, 1024);

            byte[] blockZ = new byte[1024];
            Array.Copy(blockR, blockZ, 1024);
            // R is viewed as an 8x8 matrix of 16-byte registers.
            // R0 .. R63
            // We permute rows, and then columns
            for (int i = 0; i < PermutationIndices.Length; i++)
            {
                Permute(blockZ, PermutationIndices[i]);
            }

            // output Z XOR R
            Xor(blockZ, 0, blockR, 0, 1024);

            return blockR;
        }

        /// <summary>
        /// Primary entrypoint of Argon2 - uses the parameters provided in the 
        /// constructor to initialize <see cref="MemorySize"/> Kb of memory and
        /// then performs <see cref="Iterations"/> passes over it to ultimately
        /// generate a hash of <see cref="TagLength"/> bytes.
        /// </summary>
        /// <param name="output">The buffer to fill with the output.</param>
        /// <returns>A task that completes when the hash is ready.</returns>
        public Task HashAsync(byte[] output)
        {
            if (this.memory != null)
            {
                throw new InvalidOperationException();
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (output.Length != TagLength)
            {
                throw new ArgumentException(nameof(output));
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
                int blockAddress = i * bytesPerRow;

                GetLittleEndianBytes(0, hashParams, h0.Length);
                GetLittleEndianBytes((uint)i, hashParams, h0.Length + 4);

                byte[] blockValue = Hash(hashParams, 1024);
                Array.Copy(blockValue, 0, this.memory, blockAddress, blockValue.Length);

                GetLittleEndianBytes(1, hashParams, h0.Length);
                blockValue = Hash(hashParams, 1024);
                Array.Copy(blockValue, 0, this.memory, blockAddress + 1024, blockValue.Length);
            }

            for (int j = 2; j < numCols; j++)
            {
                for (int i = 0; i < Parallelism; i++)
                {
                    int xOffset = (i * bytesPerRow) + (1024 * (j - 1));

                    int refLane;
                    int refBlock;
                    GetRefCoordinates(i, j, 0, numCols / 4, out refLane, out refBlock);
                    int yOffset = (refLane * bytesPerRow) + (1024 * refBlock);

                    byte[] block = Compress(xOffset, yOffset);
                    Array.Copy(block, 0, this.memory, (i * bytesPerRow) + (1024 * j), 1024);
                }
            }

            // Perform remaining iterations
            for (int pass = 1; pass < Iterations; pass++)
            {
                for (int i = 0; i < Parallelism; i++)
                {
                    // B[i][0] = G(B-[i][q-1], B[i'][j']) XOR B-[i][0]
                    int xOffset = (i * bytesPerRow) + (1024 * (numCols - 1));

                    int refLane;
                    int refBlock;
                    GetRefCoordinates(i, 0, pass, numCols / 4, out refLane, out refBlock);
                    int yOffset = (refLane * bytesPerRow) + (1024 * refBlock);

                    byte[] block = Compress(xOffset, yOffset);
                    Xor(block, 0, this.memory, i * bytesPerRow, 1024);

                    for (int j = 1; j < numCols; j++)
                    {
                        xOffset = (i * bytesPerRow) * 1024 * (j - 1);
                        GetRefCoordinates(i, j, pass, numCols / 4, out refLane, out refBlock);
                        yOffset = (refLane * bytesPerRow) + (1024 * refBlock);

                        block = Compress(xOffset, yOffset);
                        Xor(block, 0, this.memory, (i * bytesPerRow) + (1024 * j), 1024);
                    }
                }
            }

            // XOR the final column to compute Bfinal; we use B[0][q-1] to 
            // accumulate this result.
            for (int i = 1; i < Parallelism ; i++)
            {
                Xor(this.memory, (i * bytesPerRow) + (1024 * (numCols - 1)),
                    this.memory, 1024 * (numCols - 1),
                    1024);
            }

            byte[] bFinal = new byte[1024];
            Array.Copy(this.memory, 1024 * (numCols - 1), bFinal, 0, 1024);
            byte[] hashedBFinal = Hash(bFinal, (uint)TagLength);
            Array.Copy(hashedBFinal, output, 1024);

            this.memory = null;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Given information about the current block, outputs the coordinates
        /// of the previous block that should go into its computation.
        /// </summary>
        /// <param name="currentLane">The lane being processed, [0, Parallelism).</param>
        /// <param name="currentBlock">The block within <paramref name="lane"/> being processed, [0, m').</param>
        /// <param name="pass">The current pass over the memory, [0, <see cref="Iterations"/>).</param>
        /// <param name="blocksPerSegment">Pre-derived number of blocks per sliced segment.</param>
        /// <param name="refLane">[Out] the lane to use.</param>
        /// <param name="refBlock">[Out] the block within <paramref name="refLane"/> to use.</param>
        private void GetRefCoordinates(int currentLane, int currentBlock, int pass, int blocksPerSegment, out int refLane, out int refBlock)
        {
            Dbg.Assert(currentBlock > 0);
            Dbg.Assert(blocksPerSegment > 0);

            // We want the absolute "block index" of the previous block,
            // and then we'll convert that to the "byte address" by multiplying
            // by 1024.
            int blocksPerLane = blocksPerSegment * NumSlices;
            int prevAddress = ((currentLane * blocksPerLane) + currentBlock - 1) * 1024;

            uint j1 = BufferToLittleEndianUInt32(this.memory, prevAddress);
            uint j2 = BufferToLittleEndianUInt32(this.memory, prevAddress + 4);

            int currentSlice = currentBlock / blocksPerSegment;
            if (currentSlice == 0)
            {
                refLane = currentLane;
            }
            else
            {
                refLane = (int)(j2 % Parallelism);
            }

            bool sameLane = refLane == currentLane;

            int referenceBlocks;

            if (pass == 0)
            {
                // On the first pass, only segments/blocks *behind* are
                // usable because the state of the rest of the memory
                // is undefined.
                referenceBlocks = currentSlice * blocksPerSegment;
                if (sameLane)
                {
                    // Add finished blocks from same lane
                    referenceBlocks += currentBlock - 1;
                }
                else if (currentBlock % blocksPerSegment == 0)
                {
                    // If this is the first block of the segment,
                    // the last block of the previous segment is off-limits.
                    referenceBlocks--;
                }
            }
            else
            {
                // On subsequent passes, segments *ahead* of the 
                // current segment are fair game as they were filled
                // in a previous pass.
                referenceBlocks = blocksPerLane - blocksPerSegment;
                if (sameLane)
                {
                    referenceBlocks += currentBlock - 1;
                }
                else if (currentBlock % blocksPerSegment == 0)
                {
                    // If this is the first block of the segment,
                    // the last block of the previous segment is off-limits.
                    referenceBlocks--;
                }
            }

            // "referenceBlocks" is now the number of blocks available to use
            // as a reference set to choose from.
            Dbg.Assert(referenceBlocks > 0);

            // We need to compute the offset into referenceBlocks that will be
            // our actual reference block.

            // This is chosen according to |R| * (1 - (j1^2) / (2^64)), or
            // the following approximation.
            ulong relativePosition = j1;
            relativePosition = (relativePosition * relativePosition) >> 32;
            relativePosition = (uint)referenceBlocks - 1 - ((uint)referenceBlocks * relativePosition >> 32);
            Dbg.Assert((int)relativePosition >= 0);
            Dbg.Assert((int)relativePosition < referenceBlocks);

            // The "anchor block" is the first block of the working set.
            // The available blocks extend forward from that block until we have
            // a set of "referenceBlocks" size, that wraps around to the previous
            // lane if needed.
            uint anchor = 0;
            if (pass > 0)
            {
                // If we're on the last slice, anchor to the beginning.
                // Otherwise, anchor to the start of the next slice.
                anchor = (currentSlice == NumSlices - 1)
                    ? 0U
                    : ((uint)currentSlice + 1U) * (uint)blocksPerSegment;
            }

            refBlock = (int)((anchor + relativePosition) % (ulong)blocksPerLane);
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
            ulong aL = state[ai] & 0xFFFFFFFF;
            ulong bL = state[bi] & 0xFFFFFFFF;
            ulong cL = state[ci] & 0xFFFFFFFF;
            ulong dL = state[di] & 0xFFFFFFFF;

            unchecked
            {
                state[ai] = state[ai] + state[bi] + 2 * aL * bL;
                aL = state[ai] & 0xFFFFFFFF;

                state[di] = RotateRight(state[di] ^ state[ai], 32);
                dL = state[di] & 0xFFFFFFFF;

                state[ci] = state[ci] + state[di] + 2 * cL * dL;
                cL = state[ci] & 0xFFFFFFFF;

                state[bi] = RotateRight(state[bi] ^ state[ci], 24);
                bL = state[bi] & 0xFFFFFFFF;

                state[ai] = state[ai] + state[bi] + 2 * aL * bL;
                aL = state[ai] & 0xFFFFFFFF;

                state[di] = RotateRight(state[di] ^ state[ai], 16);
                dL = state[di] & 0xFFFFFFFF;

                state[ci] = state[ci] + state[di] + 2 * cL * dL;
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
                BufferToLittleEndianUInt64(state, offsets[0] * 16),
                BufferToLittleEndianUInt64(state, offsets[0] * 16 + 8),
                BufferToLittleEndianUInt64(state, offsets[1] * 16),
                BufferToLittleEndianUInt64(state, offsets[1] * 16 + 8),

                BufferToLittleEndianUInt64(state, offsets[2] * 16),
                BufferToLittleEndianUInt64(state, offsets[2] * 16 + 8),
                BufferToLittleEndianUInt64(state, offsets[3] * 16),
                BufferToLittleEndianUInt64(state, offsets[3] * 16 + 8),

                BufferToLittleEndianUInt64(state, offsets[4] * 16),
                BufferToLittleEndianUInt64(state, offsets[4] * 16 + 8),
                BufferToLittleEndianUInt64(state, offsets[5] * 16),
                BufferToLittleEndianUInt64(state, offsets[5] * 16 + 8),

                BufferToLittleEndianUInt64(state, offsets[6] * 16),
                BufferToLittleEndianUInt64(state, offsets[6] * 16 + 8),
                BufferToLittleEndianUInt64(state, offsets[7] * 16),
                BufferToLittleEndianUInt64(state, offsets[7] * 16 + 8)
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
                int offset = (offsets[i / 2] * 16) + (i % 2 == 1 ? 8 : 0);
                GetLittleEndianBytes(v[i], state, offset);
            }
        }
    }
}
