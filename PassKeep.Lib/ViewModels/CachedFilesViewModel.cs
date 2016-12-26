using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Models;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
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

        private readonly StorageFolder cacheFolder;

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

            this.cacheFolder = proxyProvider.ProxyFolder;

            this.deleteFileCommand = new AsyncTypedCommand<StoredFileDescriptor>(
                (descriptor) => descriptor != null,
                async (descriptor) =>
                {
                    if (this.allFiles.Contains(descriptor))
                    {
                        try
                        {
                            StorageFile file = await this.cacheFolder.GetFileAsync(descriptor.Token)
                                .AsTask().ConfigureAwait(false);
                            await file.DeleteAsync();
                            this.allFiles.Remove(descriptor);
                        }
                        catch (Exception ex)
                        {
                            // Better to fail to delete than to crash
                            Dbg.Trace($"Unable to delete cached file; ex: {ex}");
                        }
                    }
                }
            );

            this.deleteAllCommand = new AsyncActionCommand(
                async () =>
                {
                    foreach (StorageFile file in await GetAllFiles().ConfigureAwait(false))
                    {
                        try
                        {
                            await file.DeleteAsync().AsTask().ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Dbg.Trace($"Unable to delete cached file; ex: {ex}");
                        }
                    }
                    this.allFiles.Clear();
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
            this.allFiles.Clear();
            foreach (StorageFile file in await GetAllFiles().ConfigureAwait(false))
            {
                this.allFiles.Add(new StoredFileDescriptor(
                    new AccessListEntry
                    {
                        Metadata = file.Name,
                        Token = file.Name
                    }
                ));
            }
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
        /// Helper to enumerate the files in the cache folder.
        /// </summary>
        /// <returns>An enumeration over the files in the cache folder.</returns>
        private async Task<IEnumerable<StorageFile>> GetAllFiles()
        {
            return await this.cacheFolder.GetFilesAsync()
                .AsTask().ConfigureAwait(false);
        }
    }
}
