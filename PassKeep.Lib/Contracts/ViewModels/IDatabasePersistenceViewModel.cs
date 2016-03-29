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
