using PassKeep.Contracts.Models;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Provides a means of saving raw keys (pre-transformation)
    /// to some sort of secure storage, after verifying the identity
    /// of the user.
    /// </summary>
    public interface ICredentialStorageProvider
    {
        /// <summary>
        /// Asynchronously fetches data representing the raw
        /// aggregate key for a database.
        /// </summary>
        /// <param name="database">Data identifying the key to fetch.</param>
        /// <returns>A task representing the key data.</returns>
        Task<IBuffer> GetRawKeyAsync(IDatabaseCandidate database);

        /// <summary>
        /// Asynchronously stores the key for a database in a secure location.
        /// </summary>
        /// <param name="database">Data identifying the key for future retrieval.</param>
        /// <param name="key">The key to store.</param>
        /// <returns>A task representing whether the storage is successful.</returns>
        Task<bool> TryStoreRawKeyAsync(IDatabaseCandidate database, IBuffer key);
    }
}
