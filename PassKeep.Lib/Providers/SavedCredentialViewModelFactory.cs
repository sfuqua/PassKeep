// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.ViewModels;
using System;
using System.Threading.Tasks;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// A helper for constructing activated <see cref="SavedCredentialsViewModel"/> instances.
    /// </summary>
    public sealed class SavedCredentialViewModelFactory : ISavedCredentialsViewModelFactory
    {
        private readonly ICredentialStorageProvider credentialProvider;

        public SavedCredentialViewModelFactory(ICredentialStorageProvider credentialProvider)
        {
            this.credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
        }

        /// <summary>
        /// Asynchronously constructs and activates an 
        /// <see cref="ISavedCredentialsViewModel"/>.
        /// </summary>
        /// <returns>A task that completes when the ViewModel is ready for use.</returns>
        public async Task<ISavedCredentialsViewModel> AssembleAsync()
        {
            ISavedCredentialsViewModel viewModel = new SavedCredentialsViewModel(
                this.credentialProvider
            );

            await viewModel.ActivateAsync();
            return viewModel;
        }
    }
}
