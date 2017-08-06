// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using System;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Wraps an <see cref="IKeePassNode"/> with some additional View-related logic.
    /// </summary>
    public interface IDatabaseNodeViewModel
    {
        /// <summary>
        /// Provides access to the wrapped node.
        /// </summary>
        IKeePassNode Node { get; }

        /// <summary>
        /// Fired when the user requests a rename of this node.
        /// </summary>
        event EventHandler RenameRequested;

        /// <summary>
        /// Fired when the user requests to edit the details of this node.
        /// </summary>
        event EventHandler EditRequested;

        /// <summary>
        /// Fired when the user requests to delete this node.
        /// </summary>
        event EventHandler DeleteRequested;

        /// <summary>
        /// Command for requesting a rename prompt for the current node.
        /// </summary>
        ICommand RequestRenameCommand { get; }

        /// <summary>
        /// Command for requesting to edit the details for the current node.
        /// </summary>
        ICommand RequestEditDetailsCommand { get; }

        /// <summary>
        /// Command for requesting deletion of the current node.
        /// </summary>
        ICommand RequestDeleteCommand { get; }
    }
}
