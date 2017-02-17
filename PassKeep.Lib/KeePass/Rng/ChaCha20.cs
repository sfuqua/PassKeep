using PassKeep.Lib.Contracts.KeePass;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using static PassKeep.Lib.Util.ByteHelper;

namespace PassKeep.Lib.KeePass.Rng
{
    /// <summary>
    /// An implementation (based on the spec) of the Salsa20 cipher.
    /// <see cref="http://cr.yp.to/snuffle/spec.pdf"/>.
    /// </summary>
    public class ChaCha20 : AbstractRng
    {
        public const uint StateConstant1 = 0x61707865;
        public const uint StateConstant2 = 0x3320646e;
        public const uint StateConstant3 = 0x79622d32;
        public const uint StateConstant4 = 0x6b206574;

        private readonly byte[] key;
        private readonly byte[] initializationVector;
        private readonly uint initialCounter;

        private uint counterValue;
        private int blockOffset;
        private byte[] currentBlock;

        public ChaCha20(byte[] key, byte[] initializationVector, uint initialCounter)
            : base(key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (initializationVector == null)
            {
                throw new ArgumentNullException(nameof(initializationVector));
            }

            if (key.Length != 32)
            {
                throw new ArgumentException("Key must be 32 bytes", nameof(key));
            }

            if (initializationVector.Length != 12)
            {
                throw new ArgumentException("IV/Nonce must be 12 bytes", nameof(initializationVector));
            }

            this.key = new byte[key.Length];
            this.initializationVector = new byte[initializationVector.Length];
            this.initialCounter = initialCounter;
            this.counterValue = initialCounter;

            Array.Copy(key, this.key, key.Length);
            Array.Copy(initializationVector, this.initializationVector, initializationVector.Length);

            this.currentBlock = Block(ConstructState(this.key, this.initializationVector, this.counterValue++));
        }

        /// <summary>
        /// Fetches <paramref name="numBytes"/> bytes from the ChaCha20 keystream.
        /// </summary>
        /// <param name="numBytes">The number of bytes to generate.</param>
        /// <returns>A buffer of <paramref name="numBytes"/> bytes.</returns>
        public override byte[] GetBytes(uint numBytes)
        {
            if (numBytes == 0)
            {
                return new byte[0];
            }

            byte[] buffer = new byte[numBytes];
            int bufferOffset = 0;

            // Keep generating blocks until we satisfy the request
            while (numBytes > 0)
            {
                // How many bytes should we take from the current block?
                // If the request is large enough, take all of them, otherwise leave
                // some for the next request.
                int bytesInBlock = this.currentBlock.Length - this.blockOffset;
                int bytesToTake = (bytesInBlock > numBytes ? (int)numBytes : bytesInBlock);

                // Copy bytes from this block into the working buffer.
                Buffer.BlockCopy(this.currentBlock, this.blockOffset, buffer, bufferOffset, bytesToTake);
                bufferOffset += bytesToTake;

                // If we're at the end of a block, generate the next one.
                if (bytesToTake == bytesInBlock)
                {
                    this.blockOffset = 0;
                    this.currentBlock = Block(ConstructState(this.key, this.initializationVector, this.counterValue++));
                }
                else
                {
                    this.blockOffset += bytesToTake;
                }

                numBytes -= (uint)bytesToTake;
            }
            
            return buffer;
        }

        /// <summary>
        /// Returns a new <see cref="ChaCha20"/> instance with the same key, IV, and initial counter.
        /// </summary>
        /// <returns></returns>
        public override IRandomNumberGenerator Clone()
        {
            return new ChaCha20(this.key, this.initializationVector, this.initialCounter);
        }

        /// <summary>
        /// <see cref="RngAlgorithm.ChaCha20"/>.
        /// </summary>
        public override RngAlgorithm Algorithm
        {
            get { return RngAlgorithm.ChaCha20; }
        }

        /// <summary>
        /// The quarter round is an operation on four 32-bit unsigned integers,
        /// "a", "b", "c", and "d", and does the following:
        /// 
        /// 1. a += b; d ^= a; d leftshift 16
        /// 2. c += d; b ^= c; b leftshift 12
        /// 3. a += b; d ^= a; d leftshift 8
        /// 4. c += d; b ^= c; b leftshift 7
        /// </summary>
        /// <param name="data">An array to quarter round.</param>
        /// <param name="ai">Index to use as 'a'.</param>
        /// <param name="bi">Index to use as 'b'.</param>
        /// <param name="ci">Index to use as 'c'.</param>
        /// <param name="di">Index to use as 'd'.</param>
        /// <returns>A new array representing the quarter rounded data.</returns>
        public static void QuarterRound(uint[] data, int ai = 0, int bi = 1, int ci = 2, int di = 3)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            uint a = data[ai];
            uint b = data[bi];
            uint c = data[ci];
            uint d = data[di];
 
