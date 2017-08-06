// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Models;
using PassKeep.Models;
using SariphLib.Files;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that provides MostRecentlyUsed access to databases for easy opening.
    /// </summary>
    public class DashboardViewModel : CachedFileExportingViewModel, IDashboardViewModel
    {
        private readonly IMotdProvider motdProvider;
        private readonly IFileProxyProvider proxyProvider;

        /// <summary>
        /// Initializes the ViewModel with the <see cref="IStorageItemAccessList"/> provided.
        /// </summary>
        /// <param name="accessList">The access list used to populate the RecentDatabases collection.</param>
        /// <param name="motdProvider">Used to provide the message-of-the-day.</param>
        /// <param name="proxyProvider">Used to generate database proxy files in the roaming directory.</param>
        /// <param name="exportService">Used to export copies of cached files.</param>
        /// <param name="deletePrompter">Used to prompt the user for consent/understanding.</param>
        /// <param name="updatePrompter">Used to prompt the user for consent/understanding.</param>
        /// <param name="fileService">Used to access the filesystem.</param>
        public DashboardViewModel(
            IDatabaseAccessList accessList,
            IMotdProvider motdProvider,
            IFileProxyProvider proxyProvider,
            IFileExportService exportService,
            IUserPromptingService deletePrompter,
            IUserPromptingService updatePrompter,
            IFileAccessService fileService
        ) : base(accessList, proxyProvider, exportService, deletePrompter, updatePrompter, fileService)
        {
            if (motdProvider == null)
            {
                throw new ArgumentNullException(nameof(motdProvider));
            }

            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }
            
            this.motdProvider = motdProvider;
            this.proxyProvider = proxyProvider;
        }

        /// <summary>
        /// Fired when the View should handle opening the specified file.
        /// </summary>
        public event TypedEventHandler<IDashboardViewModel, StoredFileDescriptor> RequestOpenFile;

        /// <summary>
        /// Activates the dashboard ViewModel by resolving each stored file
        /// to update the descriptors and remove bad bookmarks.
        /// </summary>
        /// <returns>A Task representing the activation.</returns>
        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();

            List<StoredFileDescriptor> badDescriptors = new List<StoredFileDescriptor>();
            foreach (StoredFileDescriptor descriptor in StoredFiles)
            {
                WireDescriptorEvents(descriptor);
                ITestableFile file = await GetFileAsync(descriptor);
                if (file != null)
                {
                    descriptor.IsAppOwned = await this.proxyProvider.PathIsInScopeAsync(file.AsIStorageItem2);
                }
                else
                {
                    badDescriptors.Add(descriptor);
                }
            }

            Task[] forgetTasks = new Task[badDescriptors.Count];
            for (int i = 0; i < forgetTasks.Length; i++)
            {
                StoredFileDescriptor descriptor = badDescriptors[i];
                forgetTasks[i] = descriptor.ForgetCommand.ExecuteAsync(null);
            }

            await Task.WhenAll(forgetTasks);
        }

        /// <summary>
        /// Gets a MOTD to display to the user.
        /// </summary>
        /// <returns>A <see cref="MessageOfTheDay"/> with "ShouldDisplay" set appropriately.</returns>
        public MessageOfTheDay RequestMotd()
        {
            return this.motdProvider.GetMotdForDisplay();
        }

        /// <summary>
        /// Helper to fire <see cref="RequestOpenFile"/>.
        /// </summary>
        /// <param name="file">The file to open.</param>
        private void FireRequestOpenFile(StoredFileDescriptor file)
        {
            RequestOpenFile?.Invoke(this, file);
        }

        /// <summary>
        /// Helper to hook up UI events from a <see cref="StoredFileDescriptor"/>.
        /// </summary>
        /// <param name="descriptor">The descriptor to hook up events for.</param>
        protected override void WireDescriptorEvents(StoredFileDescriptor descriptor)
        {
            base.WireDescriptorEvents(descriptor);

            descriptor.OpenRequested += (s, e) =>
            {
                FireRequestOpenFile(s);
            };
        }
    }
}
