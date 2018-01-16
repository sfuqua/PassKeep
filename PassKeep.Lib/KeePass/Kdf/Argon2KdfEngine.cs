// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.KeePass.Crypto;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.Kdf
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
            this.algoParams = algoParams ?? throw new ArgumentNullException(nameof(algoParams));
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
                throw new ArgumentNullException(nameof(rawKey));
            }

            Argon2d instance = this.algoParams.CreateArgonInstance(rawKey.ToArray());

            byte[] buffer = new byte[instance.TagLength];
            await instance.HashAsync(buffer, token).ConfigureAwait(false);

            return buffer.AsBuffer();
        }

        /// <summary>
        /// Asynchronously computes the number of iterations required to get to one second of runtime.
        /// </summary>
        /// <returns>A task that resolves to the desired number of Argon2 iterations.</returns>
        public async Task<ulong> ComputeOneSecondDelay()
        {
            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            Argon2d instance = this.algoParams.CreateArgonInstance(new byte[32]);
            byte[] buffer = new byte[instance.TagLength];

            int iterations = await instance.HashAsync(buffer, cts.Token, Argon2HashingMode.Indefinite).ConfigureAwait(false);
            return (ulong)iterations;
        }
    }
}
