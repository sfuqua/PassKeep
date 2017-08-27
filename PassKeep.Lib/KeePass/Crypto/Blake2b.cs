// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Infrastructure;
using System;
using static PassKeep.Lib.Util.ByteHelper;

namespace PassKeep.Lib.KeePass.Crypto
{
    /// <summary>
    /// An implementation of the BLAKE2b hashing function
    /// specified here: <see cref="https://tools.ietf.org/html/rfc7693"/>.
    /// </summary>
    public class Blake2b
    {
        /// <summary>
        /// The BLAKE2b 8-word IV.
        /// </summary>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc7693#section-2.6
        /// https://tools.ietf.org/html/rfc7693#appendix-C
        /// </remarks>
        public static readonly ulong[] InitializationVector = new ulong[8]
        {
           0x6A09E667F3BCC908, 0xBB67AE8584CAA73B,
           0x3C6EF372FE94F82B, 0xA54FF53A5F1D36F1,
           0x510E527FADE682D1, 0x9B05688C2B3E6C1F,
           0x1F83D9ABFB41BD6B, 0x5BE0CD19137E2179
        };

        /// <summary>
        /// The BLAKE2b 12-round message schedule, SIGMA.
        /// </summary>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc7693#section-2.7
        /// </remarks>
        public static readonly byte[,] Sigma = new byte[12, 16]
        {           
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            { 14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3 },
            { 11, 8, 12, 0, 5, 2, 15, 13, 10, 14, 3, 6, 7, 1, 9, 4 },
            { 7, 9, 3, 1, 13, 12, 11, 14, 2, 6, 5, 10, 4, 0, 15, 8 },
            { 9, 0, 5, 7, 2, 4, 10, 15, 14, 1, 11, 12, 6, 8, 3, 13 },
            { 2, 12, 6, 10, 0, 11, 8, 3, 4, 13, 7, 5, 15, 14, 1, 9 },
            { 12, 5, 1, 15, 14, 13, 4, 10, 0, 7, 6, 3, 9, 2, 8, 11 },
            { 13, 11, 7, 14, 12, 1, 3, 9, 5, 0, 15, 4, 8, 6, 2, 10 },
            { 6, 15, 14, 9, 11, 3, 0, 8, 12, 2, 13, 7, 1, 4, 10, 5 },
            { 10, 2, 8, 4, 7, 6, 1, 5, 15, 11, 9, 14, 3, 12, 13, 0 },
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 },
            { 14, 10, 4, 8, 9, 15, 13, 6, 1, 12, 0, 2, 11, 7, 5, 3 }
        };

        /// <summary>
        /// Given a message of arbitrary length and a key, returns the BLAKE2
        /// padded blocks that should be compressed.
        /// </summary>
        /// <param name="message">The message to hash.</param>
        /// <param name="key">Optional key.</param>
        /// <returns>The padded 16-word blocks to be passed to the BLAKE2 hash function.</returns>
        public static ulong[][] GetDataBlocks(byte[] message, byte[] key)
        {
            ulong[] keyBlock = null;

            // 8 bytes per ulong, 16 ulongs per block
            // 128 bytes per block
            if (key != null && key.Length > 0)
            {
                if (key.Length > 64)
                {
                    throw new ArgumentOutOfRangeException(nameof(key), "Key is limited to 64 bytes");
                }

                keyBlock = GetSingleBlock(key, 0, out int keyBytes);
                Dbg.Assert(keyBytes == key.Length);
            }

            int fullDataBlocks = message.Length / 128;
            int leftoverDataBytes = message.Length % 128;
            bool extraDataBlock = leftoverDataBytes > 0;

            int totalBlocks = fullDataBlocks + (extraDataBlock ? 1 : 0) + (keyBlock != null ? 1 : 0);
            ulong[][] blocks = new ulong[totalBlocks][];

            // First block is the key block, if it exists
            int blockIndex = 0;
            if (keyBlock != null)
            {
                blocks[blockIndex++] = keyBlock;
            }

            // Populate the full data blocks
            int messageOffset = 0;
            for (int i = 0; i < fullDataBlocks; i++)
            {
                blocks[blockIndex++] = GetSingleBlock(message, messageOffset, out int bytesRead);
                Dbg.Assert(bytesRead == 128);

                messageOffset += bytesRead;
            }

            // Populate the remaining data block
            if (extraDataBlock)
            {
                blocks[totalBlocks - 1] = GetSingleBlock(message, messageOffset, out int bytesRead);
                Dbg.Assert(bytesRead == leftoverDataBytes);
            }

            return blocks;
        }

