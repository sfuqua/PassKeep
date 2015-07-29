using PassKeep.Lib.KeePass.Dom;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// An interface for a service that can persist (save) databases to storage.
    /// </summary>
    public interface IDatabasePersistenceService
    {
        /// <summary>
        /// Attempts to asynchronously persist the document.
        /// </summary>
        /// <param name="document">The KdbxDocument to persist.</param>
        /// <param name="token">A CancellationToken for the operation.</param>
        /// <returns>A Task representing whether the save was successful.</returns>
        Task<bool> Save(KdbxDocument document, CancellationToken token);
    }
}
