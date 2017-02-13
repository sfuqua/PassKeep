﻿using NativeHelpers;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.Kdf
{
    public sealed class AesKdfEngine : IKdfEngine
    {
        private readonly AesParameters algoParams;

        /// <summary>
        /// Initializes the engine with the given parameters.
        /// </summary>
        /// <param name="algoParams">The parameters to use for AES key transformation.</param>
        public AesKdfEngine(AesParameters algoParams)
        {
            if (algoParams == null)
            {
                throw new ArgumentNullException(nameof(algoParams));
            }

            this.algoParams = algoParams;
        }

        /// <summary>
        /// Asynchronously transforms the user's 32-byte key using ECB AES.
        /// 
        /// Since Rijndael works on 16-byte blocks, the k is split in half and
        /// each half is encrypted separately the same number of times.
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

            if (rawKey.Length != 32)
            {
                throw new ArgumentException("Key must be 32 bytes", nameof(rawKey));
            }

            // Split the k buffer in half
            byte[] rawKeyBytes = rawKey.ToArray();
            IBuffer lowerBuffer = WindowsRuntimeBuffer.Create(rawKeyBytes, 0, 16, 16);
            IBuffer upperBuffer = WindowsRuntimeBuffer.Create(rawKeyBytes, 16, 16, 16);

            // Set up the encryption parameters
            var aes = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcb);
            var key = aes.CreateSymmetricKey(this.algoParams.Seed);
            IBuffer iv = null;

            // Run the encryption rounds in two threads (upper and lower)
            ConditionChecker checkForCancel = () => token.IsCancellationRequested;
            Task<bool> lowerTask = Task.Run(() =>
            {
                lowerBuffer = KeePassHelper.TransformKey(this.algoParams.Rounds, this.algoParams.Seed, iv, lowerBuffer, checkForCancel);
                return !checkForCancel();
            }
            );
            Task<bool> upperTask = Task.Run(() =>
            {
                upperBuffer = KeePassHelper.TransformKey(this.algoParams.Rounds, this.algoParams.Seed, iv, upperBuffer, checkForCancel);
                return !checkForCancel();
            }
            );

            // Verify the work was completed successfully
            await Task.WhenAll(lowerTask, upperTask);
            if (!(lowerTask.Result && upperTask.Result))
            {
                return null;
            }

            // Copy the units of work back into one buffer, hash it, and return.
            IBuffer transformedKey = (new byte[32]).AsBuffer();
            lowerBuffer.CopyTo(0, transformedKey, 0, 16);
            upperBuffer.CopyTo(0, transformedKey, 16, 16);

            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var hash = sha256.CreateHash();
            hash.Append(transformedKey);

            return hash.GetValueAndReset();
        }
    }
}