        /// <summary>
        /// Helper to get a single "block" (16 words) from a data buffer.
        /// If the data buffer does not contain 128 bytes, it is padded with zeros.
        /// </summary>
        /// <param name="buffer">The buffer to pad.</param>
        /// <param name="bufferOffset">Offset into <paramref name="buffer"/> to start reading.</param>
        /// <param name="bytesUsed">[Out] The number of bytes read from <paramref name="buffer"/>.</param>
        /// <returns>A padded block.</returns>
        public static ulong[] GetSingleBlock(byte[] buffer, int bufferOffset, out int bytesUsed)
        {
            ulong[] block = new ulong[16];
            int bytesRemaining = buffer.Length - bufferOffset;

            if (bytesRemaining >= 128)
            {
                // We have an entire block remaining in this buffer, easy!
                bytesUsed = 128;
                for (int i = 0; i < 16; i++)
                {
                    block[i] = BufferToLittleEndianUInt64(buffer, bufferOffset);
                    bufferOffset += 8;
                }
            }
            else
            {
                // We don't have an entire block and will need to pad.
                bytesUsed = bytesRemaining;
                int fullWords = bytesRemaining / 8;
                for (int i = 0; i < fullWords; i++)
                {
                    block[i] = BufferToLittleEndianUInt64(buffer, bufferOffset);
                    bufferOffset += 8;
                }

                // We might have less than a full word of bytes remaining which we need to pad.
                int partialWordBytes = bytesRemaining % 8;
                byte[] word = new byte[8];
                Array.Copy(buffer, bufferOffset, word, 0, partialWordBytes);

                block[fullWords] = BufferToLittleEndianUInt64(word, 0);

                // Any remaining words can be ignored since they'll default to 0 anyway
            }

            return block;
        }

