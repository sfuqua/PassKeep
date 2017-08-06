// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Models;
using SariphLib.Mvvm;
using System.Collections.ObjectModel;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Represents a ViewModel over a list of local cached files, allowing
    /// them to be managed/deleted.
    /// </summary>
    public interface ICachedFilesViewModel : IStoredDatabaseManagingViewModel
    {
        /// <summary>
        /// A command for deleting all files.
        /// </summary>
        IAsyncCommand DeleteAllAsyncCommand { get; }
    }
}