            unchecked
            {
                a += b; d ^= a; d = RotateLeft(d, 16);
                c += d; b ^= c; b = RotateLeft(b, 12);
                a += b; d ^= a; d = RotateLeft(d, 8);
                c += d; b ^= c; b = RotateLeft(b, 7);
            }

            data[ai] = a;
            data[bi] = b;
            data[ci] = c;
            data[di] = d;
        }

        /// <summary>
        /// The ChaCha20 function takes as inputs:
        /// * 256-bit key, composed of eight 32-bit little endian integers
        /// * 96-bit nonce, composed of three 32-bit little endian integers
        /// * 32-bit block count, a 32-bit little endian integer
        /// </summary>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc7539#section-2.3
        /// </remarks>
        /// <param name="key">Eight 32-bit little endian integers.</param>
        /// <param name="nonce">Three 32-bit little endian integers.</param>
        /// <param name="blockCount">Iterative counter.</param>
        /// <returns>16 words representing the initial ChaCha20 state.</returns>
        public static uint[] ConstructState(byte[] key, byte[] nonce, uint blockCount)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (nonce == null)
            {
                throw new ArgumentNullException(nameof(nonce));
            }

            if (key.Length != 32)
            {
                throw new ArgumentException("Key must be 32 bytes long", nameof(key));
            }

            if (nonce.Length != 12)
            {
                throw new ArgumentException("Nonce must be 12 bytes long", nameof(nonce));
            }

            uint[] state = new uint[16];
            state[0] = StateConstant1;
            state[1] = StateConstant2;
            state[2] = StateConstant3;
            state[3] = StateConstant4;

            // The next 8 words of the state are taken from the key, read as bytes in little endian order
            for (int i = 0; i < 8; i++)
            {
                state[i + 4] = BufferToLittleEndianUInt32(key, i * 4);
            }

            // Word 12 is the block count
            state[12] = blockCount;

            // The last 3 words are the nonce, as bytes in little endian order
            for (int i = 0; i < 3; i++)
            {
                state[i + 13] = BufferToLittleEndianUInt32(nonce, i * 4);
            }

            return state;
        }

        /// <summary>
        /// The ChaCha20 block function operates on a 64-byte state and transforms it
        /// with 10 sets of 8 quarterround functions.
        /// </summary>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc7539#section-2.3
        /// </remarks>
        /// <param name="state">The ChaCha20 state to operate on.</param>
        /// <returns>64 pseudo-random bytes.</returns>
        public static byte[] Block(uint[] state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.Length != 16)
            {
                throw new ArgumentException("State must be 16 words", nameof(state));
            }

            // We round a working state copy of the original, so we can add the original
            // at the end.
            uint[] workingState = new uint[state.Length];
            Array.Copy(state, workingState, state.Length);

            for (int i = 0; i < 10; i++)
            {
                InnerBlock(workingState);
            }

            // We add original state to working state and serialize
            // the words in little endian order.
            byte[] serialized = new byte[state.Length * 4];
            for (int i = 0; i < state.Length; i++)
            {
                byte[] outputBytes = GetLittleEndianBytes(unchecked(state[i] + workingState[i]));
                Array.Copy(outputBytes, 0, serialized, i * 4, 4);
            }

            return serialized;
        }

        /// <summary>
        /// The "inner block" function consists of four column rounds
        /// followed by four diagonal rounds. These are done in-place.
        /// </summary>
        /// <param name="state">The data to round.</param>
        public static void InnerBlock(uint[] state)
        {
            Dbg.Assert(state != null);
            Dbg.Assert(state.Length == 16);

            // Column rounds
            QuarterRound(state, 0, 4, 8, 12);
            QuarterRound(state, 1, 5, 9, 13);
            QuarterRound(state, 2, 6, 10, 14);
            QuarterRound(state, 3, 7, 11, 15);

            // Diagonal rounds
            QuarterRound(state, 0, 5, 10, 15);
            QuarterRound(state, 1, 6, 11, 12);
            QuarterRound(state, 2, 7, 8, 13);
            QuarterRound(state, 3, 4, 9, 14);
        }
    }
}