        /// <summary>
        /// Generates a BLAKE2b hash.
        /// </summary>
        /// <param name="data">Padded 16-word data blocks - if key exists, it is data[0].</param>
        /// <param name="inputBytesHigh">The high word of the </param>
        /// <param name="inputBytesLow"></param>
        /// <param name="keyBytes"></param>
        /// <param name="hashBytes"></param>
        /// <returns></returns>
        public static byte[] Hash(ulong[][] data, int inputBytes, byte keyBytes, byte hashBytes)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (inputBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(inputBytes));
            }

            if (keyBytes > 64)
            {
                throw new ArgumentOutOfRangeException(nameof(keyBytes));
            }

            if (hashBytes == 0 || hashBytes > 64)
            {
                throw new ArgumentOutOfRangeException(nameof(hashBytes));
            }

            ulong[] state = new ulong[8];
            Array.Copy(InitializationVector, state, 8);

            // This implementation only supports the default parameter block (which gets XOR'd with
            // the IV to form the initial state).
            uint parameterBlock0 = 0x01010000 | (uint)(keyBytes << 8) | hashBytes;
            state[0] ^= parameterBlock0;

            // Process all but the last block
            for (int i = 0; i < data.Length - 1; i++)
            {
                // Currently intentionally failing overflow and not using high bytes
                Compress(state, data[i], 0, checked(((ulong)i + 1UL) * 128UL), false);
            }

            // Final block
            ulong bitOffset = (ulong)inputBytes;
            if (keyBytes > 0)
            {
                bitOffset += 128UL;
            }

            Compress(state, data[data.Length - 1], 0, bitOffset, true);

            byte[] hash = new byte[hashBytes];
            byte[] stateBytes = new byte[8];
            int stateOffset = 0;

            for (int i = 0; i < hashBytes; i++)
            {
                int byteOffset = i % 8;
                if (byteOffset == 0)
                {
                    GetLittleEndianBytes(state[stateOffset++], stateBytes);
                }

                hash[i] = stateBytes[byteOffset];
            }

            return hash;
        }

        /// <summary>
        /// The BLAKE2b "mixing function", G, mixes <paramref name="x"/> and <paramref name="y"/>
        /// into the words indexed by the other parameters in the vector <paramref name="state"/>.
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
        public static void Mix(ulong[] state, int ai, int bi, int ci, int di, ulong x, ulong y)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            unchecked
            {
                state[ai] = state[ai] + state[bi] + x;
                state[di] = RotateRight(state[di] ^ state[ai], 32);
                state[ci] = state[ci] + state[di];
                state[bi] = RotateRight(state[bi] ^ state[ci], 24);
                state[ai] = state[ai] + state[bi] + y;
                state[di] = RotateRight(state[di] ^ state[ai], 16);
                state[ci] = state[ci] + state[di];
                state[bi] = RotateRight(state[bi] ^ state[ci], 63);
            }
        }

        /// <summary>
        /// The BLAKE2b "compression function", F, returns a new state vector given
        /// an existing state vector, a message block vector, a bit offset counter "t",
        /// and a final block indicator flag.
        /// </summary>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc7693#section-3.2
        /// </remarks>
        /// <param name="state"></param>
        /// <param name="block"></param>
        /// <param name="tHigh"></param>
        /// <param name="tLow"></param>
        /// <param name="finalBlock"></param>
        public static void Compress(ulong[] state, ulong[] block, ulong tHigh, ulong tLow, bool finalBlock)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.Length != 8)
            {
                throw new ArgumentException("state must be 8 words long", nameof(state));
            }

            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            if (block.Length != 16)
            {
                throw new ArgumentException("block must be 16 words long", nameof(block));
            }

            ulong[] workingVector = new ulong[16];
            Array.Copy(state, workingVector, 8);
            Array.Copy(InitializationVector, 0, workingVector, 8, 8);

            workingVector[12] ^= tLow;
            workingVector[13] ^= tHigh;

            // For the final block, invert all bits
            if (finalBlock)
            {
                workingVector[14] ^= 0xFFFFFFFFFFFFFFFF;
            }

            // Mix the working vector according to the message schedule, sigma
            for (int i = 0; i < 12; i++)
            {
                Mix(workingVector, 0, 4,  8, 12, block[Sigma[i,  0]], block[Sigma[i,  1]]);
                Mix(workingVector, 1, 5,  9, 13, block[Sigma[i,  2]], block[Sigma[i,  3]]);
                Mix(workingVector, 2, 6, 10, 14, block[Sigma[i,  4]], block[Sigma[i,  5]]);
                Mix(workingVector, 3, 7, 11, 15, block[Sigma[i,  6]], block[Sigma[i,  7]]);

                Mix(workingVector, 0, 5, 10, 15, block[Sigma[i,  8]], block[Sigma[i,  9]]);
                Mix(workingVector, 1, 6, 11, 12, block[Sigma[i, 10]], block[Sigma[i, 11]]);
                Mix(workingVector, 2, 7,  8, 13, block[Sigma[i, 12]], block[Sigma[i, 13]]);
                Mix(workingVector, 3, 4,  9, 14, block[Sigma[i, 14]], block[Sigma[i, 15]]);
            }

            // Generate the new state by XORing both halves of the working vector with the current
            // state
            for (int i = 0; i < 8; i++)
            {
                state[i] ^= workingVector[i] ^ workingVector[i + 8];
            }
        }
    }
}
