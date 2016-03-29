using PassKeep.Lib.Contracts.Services;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// An implementation of <see cref="ICredentialStorageService"/> that uses the Windows
    /// <see cref="PasswordVault"/> to secure credentials.
    /// </summary>
    public sealed class PasswordVaultCredentialService : ICredentialStorageService
    {
        private PasswordVault vault;

        /// <summary>
        /// Asynchronously fetches data representing the raw
        /// aggregate key for a database.
        /// </summary>
        /// <param name="databaseToken">Data identifying the key to fetch.</param>
        /// <returns>A task representing the key data.</returns>
        public Task<IBuffer> GetRawKeyAsync(string databaseToken)
        {
            return null;
        }

        /// <summary>
        /// Asynchronously stores the key for a database in a secure location.
        /// </summary>
        /// <param name="databaseToken">Data identifying the key for future retrieval.</param>
        /// <param name="key">The key to store.</param>
        /// <returns>A task that completes when storage has finished.</returns>
        public Task StoreRawKeyAsync(string databaseToken, IBuffer key)
        {
            return null;
        }
    }
}
