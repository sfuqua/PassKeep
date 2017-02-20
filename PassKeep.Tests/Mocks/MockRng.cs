using PassKeep.Lib.Contracts.KeePass;
using System;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// A "random number generator" that always returns zeros.
    /// </summary>
    public class MockRng : IRandomNumberGenerator
    {
        /// <summary>
        /// Placeholder. Throws.
        /// </summary>
        public RngAlgorithm Algorithm
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// New <see cref="MockRng"/>.
        /// </summary>
        /// <returns></returns>
        public IRandomNumberGenerator Clone()
        {
            return new MockRng();
        }

        /// <summary>
        /// Empty array.
        /// </summary>
        /// <param name="numBytes"></param>
        /// <returns></returns>
        public byte[] GetBytes(uint numBytes)
        {
            return new byte[(int)numBytes];
        }
    }
}
