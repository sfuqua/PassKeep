using PassKeep.Lib.KeePass.Dom;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// An interface for a service that can persist (save) databases to storage.
    /// </summary>
    public interface IDatabasePersistenceService : INotifyPropertyChanged
    {
        /// <summary>
        /// Whether the service is currently capable of persisting a document.
        /// </summary>
        bool CanSave { get; }

        /// <summary>
        /// Whether a save operation is currently in progress.
        /// </summary>
        bool IsSaving { get; }

        /// <summary>
        /// Attempts to asynchronously persist the document.
        /// </summary>
        /// <param name="document">The KdbxDocument to persist.</param>
        /// <returns>A Task representing whether the save was successful.</returns>
        Task<bool> Save(KdbxDocument document);
    }
}
