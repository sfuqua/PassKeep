using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Models;
using SariphLib.Files;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents a ViewModel over a list of saved credentials, allowing
    /// them to be managed/deleted.
    /// </summary>
    public sealed class CachedFilesViewModel : AbstractViewModel, ICachedFilesViewModel
    {
        private readonly AsyncTypedCommand<StoredFileDescriptor> deleteFileCommand;
        private readonly AsyncActionCommand deleteAllCommand;

        private readonly ObservableCollection<StoredFileDescriptor> allFiles;

        private readonly IFileProxyProvider proxyProvider;

        /// <summary>
        /// Initializes the commands and sets <see cref="CredentialTokens"/> to an empty collection.
        /// The ViewModel must be activated before use.
        /// </summary>
        /// <param name="credentialProvider">Provider to use for accessing stored credentials.</param>
        public CachedFilesViewModel(
            IFileProxyProvider proxyProvider
        )
        {
            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }

            this.proxyProvider = proxyProvider;

            this.deleteFileCommand = new AsyncTypedCommand<StoredFileDescriptor>(
                (descriptor) => descriptor != null,
                async (descriptor) =>
                {
                    if (this.allFiles.Contains(descriptor))
                    {
                        if (await this.proxyProvider.TryDeleteProxyAsync(descriptor.Token)
                            .ConfigureAwait(false)
                        )
                        {
                            this.allFiles.Remove(descriptor);
                        }
                        else
                        {
                            // Better to fail to delete than to do anything bad.
                            Dbg.Trace($"Warning: Unable to delete cached file");
                        }
                    }
                }
            );

            this.deleteAllCommand = new AsyncActionCommand(
                async () =>
                {
                    if (await this.proxyProvider.TryDeleteAllProxiesAsync()
                        .ConfigureAwait(false)
                    )
                    {
                        this.allFiles.Clear();
                    }
                    else
                    {
                        // If clearing wasn't successful we might need to add back some
                        // files.
                        await ResyncFiles();
                    }
                }
            );

            this.allFiles = new ObservableCollection<StoredFileDescriptor>();
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
        /// A command for deleting a specific file.
        /// </summary>
        public IAsyncCommand DeleteFileAsyncCommand
        {
            get { return this.deleteFileCommand; }
        }

        /// <summary>
        /// A command for deleting all files.
        /// </summary>
        public IAsyncCommand DeleteAllAsyncCommand
        {
            get { return this.deleteAllCommand; }
        }

        /// <summary>
        /// A collection of files that are stored.
        /// </summary>
        public ObservableCollection<StoredFileDescriptor> CachedFiles
        {
            get { return this.allFiles; }
        }

        /// <summary>
        /// Asynchronously clears and adds back <see cref="StoredFileDescriptor"/> to updated
        /// <see cref="CachedFiles"/>.
        /// </summary>
        /// <returns></returns>
        private async Task ResyncFiles()
        {
            this.allFiles.Clear();
            foreach (ITestableFile file in await this.proxyProvider.GetKnownProxiesAsync()
                .ConfigureAwait(false)
            )
            {
                this.allFiles.Add(new StoredFileDescriptor(
                    new AccessListEntry
                    {
                        Metadata = file.AsIStorageFile.Name,
                        Token = file.AsIStorageFile.Name
                    }
                ));
            }
        }
    }
}
