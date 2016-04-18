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
            if (credentialProvider == null)
            {
                throw new ArgumentNullException(nameof(credentialProvider));
            }

            this.credentialProvider = credentialProvider;
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
