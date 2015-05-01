using PassKeep.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// An implementation of IStorageItemAccessList that uses a Dictionary
    /// for backing data.
    /// </summary>
    /// <remarks>
    /// http://msdn.microsoft.com/en-us/library/windows/apps/windows.storage.accesscache.istorageitemaccesslist
    /// </remarks>
    public class MockStorageItemAccessList : IDatabaseAccessList
    {
        private Dictionary<string, IStorageItem> backingData;
        private Dictionary<string, AccessListEntry> publicData;

        public MockStorageItemAccessList()
        {
            this.backingData = new Dictionary<string, IStorageItem>();
            this.publicData = new Dictionary<string, AccessListEntry>();
        }

        public string Add(IStorageItem file, string metadata)
        {
            string token = Guid.NewGuid().ToString();
            this.backingData[token] = file;
            this.publicData[token] = new AccessListEntry
            {
                Token = token,
                Metadata = metadata
            };
            return token;
        }

        public bool ContainsItem(string token)
        {
            return this.publicData.ContainsKey(token);
        }

        public IReadOnlyList<AccessListEntry> Entries
        {
            get
            {
                return new List<AccessListEntry>(this.publicData.Values);
            }
        }

        public IAsyncOperation<IStorageItem> GetItemAsync(string token)
        {
            return Task.Run(() => this.backingData[token]).AsAsyncOperation();
        }

        public void Remove(string token)
        {
            this.publicData.Remove(token);
            this.backingData.Remove(token);
        }
    }
}
