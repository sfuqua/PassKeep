// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
            this.credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            this.credentialViewModelFactory = credentialViewModelFactory ?? throw new ArgumentNullException(nameof(credentialViewModelFactory));
            this.candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
            this.credential = credential ?? throw new ArgumentNullException(nameof(credential));
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
            return this.credentialProvider.TryStoreRawKeyAsync(this.candidate.File, this.credential);
        }
    }
}
