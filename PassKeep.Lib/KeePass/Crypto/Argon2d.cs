using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PassKeep.Lib.Util.ByteHelper;

namespace PassKeep.Lib.KeePass.Crypto
{
    class Argon2d
    {
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
