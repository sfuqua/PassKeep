// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using SariphLib.Files;
using System.Collections.Generic;
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
        /// Asynchronously clears all stored credentials.
        /// </summary>
        /// <returns>A task that finishes when the clearing is completed.</returns>
        Task ClearAsync();

        /// <summary>
        /// Asynchronously gets a list of strings identifying all credentials
        /// in the data store.
        /// </summary>
        /// <returns>A task represeting a list of all databases in the vault.</returns>
        Task<IReadOnlyCollection<string>> GetAllEntriesAsync();

        /// <summary>
        /// Asynchronously removes a credential. Completes silently if credential is not found.
        /// </summary>
        /// <param name="databaseToken">The database to delete data for.</param>
        /// <returns>A task that finishes when the database is removed.</returns>
        Task DeleteAsync(string databaseToken);

        /// <summary>
        /// Asynchronously removes a credential. Completes silently if credential is not found.
        /// </summary>
        /// <param name="database">The database to delete data for.</param>
        /// <returns>A task that finishes when the database is removed.</returns>
        Task DeleteAsync(ITestableFile database);

        /// <summary>
        /// Asynchronously fetches data representing the raw
        /// aggregate key for a database.
        /// </summary>
        /// <param name="databaseToken">Data identifying the key to fetch.</param>
        /// <returns>A task representing the key data.</returns>
        Task<IBuffer> GetRawKeyAsync(string databaseToken);

        /// <summary>
        /// Asynchronously fetches data representing the raw
        /// aggregate key for a database.
        /// </summary>
        /// <param name="database">Data identifying the key to fetch.</param>
        /// <returns>A task representing the key data.</returns>
        Task<IBuffer> GetRawKeyAsync(ITestableFile database);

        /// <summary>
        /// Asynchronously stores the key for a database in a secure location.
        /// </summary>
        /// <param name="database">Data identifying the key for future retrieval.</param>
        /// <param name="key">The key to store.</param>
        /// <returns>A task representing whether the storage is successful.</returns>
        Task<bool> TryStoreRawKeyAsync(ITestableFile database, IBuffer key);
    }
}
