using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Models;
using PassKeep.Models;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Storage.AccessCache;
using PassKeep.Lib.EventArgClasses;

namespace PassKeep.Lib.ViewModels.DesignTime
{
    /// <summary>
    /// Represents an IDashboardViewModel suitable for the XAML designer.
    /// </summary>
    public class DesignDashboardViewModel : AbstractViewModel, IDashboardViewModel
    {
        private ObservableCollection<StoredFileDescriptor> mockData;
        private ReadOnlyObservableCollection<StoredFileDescriptor> readOnlyData;

        public event TypedEventHandler<IStoredDatabaseManagingViewModel, RequestUpdateDescriptorEventArgs> RequestUpdateDescriptor
        {
            add { }
            remove { }
        }

        public event TypedEventHandler<IStoredDatabaseManagingViewModel, RequestForgetDescriptorEventArgs> RequestForgetDescriptor
        {
            add { }
            remove { }
        }

        public event TypedEventHandler<IDashboardViewModel, StoredFileDescriptor> RequestOpenFile
        {
            add { }
            remove { }
        }

        public event TypedEventHandler<IDashboardViewModel, StoredFileDescriptor> RequestExportFile
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Initializes the mock data list.
        /// </summary>
        public DesignDashboardViewModel()
        {
            this.mockData = new ObservableCollection<StoredFileDescriptor>(
                new List<AccessListEntry>
                {
                    new AccessListEntry
                    {
                        Token = Guid.NewGuid().ToString(),
                        Metadata = "Sample Database"
                    },
                    new AccessListEntry
                    {
                        Token = Guid.NewGuid().ToString(),
                        Metadata = "My KeePass Vault"
                    },
                    new AccessListEntry
                    {
                        Token = Guid.NewGuid().ToString(),
                        Metadata = "Passwords"
                    }
                }.Select(a => new StoredFileDescriptor(a))
            );

            this.readOnlyData = new ReadOnlyObservableCollection<StoredFileDescriptor>(this.mockData);

            ForgetCommand = new TypedCommand<StoredFileDescriptor>(
                (file) =>
                {
                    this.mockData.Remove(file);
                }
            );
        }

        /// <summary>
        /// Provides access to the mock data represented by this ViewModel.
        /// </summary>
        public ReadOnlyObservableCollection<StoredFileDescriptor> StoredFiles
        {
            get
            {
                return this.readOnlyData;
            }
        }

        /// <summary>
        /// Dummy.
        /// </summary>
        public ICommand ForgetCommand
        {
            get;
            private set;
        }

        public MessageOfTheDay RequestMotd()
        {
            return MessageOfTheDay.Hidden;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="descriptor">A previously stored reference to a file.</param>
        /// <returns>An IStorageFile if possible, else null.</returns>
        public Task<ITestableFile> GetFileAsync(StoredFileDescriptor descriptor)
        {
            return null;
        }
    }
}
