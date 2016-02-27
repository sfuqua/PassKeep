﻿using PassKeep.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Represents an interface to a ViewModel used for the user's PassKeep dashboard.
    /// </summary>
    public interface IDashboardViewModel : IViewModel
    {
        /// <summary>
        /// Provides access to a list of recently accessed databases, for easy opening.
        /// </summary>
        ReadOnlyObservableCollection<StoredFileDescriptor> RecentDatabases
        {
            get;
        }

        /// <summary>
        /// Used to remove an entry from the list of RecentDatabases.
        /// </summary>
        ICommand ForgetCommand
        {
            get;
        }

        /// <summary>
        /// Whether the view should show a MOTD on activation.
        /// </summary>
        bool ShouldShowMotd
        {
            get;
        }

        /// <summary>
        /// Title for the message-of-the-day.
        /// </summary>
        string MotdTitle
        {
            get;
        }

        /// <summary>
        /// Contents of the message-of-the-day.
        /// </summary>
        string MotdBody
        {
            get;
        }

        /// <summary>
        /// Describes the action to dismiss the message-of-the-day.
        /// </summary>
        string MotdDismissText
        {
            get;
        }

        /// <summary>
        /// Attempts to fetch an IStorageFile based on a descriptor.
        /// </summary>
        /// <param name="descriptor">A previously stored reference to a file.</param>
        /// <returns>An IStorageFile if possible, else null.</returns>
        Task<IStorageFile> GetFileAsync(StoredFileDescriptor descriptor);
    }
}
