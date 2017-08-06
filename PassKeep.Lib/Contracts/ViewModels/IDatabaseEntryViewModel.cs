// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Extends <see cref="IDatabaseNodeViewModel"/> to enable
    /// copying of credentials and other entry-specific tasks.
    /// </summary>
    public interface IDatabaseEntryViewModel : IDatabaseNodeViewModel
    {
        /// <summary>
        /// Command for requesting copy of the username credential.
        /// </summary>
        ICommand RequestCopyUsernameCommand { get; }

        /// <summary>
        /// Command for requesting copy of the password credential.
        /// </summary>
        ICommand RequestCopyPasswordCommand { get; }

        /// <summary>
        /// Command for requesting copy of the entry's url.
        /// </summary>
        ICommand RequestCopyUrlCommand { get; }

        /// <summary>
        /// Command for requesting a launch of the entry's URL.
        /// </summary>
        ICommand RequestLaunchUrlCommand { get; }
    }
}
