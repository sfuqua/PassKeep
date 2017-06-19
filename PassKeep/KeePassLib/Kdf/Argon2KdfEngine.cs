using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using PassKeep.KeePassLib.Crypto;

namespace PassKeep.KeePassLib.Kdf
{
    public sealed class Argon2KdfEngine : IKdfEngine
    {
        private readonly Argon2Parameters algoParams;

        /// <summary>
        /// Initializes the engine with the given parameters.
        /// </summary>
        /// <param name="algoParams">The parameters to use for Argon2.</param>
        public Argon2KdfEngine(Argon2Parameters algoParams)
        {
            if (algoParams == null)
            {
                throw new ArgumentNullException("algoParams");
            }

            this.algoParams = algoParams;
        }

        /// <summary>
        /// Asynchronously transforms the user's key.
        /// </summary>
        /// <param name="rawKey">The key to transform.</param>
        /// <param name="token">Token used to cancel the transform task.</param>
        /// <returns>The transformed key.</returns>
        public async Task<IBuffer> TransformKeyAsync(IBuffer rawKey, CancellationToken token)
        {
            if (rawKey == null)
            {
                throw new ArgumentNullException("rawKey");
            }

            Argon2d instance = this.algoParams.CreateArgonInstance(rawKey.ToArray());

            byte[] buffer = new byte[instance.TagLength];
            await instance.HashAsync(buffer, token);

            return buffer.AsBuffer();
        }
    }
}
