using System;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// ViewModel for a view over an unlocked database, regardless of the details of the view.
    /// </summary>
    public interface IDatabaseParentViewModel : IDatabasePersistenceViewModel, IActiveDatabaseViewModel
    {
        /// <summary>
        /// Fired when the View should lock the current workspace.
        /// </summary>
        event EventHandler LockRequested;

        /// <summary>
        /// The file on disk represented by this database.
        /// </summary>
        IStorageFile File
        {
            get;
        }

        /// <summary>
        /// Whether <see cref="File"/> is a sample database.
        /// </summary>
        bool FileIsSample
        {
            get;
        }

        /// <summary>
        /// Generates an <see cref="IDatabaseViewModel"/> based on current state.
        /// </summary>
        /// <returns>A ViewModel over the database tree.</returns>
        IDatabaseViewModel GetDatabaseViewModel();

        /// <summary>
        /// Attempts to lock the workspace manually.
        /// </summary>
        void TryLock();
    }
}
