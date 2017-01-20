using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Models;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents a ViewModel over a list of saved credentials, allowing
    /// them to be managed/deleted.
    /// </summary>
    public sealed class CachedFilesViewModel : CachedFileExportingViewModel, ICachedFilesViewModel
    {
        private readonly AsyncActionCommand deleteAllCommand;
        private readonly IFileProxyProvider proxyProvider;

        /// <summary>
        /// Initializes the commands and sets <see cref="CredentialTokens"/> to an empty collection.
        /// The ViewModel must be activated before use.
        /// </summary>
        /// <param name="accessList">A list used to look up candidates to get their underlying files.</param>
        /// <param name="exportService">Used to export stored files.</param>
        /// <param name="credentialProvider">Provider to use for accessing stored credentials.</param>
        public CachedFilesViewModel(
            IDatabaseAccessList accessList,
            IFileExportService exportService,
            IFileProxyProvider proxyProvider
        ) : base(accessList, proxyProvider, exportService)
        {
            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }

            this.proxyProvider = proxyProvider;
            this.deleteAllCommand = new AsyncActionCommand(
                async () =>
                {
                    if (await this.proxyProvider.TryDeleteAllProxiesAsync()
                        .ConfigureAwait(false)
                    )
                    {
                        ClearAllFiles();
                    }
                    else
                    {
                        // If clearing wasn't successful we might need to add back some
                        // files.
                        await ResyncFiles();
                    }
                }
            );
        }

        /// <summary>
        /// Builds up the list of stored files using the cache folder.
        /// </summary>
        /// <returns></returns>
        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();
            await ResyncFiles();
        }

        /// <summary>
        /// A command for deleting all files.
        /// </summary>
        public IAsyncCommand DeleteAllAsyncCommand
        {
            get { return this.deleteAllCommand; }
        }

        /// <summary>
        /// Asynchronously clears and adds back <see cref="StoredFileDescriptor"/> to updated
        /// <see cref="StoredFiles"/>.
        /// </summary>
        /// <returns></returns>
        private async Task ResyncFiles()
        {
            ClearAllFiles();
            foreach (ITestableFile file in await this.proxyProvider.GetKnownProxiesAsync()
                .ConfigureAwait(false)
            )
            {
                StoredFileDescriptor descriptor = new StoredFileDescriptor(
                    new AccessListEntry
                    {
                        Metadata = file.AsIStorageFile.Name,
                        Token = file.AsIStorageFile.Name
                    }
                );
                descriptor.IsAppOwned = await this.proxyProvider.PathIsInScopeAsync(file.AsIStorageItem2);

                // Wire events
                WireDescriptorEvents(descriptor);
                AddFile(descriptor);
            }
        }

        private void Descriptor_ForgetRequested(StoredFileDescriptor sender, EventArgClasses.RequestForgetDescriptorEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
