// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using SariphLib.Files;
using SariphLib.Diagnostics;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Abstracts shared code responsible for exporting and updating databases.
    /// </summary>
    public abstract class CachedFileExportingViewModel : AbstractViewModel, IStoredDatabaseManagingViewModel
    {
        private readonly IDatabaseAccessList accessList;
        private readonly IFileProxyProvider proxyProvider;
        private readonly IFileExportService exportService;
        private readonly IUserPromptingService deletePrompter;
        private readonly IUserPromptingService updatePrompter;
        private readonly IFileAccessService fileService;
        private readonly ObservableCollection<StoredFileDescriptor> data;
        private readonly ReadOnlyObservableCollection<StoredFileDescriptor> readOnlyData;

        /// <summary>
        /// Initializes the instance with the services needed to export files.
        /// </summary>
        /// <param name="accessList">Used to retrieve files from stored tokens.</param>
        /// <param name="proxyProvider">Used to manage underlying cached file proxies.</param>
        /// <param name="exportService">The service used to export databases.</param>
        /// <param name="deletePrompter">A service used to prompt the user for consent to delete a file.</param>
        /// <param name="updatePrompter">A service used to prompt the user for consent to update a file.</param>
        /// <param name="fileService">A service used to access the filesystem.</param>
        protected CachedFileExportingViewModel(
            IDatabaseAccessList accessList,
            IFileProxyProvider proxyProvider,
            IFileExportService exportService,
            IUserPromptingService deletePrompter,
            IUserPromptingService updatePrompter,
            IFileAccessService fileService
        )
        {
            if (accessList == null)
            {
                throw new ArgumentNullException(nameof(accessList));
            }

            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }

            if (exportService == null)
            {
                throw new ArgumentNullException(nameof(exportService));
            }

            if (deletePrompter == null)
            {
                throw new ArgumentNullException(nameof(deletePrompter));
            }

            if (updatePrompter == null)
            {
                throw new ArgumentNullException(nameof(updatePrompter));
            }

            if (fileService == null)
            {
                throw new ArgumentNullException(nameof(fileService));
            }

            this.accessList = accessList;
            this.proxyProvider = proxyProvider;
            this.exportService = exportService;
            this.deletePrompter = deletePrompter;
            this.updatePrompter = updatePrompter;
            this.fileService = fileService;

            this.data = new ObservableCollection<StoredFileDescriptor>(
                this.accessList.Entries.Select(entry => new StoredFileDescriptor(entry))
            );

            this.readOnlyData = new ReadOnlyObservableCollection<StoredFileDescriptor>(this.data);
        }

        /// <summary>
        /// Provides access to a list of recently accessed databases, for easy opening.
        /// </summary>
        public ReadOnlyObservableCollection<StoredFileDescriptor> StoredFiles
        {
            get
            {
                return this.readOnlyData;
            }
        }

        /// <summary>
        /// Attempts to fetch an IStorageFile based on a descriptor.
        /// </summary>
        /// <param name="descriptor">A previously stored reference to a file.</param>
        /// <returns>An IStorageFile if possible, else null.</returns>
        public async Task<ITestableFile> GetFileAsync(StoredFileDescriptor descriptor)
        {
            try
            {
                return await this.accessList.GetFileAsync(descriptor.Token).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Adds predefined handles for the Export and Update requested events.
        /// </summary>
        /// <param name="sender">The descriptor to wire.</param>
        protected virtual void WireDescriptorEvents(StoredFileDescriptor sender)
        {
            DebugHelper.Assert(sender != null);

            sender.ForgetRequested += ForgetRequestedHandler;
            sender.ExportRequested += ExportRequestedHandler;
            sender.UpdateRequested += UpdateRequestedHandler;
        }

        /// <summary>
        /// Adds the specified file to the internal list for tracking.
        /// </summary>
        /// <param name="descriptor">The descriptor to add.</param>
        protected void AddFile(StoredFileDescriptor descriptor)
        {
            DebugHelper.Assert(descriptor != null);
            this.data.Add(descriptor);
        }

        /// <summary>
        /// Clears the internal list of tracked files. Does not modify
        /// the filesystem.
        /// </summary>
        protected void ClearAllFiles()
        {
            this.data.Clear();
        }

        /// <summary>
        /// Intended to be registered as a handler for <see cref="StoredFileDescriptor.ForgetRequested"/>.
        /// </summary>
        /// <param name="sender">The descriptor being deleted.</param>
        /// <param name="e">Unused.</param>
        private async void ForgetRequestedHandler(StoredFileDescriptor sender, RequestForgetDescriptorEventArgs args)
        {
            // Ask the user whether they consent/understand the deletion
            if (!sender.IsAppOwned || await this.deletePrompter.PromptYesNoAsync())
            {
                // Delete from access list if appropriate
                if (this.accessList.ContainsItem(sender.Token))
                {
                    this.accessList.Remove(sender.Token);
                }

                // Update ViewModel's data list
                this.data.Remove(sender);

                // If this represents a proxy, delete it
                if (sender.IsAppOwned)
                {
                    // Delete the proxy
                    await this.proxyProvider.TryDeleteProxyAsync(sender.Metadata);
                }
            }
        }

        /// <summary>
        /// Intended to be registered as a handler for <see cref="StoredFileDescriptor.ExportRequested"/>.
        /// </summary>
        /// <param name="sender">The descriptor being exported.</param>
        /// <param name="e">Unused.</param>
        private void ExportRequestedHandler(StoredFileDescriptor sender, EventArgs e)
        {
            this.exportService.ExportAsync(sender);
        }

        /// <summary>
        /// Intended to be registered as a handler for <see cref="StoredFileDescriptor.UpdateRequested"/>.
        /// </summary>
        /// <param name="sender">The descriptor being updated.</param>
        /// <param name="args">EventArgs used to request the file to update with.</param>
        private async void UpdateRequestedHandler(StoredFileDescriptor sender, RequestUpdateDescriptorEventArgs args)
        {
            if (!sender.IsAppOwned)
            {
                DebugHelper.Assert(false, "This should be impossible");
                return;
            }

            ITestableFile file = await this.fileService.PickFileForOpenAsync().ConfigureAwait(false);
            if (file != null)
            {
                DebugHelper.Trace($"Updating cached file");
                ITestableFile storedFile = await GetFileAsync(sender).ConfigureAwait(false);

                if (await CheckShouldProceedWithUpdateAsync(storedFile, file).ConfigureAwait(false))
                {
                    await file.AsIStorageFile.CopyAndReplaceAsync(storedFile.AsIStorageFile)
                        .AsTask().ConfigureAwait(false);

                    await storedFile.ClearReadOnlyFlag().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Helper to prompt the user - if necessary - when updating a cached file.
        /// If there is a name mismatch or the replacement is older than the original,
        /// the user is prompted.
        /// </summary>
        /// <param name="original"></param>
        /// <param name="replacement"></param>
        /// <returns>Whether to proceed with the update.</returns>
        private async Task<bool> CheckShouldProceedWithUpdateAsync(ITestableFile original, ITestableFile replacement)
        {
            DateTimeOffset originalModified = await original.GetLastModifiedAsync().ConfigureAwait(false);
            DateTimeOffset replacementModified = await replacement.GetLastModifiedAsync().ConfigureAwait(false);
            
            bool relativeTimeIsSafe = replacementModified >= originalModified;
            bool nameIsSafe = original.Name == replacement.Name;

            if (relativeTimeIsSafe && nameIsSafe)
            {
                return true;
            }

            return await this.updatePrompter.PromptYesNoAsync(
                replacement.Name,
                originalModified,
                replacementModified
            );
        }
    }
}
