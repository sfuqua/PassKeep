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

        private int parallelism;
        private int tagLength;
        private int memorySize;
        private int iterations;
        private byte[] secretValue;
        private byte[] associatedData;

        /// <summary>
        /// Initializes an engine that can be used to exercise the Argon2d algorithm.
        /// </summary>
        /// <param name="message">The password, from 0 to 2^32 - 1 bytes.</param>
        /// <param name="nonce">The salt, from 8 to 2^32 -1 bytes. 16 recommended.</param>
        public Argon2d(byte[] message, byte[] nonce)
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

        public static void Permute(ushort[] state)
        {
            
        }
    }
}
