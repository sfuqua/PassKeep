// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using PassKeep.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IDatabaseNavigationViewModel : IViewModel
    {
        /// <summary>
        /// Raised when the visible leaf nodes of the current position change.
        /// </summary>
        event EventHandler LeavesChanged;

        /// <summary>
        /// The collection of groups leading up to (and including) the current position in the tree.
        /// </summary>
        ReadOnlyObservableCollection<Breadcrumb> Breadcrumbs
        {
            get;
        }

        // TODO: Implement this for TreeView support
        /// <summary>
        /// Allows binding to a continually sorted list of nodes in the current document view.
        /// </summary>
        // ReadOnlyObservableCollection<IDatabaseNodeViewModel> SortedChildren { get; }

        /// <summary>
        /// The last group in the breadcrumb trail - the group the user is currently exploring.
        /// </summary>
        IKeePassGroup ActiveGroup
        {
            get;
        }

        /// <summary>
        /// The entry the user is investigating, if one exists.
        /// </summary>
        IKeePassEntry ActiveLeaf
        {
            get;
        }

        /// <summary>
        /// An ICommand responsible for launching an entry's URL.
        /// </summary>
        ICommand UrlLaunchCommand
        {
            get;
        }

        /// <summary>
        /// Clears out the ActiveLeaf.
        /// </summary>
        void Prune();

        /// <summary>
        /// Sets the last Breadcrumb to an Entry's parent, resets the leaves, and flags the Entry as active.
        /// </summary>
        /// <param name="entry">The entry to activate.</param>
        void SetEntry(IKeePassEntry entry);

        /// <summary>
        /// Updates the Breadcrumb list and resets all the leaves (including the active one).
        /// </summary>
        /// <param name="group">The group to activate.</param>
        void SetGroup(IKeePassGroup group);
    }
}
