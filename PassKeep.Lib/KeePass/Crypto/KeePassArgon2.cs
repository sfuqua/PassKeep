using System;
using System.Threading;
using System.Threading.Tasks;

namespace PassKeep.Lib.KeePass.Crypto
{
    /// <summary>
    /// KeePass utilizes Argon2d with limited paramaterization.
    /// This class encapsulates <see cref="Argon2d"/> with the right arguments.
    /// </summary>
    public class KeePassArgon2
    {
        private static readonly int TagLength = 32; // 256 bits
        private static readonly int SaltLength = 32; // 256 bits

        private readonly Argon2d engine;

        /// <summary>
        /// Initialize the engine with user parameters.
        /// </summary>
        /// <param name="iterations">Number of passes to run over memory.</param>
        /// <param name="memoryBlocks">Number of blocks (KiB) to use for hashing.</param>
        /// <param name="parallelism">Number of threads to hash with.</param>
        /// <param name="password">The password to hash.</param>
        /// <param name="salt">256-bit salt.</param>
        public KeePassArgon2(
            int iterations,
            int memoryBlocks,
            int parallelism,
            byte[] password,
            byte[] salt
        )
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (salt == null)
            {
                throw new ArgumentNullException(nameof(salt));
            }

            if (salt.Length != SaltLength)
            {
                throw new ArgumentException(nameof(salt));
            }

            // Initialize the Argon2 engine with validated parameters
            this.engine = new Argon2d(
                password,
                salt,
                new byte[0],
                new byte[0],
                parallelism,
                TagLength,
                memoryBlocks,
                iterations
            );
        }

        /// <summary>
        /// Asynchronously hashes the password provided.
        /// </summary>
        /// <param name="token">Token used to cancel the hash operation.</param>
        /// <returns>A task that resolves to the 256-bit hash.</returns>
        public async Task<byte[]> HashAsync(CancellationToken token)
        {
            byte[] hash = new byte[TagLength];
            await this.engine.HashAsync(hash, token);
            return hash;
        }
    }
}
