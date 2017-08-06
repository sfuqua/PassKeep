// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Models;
using System.Collections.ObjectModel;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Abstracts shared functionality regarding managing <see cref="StoredFileDescriptor"/> instances.
    /// </summary>
    public interface IStoredDatabaseManagingViewModel : IViewModel
    {
        /// <summary>
        /// Provides access to a list of recently accessed databases, for easy opening.
        /// </summary>
        ReadOnlyObservableCollection<StoredFileDescriptor> StoredFiles
        {
            get;
        }
    }
}
