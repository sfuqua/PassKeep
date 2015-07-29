using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.EventArgClasses;
using System;
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
        /// Raised when a new save operation has begun.
        /// </summary>
        event EventHandler<CancellableEventArgs> StartedSave;

        /// <summary>
        /// Raised when a save operation has stopped for any reason.
        /// </summary>
        event EventHandler StoppedSave;

        /// <summary>
        /// Attempts to save the current state of the document to storage.
        /// </summary>
        /// <returns>A Task representing whether the save was successful.</returns>
        Task<bool> TrySave();
    }
}
