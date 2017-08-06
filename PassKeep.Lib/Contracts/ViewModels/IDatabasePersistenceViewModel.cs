// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Services;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// An interface for a ViewModel that is capable of writing to the document.
    /// </summary>
    public interface IDatabasePersistenceViewModel : IViewModel
    {
        /// <summary>
        /// Provides access to the service used to persist the database through this ViewModel.
        /// </summary>
        IDatabasePersistenceService PersistenceService
        {
            get;
        }

        /// <summary>
        /// Attempts to save the current state of the document to storage.
        /// </summary>
        /// <returns>A Task representing the save operation.</returns>
        Task Save();
    }
}
