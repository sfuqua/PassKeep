using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// Provides a means of saving raw keys (pre-transformation)
    /// to some sort of secure storage, after verifying the identity
    /// of the user.
    /// </summary>
    public interface ICredentialStorageService
    {
        /// <summary>
        /// Asynchronously fetches data representing the raw
        /// aggregate key for a database.
        /// </summary>
        /// <param name="databaseToken">Data identifying the key to fetch.</param>
        /// <returns>A task representing the key data.</returns>
        Task<IBuffer> GetRawKeyAsync(string databaseToken);

        /// <summary>
        /// Asynchronously stores the key for a database in a secure location.
        /// </summary>
        /// <param name="databaseToken">Data identifying the key for future retrieval.</param>
        /// <param name="key">The key to store.</param>
        /// <returns>A task that completes when storage has finished.</returns>
        Task StoreRawKeyAsync(string databaseToken, IBuffer key);
    }
}
