// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IDatabaseViewModel : IDatabasePersistenceViewModel, IActiveDatabaseViewModel
    {
        /// <summary>
        /// Raised when we should prompt the user to rename a specific node.
        /// </summary>
        event TypedEventHandler<IDatabaseViewModel, IDatabaseNodeViewModel> RequestRenameNode;

        /// <summary>
        /// Raised when we should prompt the user to delete a specific node.
        /// </summary>
        event TypedEventHandler<IDatabaseViewModel, IDatabaseNodeViewModel> RequestDeleteNode;

        /// <summary>
        /// Raised when the user requests details for a specific node.
        /// </summary>
        event TypedEventHandler<IDatabaseViewModel, IDatabaseNodeViewModel> RequestDetails;

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
        /// A filter used to change the displayed children.
        /// </summary>
        string Filter
        {
            get;
            set;
        }

        IDatabaseGroupViewModel RootGroup { get; }

        /// <summary>
        /// Allows binding to a continually sorted list of nodes in the current document view.
        /// </summary>
        ReadOnlyObservableCollection<IDatabaseNodeViewModel> SortedChildren { get; }

        /// <summary>
        /// Gets a collection of queryable IKeePassNodes for search purposes.
        /// </summary>
        /// <returns>A collection of all IKeePassNodes (entries and groups) that are visible to searches.</returns>
        ICollection<IKeePassNode> GetAllSearchableNodes(string query);

        /// <summary>
        /// Attempts to delete the specified node from the document.
        /// </summary>
        /// <param name="node">The node to delete.</param>
        void DeleteNodeAndSave(IKeePassNode node);

        /// <summary>
        /// Attempts to rename the specified node in the document.
        /// </summary>
        /// <param name="node">The node to rename.</param>
        /// <param name="newName">The name to use.</param>
        void RenameNodeAndSave(IKeePassNode node, string newName);

        /// <summary>
        /// Creates an EntryDetailsViewModel for a new entry.
        /// </summary>
        /// <param name="parent">The group to use for the entry's parent.</param>
        /// <returns>An EntryDetailsViewModel for a new entry.</returns>
        IEntryDetailsViewModel GetEntryDetailsViewModel(IKeePassGroup parent);

        /// <summary>
        /// Creates an EntryDetailsViewModel for an existing entry.
        /// </summary>
        /// <param name="entry">The entry to open.</param>
        /// <param name="editing">Whether to open the entry in edit mode.</param>
        /// <returns>An EntryDetailsViewModel for an existing entry.</returns>
        IEntryDetailsViewModel GetEntryDetailsViewModel(IKeePassEntry entry, bool editing);

        /// <summary>
        /// Creates a GroupDetailsViewModel for a new group.
        /// </summary>
        /// <param name="parent">The group to use for the group's parent.</param>
        /// <returns>A GroupDetailsViewModel for a new group.</returns>
        IGroupDetailsViewModel GetGroupDetailsViewModel(IKeePassGroup parent);

        /// <summary>
        /// Creates a GroupDetailsViewModel for an existing group.
        /// </summary>
        /// <param name="group">The entry to open.</param>
        /// <param name="editing">Whether to open the group in edit mode.</param>
        /// <returns>A GroupDetailsViewModel for an existing group.</returns>
        IGroupDetailsViewModel GetGroupDetailsViewModel(IKeePassGroup group, bool editing);

        /// <summary>
        /// Enumerates the children of a specific group according to sort settings.
        /// </summary>
        /// <param name="root">The node to enumerate.</param>
        /// <param name="searchQuery">Used for filtering.</param>
        /// <returns>An ordered collection of the root's children.</returns>
        IOrderedEnumerable<IDatabaseNodeViewModel> GenerateSortedChildren(IKeePassGroup root, string searchQuery);
    }
}
