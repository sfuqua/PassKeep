using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.Models;
using PassKeep.Models;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that provides MostRecentlyUsed access to databases for easy opening.
    /// </summary>
    public class DashboardViewModel : AbstractViewModel, IDashboardViewModel
    {
        private readonly IDatabaseAccessList accessList;
        private readonly IMotdProvider motdProvider;
        private readonly IFileProxyProvider proxyProvider;
        private readonly IFileExportService exportService;
        private ObservableCollection<StoredFileDescriptor> data;
        private ReadOnlyObservableCollection<StoredFileDescriptor> readOnlyData;

        /// <summary>
        /// Initializes the ViewModel with the <see cref="IStorageItemAccessList"/> provided.
        /// </summary>
        /// <param name="accessList">The access list used to populate the RecentDatabases collection.</param>
        /// <param name="motdProvider">Used to provide the message-of-the-day.</param>
        /// <param name="proxyProvider">Used to generate database proxy files in the roaming directory.</param>
        /// <param name="exportService">Used to export copies of cached files.</param>
        public DashboardViewModel(
            IDatabaseAccessList accessList,
            IMotdProvider motdProvider,
            IFileProxyProvider proxyProvider,
            IFileExportService exportService
        )
        {
            if (accessList == null)
            {
                throw new ArgumentNullException(nameof(accessList));
            }

            if (motdProvider == null)
            {
                throw new ArgumentNullException(nameof(motdProvider));
            }

            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }

            if (exportService == null)
            {
                throw new ArgumentNullException(nameof(exportService));
            }

            this.accessList = accessList;
            this.motdProvider = motdProvider;
            this.proxyProvider = proxyProvider;
            this.exportService = exportService;

            this.data = new ObservableCollection<StoredFileDescriptor>(
                this.accessList.Entries.Select(entry => new StoredFileDescriptor(entry))
            );

            this.readOnlyData = new ReadOnlyObservableCollection<StoredFileDescriptor>(this.data);
        }

        /// <summary>
        /// Fired when the View should handle opening the specified file.
        /// </summary>
        public event TypedEventHandler<IDashboardViewModel, StoredFileDescriptor> RequestOpenFile;

        /// <summary>
        /// Fired when the View should consent to deleting a stored file descriptor.
        /// </summary>
        public event TypedEventHandler<IDashboardViewModel, RequestForgetDescriptorEventArgs> RequestForgetDescriptor;

        /// <summary>
        /// Provides access to a list of recently accessed databases, for easy opening.
        /// </summary>
        public ReadOnlyObservableCollection<StoredFileDescriptor> RecentDatabases
        {
            get
            {
                return this.readOnlyData;
            }
        }

        /// <summary>
        /// Activates the dashboard ViewModel by resolving each stored file
        /// to update the descriptors and remove bad bookmarks.
        /// </summary>
        /// <returns>A Task representing the activation.</returns>
        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();

            List<StoredFileDescriptor> badDescriptors = new List<StoredFileDescriptor>();
            foreach (StoredFileDescriptor descriptor in this.data)
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

            foreach (StoredFileDescriptor descriptor in badDescriptors)
            {
                descriptor.ForgetCommand.Execute(null);
            }
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
        private void WireDescriptorEvents(StoredFileDescriptor descriptor)
        {
            descriptor.ForgetRequested += async (s, e) =>
            {
                if (this.accessList.ContainsItem(s.Token))
                {
                    RequestForgetDescriptor?.Invoke(this, e);

                    if (await e.GetConsentAsync())
                    {
                        this.accessList.Remove(s.Token);
                        this.data.Remove(s);

                        if (s.IsAppOwned)
                        {
                            // Delete the proxy
                            await this.proxyProvider.TryDeleteProxyAsync(s.Metadata);
                        }
                    }
                }
            };

            descriptor.ExportRequested += (s, e) =>
            {
                this.exportService.ExportAsync(descriptor);
            };

            descriptor.OpenRequested += (s, e) =>
            {
                FireRequestOpenFile(s);
            };
        }
    }
}
