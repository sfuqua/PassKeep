using PassKeep.EventArgClasses;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IDatabaseViewModel : IDatabasePersistenceViewModel
    {
        /// <summary>
        /// Fired when the user requests to copy credentials (username or password).
        /// </summary>
        event EventHandler<CopyRequestedEventArgs> CopyRequested;

        /// <summary>
        /// The navigation ViewModel for the document.
        /// </summary>
        /// <remarks>
        /// This is responsible for tracking "where" the user is in the document.
        /// </remarks>
        IDatabaseNavigationViewModel NavigationViewModel { get; }

        /// <summary>
        /// The actual KdbxDocument represented by the ViewModel.
        /// </summary>
        KdbxDocument Document { get; }

        /// <summary>
        /// A command that is activated when the user requests to copy
        /// an entry's username.
        /// </summary>
        ICommand RequestCopyUsernameCommand { get; }

        /// <summary>
        /// A command that is activated when the user requests to copy
        /// an entry's password.
        /// </summary>
        ICommand RequestCopyPasswordCommand { get; }

        /// <summary>
        /// A listing of all known, available sort modes.
        /// </summary>
        IReadOnlyCollection<DatabaseSortMode> AvailableSortModes { get; }

        /// <summary>
        /// The current DatabaseSortMode used by this ViewModel.
        /// </summary>
        DatabaseSortMode SortMode
        {
            get;
            set;
        }

        /// <summary>
        /// Allows binding to a continually sorted list of nodes in the current document view.
        /// </summary>
        ReadOnlyObservableCollection<IKeePassNode> SortedChildren { get; }

        /// <summary>
        /// Gets a collection of queryable IKeePassNodes for search purposes.
        /// </summary>
        /// <returns>A collection of all IKeePassNodes (entries and groups) that are visible to searches.</returns>
        ICollection<IKeePassNode> GetAllSearchableNodes();

        /// <summary>
        /// Attempts to delete the specified group from the document.
        /// </summary>
        /// <param name="group">The group to delete.</param>
        void DeleteGroupAndSave(IKeePassGroup group);

        /// <summary>
        /// Attempts to delete the specified entry from the document.
        /// </summary>
        /// <param name="entry">The entry to delete.</param>
        void DeleteEntryAndSave(IKeePassEntry entry);
    }
}
