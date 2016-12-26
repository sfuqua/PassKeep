using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
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
using System.Windows.Input;

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
        private readonly TypedCommand<StoredFileDescriptor> forgetCommand;
        private ObservableCollection<StoredFileDescriptor> data;
        private ReadOnlyObservableCollection<StoredFileDescriptor> readOnlyData;

        /// <summary>
        /// Initializes the ViewModel with the <see cref="IStorageItemAccessList"/> provided.
        /// </summary>
        /// <param name="accessList">The access list used to populate the RecentDatabases collection.</param>
        /// <param name="motdProvider">Used to provide the message-of-the-day.</param>
        /// <param name="proxyProvider">Used to generate database proxy files in the roaming directory.</param>
        public DashboardViewModel(IDatabaseAccessList accessList, IMotdProvider motdProvider, IFileProxyProvider proxyProvider)
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

            this.accessList = accessList;
            this.motdProvider = motdProvider;
            this.proxyProvider = proxyProvider;

            this.data = new ObservableCollection<StoredFileDescriptor>(
                this.accessList.Entries.Select(entry => new StoredFileDescriptor(entry))
            );

            this.readOnlyData = new ReadOnlyObservableCollection<StoredFileDescriptor>(this.data);

            this.forgetCommand = new TypedCommand<StoredFileDescriptor>(
                /* canExecute */ file => (file != null && this.data.Count > 0),
                /* execute */ file =>
                {
                    if (this.accessList.ContainsItem(file.Token))
                    {
                        this.accessList.Remove(file.Token);
                        this.data.Remove(file);
                    }
                }
            );
        }

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
        /// Used to remove an entry from the list of RecentDatabases.
        /// </summary>
        public ICommand ForgetCommand
        {
            get { return this.forgetCommand; }
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
                ITestableFile file = await GetFileAsync(descriptor);
                if (file != null)
                {
                    descriptor.IsAppOwned = await this.proxyProvider.PathIsInScopeAsync(file);
                }
                else
                {
                    badDescriptors.Add(descriptor);
                }
            }

            foreach (StoredFileDescriptor descriptor in badDescriptors)
            {
                this.forgetCommand.Execute(descriptor);
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
    }
}
