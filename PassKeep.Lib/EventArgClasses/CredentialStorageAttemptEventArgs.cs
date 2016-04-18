using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Eventing;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// A <see cref="DeferrableEventArgs"/> class that allows the View to attempt to retry
    /// a failed credential storage, potentially after prompting the user to fix the underlying
    /// issue.
    /// </summary>
    public sealed class CredentialStorageFailureEventArgs : DeferrableEventArgs
    {
        private readonly ICredentialStorageProvider credentialProvider;
        private readonly ISavedCredentialsViewModelFactory credentialViewModelFactory;
        private readonly IDatabaseCandidate candidate;
        private readonly IBuffer credential;

        public CredentialStorageFailureEventArgs(
            ICredentialStorageProvider credentialProvider,
            ISavedCredentialsViewModelFactory credentialViewModelFactory,
            IDatabaseCandidate candidate,
            IBuffer credential
        ) : base()
        {
            if (credentialProvider == null)
            {
                throw new ArgumentNullException(nameof(credentialProvider));
            }

            if (credentialViewModelFactory == null)
            {
                throw new ArgumentNullException(nameof(credentialViewModelFactory));
            }

            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            if (credential == null)
            {
                throw new ArgumentNullException(nameof(credential));
            }

            this.credentialProvider = credentialProvider;
            this.credentialViewModelFactory = credentialViewModelFactory;
            this.candidate = candidate;
            this.credential = credential;
        }

        /// <summary>
        /// Gets a ViewModel for managing saved credentials.
        /// </summary>
        /// <returns>A task that completes when the ViewModel is ready to use.</returns>
        public Task<ISavedCredentialsViewModel> GetSavedCredentialsViewModelAsync()
        {
            return this.credentialViewModelFactory.AssembleAsync();
        }

        /// <summary>
        /// Attempts to redo the storage operation.
        /// </summary>
        /// <returns>A task that represents whether the attempt was successful.</returns>
        public Task<bool> RetryStorage()
        {
            return this.credentialProvider.TryStoreRawKeyAsync(this.candidate, this.credential);
        }
    }
}
