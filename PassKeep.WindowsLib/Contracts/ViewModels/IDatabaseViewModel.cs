using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IDatabaseViewModel : IViewModel
    {
        /// <summary>
        /// Raised when a new save operation has begun.
        /// </summary>
        event EventHandler<CancellableEventArgs> StartedSave;

        /// <summary>
        /// Raised when a save operation has stopped for any reason.
        /// </summary>
        event EventHandler StoppedSave;

        /// <summary>
        /// The navigation ViewModel for the database.
        /// </summary>
        /// <remarks>
        /// This is responsible for tracking "where" the user is in the database.
        /// </remarks>
        IDatabaseNavigationViewModel NavigationViewModel { get; }

        /// <summary>
        /// The actual KdbxDocument represented by the ViewModel.
        /// </summary>
        KdbxDocument Document { get; }

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
        /// Allows binding to a continually sorted list of groups in the current database view.
        /// </summary>
        ReadOnlyObservableCollection<IKeePassGroup> SortedGroups { get; }

        /// <summary>
        /// Allows binding to a continually sorted list of entries in the current database view.
        /// </summary>
        ReadOnlyObservableCollection<IKeePassEntry> SortedEntries { get; }

        /// <summary>
        /// Attempts to save the current state of the database to storage.
        /// </summary>
        /// <returns>A Task representing whether the save was successful.</returns>
        Task<bool> TrySave();

        /// <summary>
        /// Gets a collection of queryable IKeePassNodes for search purposes.
        /// </summary>
        /// <returns>A collection of all IKeePassNodes (entries and groups) that are visible to searches.</returns>
        ICollection<IKeePassNode> GetAllSearchableNodes();

        /// <summary>
        /// Attempts to delete the specified group from the database.
        /// </summary>
        /// <param name="group">The group to delete.</param>
        void DeleteGroupAndSave(IKeePassGroup group);

        /// <summary>
        /// Attempts to delete the specified entry from the database.
        /// </summary>
        /// <param name="entry">The entry to delete.</param>
        void DeleteEntryAndSave(IKeePassEntry entry);
    }
}
