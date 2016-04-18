using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// An implementation of <see cref="ICredentialStorageProvider"/> based on a dictionary.
    /// </summary>
    public sealed class MockCredentialProvider : ICredentialStorageProvider
    {
        private readonly Dictionary<string, IBuffer> storage
            = new Dictionary<string, IBuffer>();

        public Task ClearAsync()
        {
            this.storage.Clear();
            return Task.CompletedTask;
        }

        public Task DeleteAsync(IDatabaseCandidate database)
        {
            return DeleteAsync(database.FileName);
        }

        public Task DeleteAsync(string databaseToken)
        {
            if (this.storage.ContainsKey(databaseToken))
            {
                this.storage.Remove(databaseToken);
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<string>> GetAllEntriesAsync()
        {
            return Task.FromResult(
                (IReadOnlyCollection<string>)this.storage.Keys.ToList()
            );
        }

        public Task<IBuffer> GetRawKeyAsync(IDatabaseCandidate database)
        {
            return Task.FromResult(this.storage.ContainsKey(database.FileName) ?
                this.storage[database.FileName] : null);
        }

        public Task<bool> TryStoreRawKeyAsync(IDatabaseCandidate database, IBuffer key)
        {
            this.storage[database.FileName] = key;
            return Task.FromResult(true);
        }
    }
}
