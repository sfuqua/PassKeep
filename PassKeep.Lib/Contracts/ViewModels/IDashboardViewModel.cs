﻿using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.Models;
using PassKeep.Models;
using SariphLib.Files;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Represents an interface to a ViewModel used for the user's PassKeep dashboard.
    /// </summary>
    public interface IDashboardViewModel : IViewModel
    {
        /// <summary>
        /// Fired when the View should handle opening the specified file.
        /// </summary>
        event TypedEventHandler<IDashboardViewModel, StoredFileDescriptor> RequestOpenFile;

        /// <summary>
        /// Fired when the View should consent to deleting a stored file descriptor.
        /// </summary>
        event TypedEventHandler<IDashboardViewModel, RequestForgetDescriptorEventArgs> RequestForgetDescriptor;

        /// <summary>
        /// Provides access to a list of recently accessed databases, for easy opening.
        /// </summary>
        ReadOnlyObservableCollection<StoredFileDescriptor> RecentDatabases
        {
            get;
        }

        /// <summary>
        /// Gets a MOTD to display to the user.
        /// </summary>
        /// <returns>A <see cref="MessageOfTheDay"/> with "ShouldDisplay" set appropriately.</returns>
        MessageOfTheDay RequestMotd();

        /// <summary>
        /// Attempts to fetch a a file based on a descriptor.
        /// </summary>
        /// <param name="descriptor">A previously stored reference to a file.</param>
        /// <returns>An IStorageFile if possible, else null.</returns>
        Task<ITestableFile> GetFileAsync(StoredFileDescriptor descriptor);
    }
}
