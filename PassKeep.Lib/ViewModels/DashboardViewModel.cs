﻿using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Models;
using PassKeep.Models;
using SariphLib.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that provides MostRecentlyUsed access to databases for easy opening.
    /// </summary>
    public class DashboardViewModel : AbstractViewModel, IDashboardViewModel
    {
        private readonly IDatabaseAccessList accessList;
        private readonly IMotdProvider motdProvider;
        private ObservableCollection<StoredFileDescriptor> data;
        private ReadOnlyObservableCollection<StoredFileDescriptor> readOnlyData;

        /// <summary>
        /// Initializes the ViewModel with the <see cref="IStorageItemAccessList"/> provided.
        /// </summary>
        /// <param name="accessList">The access list used to populate the RecentDatabases collection.</param>
        /// <param name="motdProvider">Used to provide the message-of-the-day.</param>
        public DashboardViewModel(IDatabaseAccessList accessList, IMotdProvider motdProvider)
        {
            if (accessList == null)
            {
                throw new ArgumentNullException(nameof(accessList));
            }

            if (motdProvider == null)
            {
                throw new ArgumentNullException(nameof(motdProvider));
            }

            this.accessList = accessList;
            this.motdProvider = motdProvider;

            this.data = new ObservableCollection<StoredFileDescriptor>(
                this.accessList.Entries.Select(entry => new StoredFileDescriptor(entry))
            );

            this.readOnlyData = new ReadOnlyObservableCollection<StoredFileDescriptor>(this.data);

            this.ForgetCommand = new TypedCommand<StoredFileDescriptor>(
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
            get;
            private set;
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
        public async Task<IStorageFile> GetFileAsync(StoredFileDescriptor descriptor)
        {
            try
            {
                IStorageItem item = await this.accessList.GetItemAsync(descriptor.Token);
                return item as IStorageFile;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}
