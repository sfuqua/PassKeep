using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Models;
using SariphLib.Files;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IDatabaseAccessList accessList;
        private readonly IFileProxyProvider proxyProvider;

        /// <summary>
        /// Initializes the commands and sets <see cref="CredentialTokens"/> to an empty collection.
        /// The ViewModel must be activated before use.
        /// </summary>
        /// <param name="accessList">A list used to look up candidates to get their underlying files.</param>
        /// <param name="exportService">Used to export stored files.</param>
        /// <param name="proxyProvider">Provider to use for accessing stored databases.</param>
        /// <param name="deletePrompter">Service to use for prompting user consent/understanding.</param>
        /// <param name="updatePrompter">Service to use for prompting user consent/understanding.</param>
        /// <param name="fileService">Service to use for accessing the filesystem.</param>
        public CachedFilesViewModel(
            IDatabaseAccessList accessList,
            IFileExportService exportService,
            IFileProxyProvider proxyProvider,
            IUserPromptingService deletePrompter,
            IUserPromptingService updatePrompter,
            IFileAccessService fileService
        ) : base(accessList, proxyProvider, exportService, deletePrompter, updatePrompter, fileService)
        {
            this.accessList = accessList;
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
                var allStoredFiles = this.accessList.Entries
                    .Select(e => new { Entry = e, FileTask = this.accessList.GetFileAsync(e.Token) });
                
                AccessListEntry? entry = null;
                foreach (var stored in allStoredFiles)
                {
                    ITestableFile storedFile = await stored.FileTask.ConfigureAwait(false);
                    if (storedFile.AsIStorageItem.Path == file.AsIStorageItem.Path)
                    {
                        entry = stored.Entry;
                    }
                }

                // If we couldn't find the file in the access list, add it.
                // This asserts because these lists shouldn't be out of sync in the first place.
                if (!entry.HasValue)
                {
                    string metadata = file.AsIStorageItem.Name;
                    entry = new AccessListEntry
                    {
                        Metadata = metadata,
                        Token = this.accessList.Add(file, metadata)
                    };

                    Dbg.Assert(false);
                }

                StoredFileDescriptor descriptor = new StoredFileDescriptor(
                    entry.Value
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
