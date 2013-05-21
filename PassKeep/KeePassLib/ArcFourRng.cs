﻿using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PassKeep.KeePassLib
{
    public class ArcFourRng : KeePassRng
    {
        private byte[] state;
        private byte indexA, indexB;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ArcFourRng(byte[] seed)
            : base(seed)
        {
            state = Enumerable.Range(0, 256).Cast<byte>().ToArray();
            indexA = 0;
            indexB = 0;

            unchecked
            {
                byte j = 0;
                for (int i = 0; i < state.Length; i++)
                {
                    j += (byte)(state[i] + seed[i % seed.Length]);

                    byte temp = state[0];
                    state[0] = state[j];
                    state[j] = temp;
                }
            }

            GetBytes(512);
        }

        public override KeePassRng Clone()
        {
            return new ArcFourRng(Seed);
        }

        public override byte[] GetBytes(uint numBytes)
        {
            if (numBytes == 0)
            {
                return new byte[0];
            }

            byte[] bytes = new byte[numBytes];

            unchecked
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    indexA++;
                    indexB += state[indexA];

                    byte temp = state[indexA];
                    state[indexA] = state[indexB];
                    state[indexB] = temp;

                    temp = (byte)(state[indexA] + state[indexB]);
                    bytes[i] = state[temp];
                }
            }

            return bytes;
        }

        public override KdbxHandler.RngAlgorithm Algorithm
        {
            get { return KdbxHandler.RngAlgorithm.ArcFourVariant; }
        }
    }
}
