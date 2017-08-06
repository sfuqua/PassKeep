// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Extends <see cref="IDatabaseNodeViewModel"/> to enable
    /// group specific tasks.
    /// </summary>
    public interface IDatabaseGroupViewModel : IDatabaseNodeViewModel
    {
        /// <summary>
        /// Fired when the group is requested to be opened.
        /// </summary>
        event EventHandler OpenRequested;

        /// <summary>
        /// Command for requesting to open the current group.
        /// </summary>
        ICommand RequestOpenCommand { get; }
    }
}
