using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents a ViewModel over a list of saved credentials, allowing
    /// them to be managed/deleted.
    /// </summary>
    public sealed class SavedCredentialsViewModel : AbstractViewModel, ISavedCredentialsViewModel
    {
        private readonly AsyncTypedCommand<string> deleteCredentialCommand;
        private readonly AsyncActionCommand deleteAllCommand;

        private readonly ObservableCollection<string> allCredentials;

        private readonly ICredentialStorageProvider credentialProvider;

        /// <summary>
        /// Initializes the commands and sets <see cref="CredentialTokens"/> to an empty collection.
        /// The ViewModel must be activated before use.
        /// </summary>
        /// <param name="credentialProvider">Provider to use for accessing stored credentials.</param>
        public SavedCredentialsViewModel(
            ICredentialStorageProvider credentialProvider
        )
        {
            if (credentialProvider == null)
            {
                throw new ArgumentNullException(nameof(credentialProvider));
            }

            this.credentialProvider = credentialProvider;

            this.deleteCredentialCommand = new AsyncTypedCommand<string>(
                (token) => token != null,
                async (token) =>
                {
                    if (this.allCredentials.Contains(token))
                    {
                        await this.credentialProvider.DeleteAsync(token);
                        this.allCredentials.Remove(token);
                    }
                }
            );

            this.deleteAllCommand = new AsyncActionCommand(
                async () =>
                {
                    await this.credentialProvider.ClearAsync();
                    this.allCredentials.Clear();
                }
            );

            this.allCredentials = new ObservableCollection<string>();
        }

        /// <summary>
        /// Builds up the list of stored credentials using the wrapped provider.
        /// </summary>
        /// <returns></returns>
        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();
            this.allCredentials.Clear();
            foreach(string token in await this.credentialProvider.GetAllEntriesAsync())
            {
                this.allCredentials.Add(token);
            }
        }

        /// <summary>
        /// A command for deleting a specific credential.
        /// </summary>
        public IAsyncCommand DeleteCredentialAsyncCommand
        {
            get { return this.deleteCredentialCommand; }
        }

        /// <summary>
        /// A command for deleting all credentials.
        /// </summary>
        public IAsyncCommand DeleteAllAsyncCommand
        {
            get { return this.deleteAllCommand; }
        }

        /// <summary>
        /// A collection of credentials that are stored.
        /// </summary>
        public ObservableCollection<string> CredentialTokens
        {
            get { return this.allCredentials; }
        }
    }
}
