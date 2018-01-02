// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PassKeep.Lib.KeePass.Rng
{
    public class ArcFourVariant : AbstractRng
    {
        private byte[] state;
        private byte indexA, indexB;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ArcFourVariant(byte[] seed)
            : base(seed)
        {
            this.state = Enumerable.Range(0, 256).Cast<byte>().ToArray();
            this.indexA = 0;
            this.indexB = 0;

            unchecked
            {
                byte j = 0;
                for (int i = 0; i < this.state.Length; i++)
                {
                    j += (byte)(this.state[i] + seed[i % seed.Length]);

                    byte temp = this.state[0];
                    this.state[0] = this.state[j];
                    this.state[j] = temp;
                }
            }

            GetBytes(512);
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
                    this.indexA++;
                    this.indexB += this.state[this.indexA];

                    byte temp = this.state[this.indexA];
                    this.state[this.indexA] = this.state[this.indexB];
                    this.state[this.indexB] = temp;

                    temp = (byte)(this.state[this.indexA] + this.state[this.indexB]);
                    bytes[i] = this.state[temp];
                }
            }

            return bytes;
        }

        public override IRandomNumberGenerator Clone()
        {
            return new ArcFourVariant(Seed);
        }

        public override RngAlgorithm Algorithm
        {
            get { return RngAlgorithm.ArcFourVariant; }
        }
    }
}
