// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Extends <see cref="DatabaseNodeViewModel"/> to wrap a group, specifically.
    /// </summary>
    public sealed class DatabaseGroupViewModel : DatabaseNodeViewModel, IDatabaseGroupViewModel
    {
        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        /// <param name="group">The database group to proxy.</param>
        /// <param name="childFactory">Used to map child nodes to new ViewModels.</param>
        /// <param name="isReadOnly">Whether the database is in a state that can be edited.</param>
        public DatabaseGroupViewModel(IKeePassGroup group, IDatabaseNodeViewModelFactory childFactory, bool isReadOnly)
            : base(group, isReadOnly)
        {
            Children = new ReadOnlyObservableCollectionTransform<IKeePassNode, IDatabaseNodeViewModel>(group.Children, childFactory.Assemble);
            RequestOpenCommand = new ActionCommand(FireOpenRequested);
        }

        /// <summary>
        /// Fired when the group is requested to be opened.
        /// </summary>
        public event EventHandler OpenRequested;

        private void FireOpenRequested()
        {
            OpenRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Command for requesting to open the current group.
        /// </summary>
        public ICommand RequestOpenCommand
        {
            get;
            private set;
        }

        public ReadOnlyObservableCollectionTransform<IKeePassNode, IDatabaseNodeViewModel> Children { get; }

        /// <summary>
        /// Whether this group has children.
        /// </summary>
        public bool HasChildren => Children.Any();
    }

    public delegate IOrderedEnumerable<IDatabaseNodeViewModel> ChildBuilder(IKeePassGroup root);
}
