using PassKeep.Contracts.Models;
using SariphLib.Files;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace PassKeep.Models
{
    /// <summary>
    /// An implementation of IDatabaseAccessList that wraps a standard
    /// WinRT IStorageItemAccessList.
    /// </summary>
    /// <remarks>
    /// The primary purpose of doing this is to allow testing with mock
    /// implementations of IStorageItemAccessList. Since the class is not fully mockable,
    /// I defined a new interface to get around "Entries".
    /// </remarks>
    public class DatabaseAccessList : IDatabaseAccessList
    {
        private IStorageItemAccessList accessList;

        /// <summary>
        /// Wraps the specified IStorageItemAccessList in an instance of this class.
        /// </summary>
        /// <param name="accessList">The list to proxy.</param>
        public DatabaseAccessList(IStorageItemAccessList accessList)
        {
            this.accessList = accessList;
        }

        /// <summary>
        /// Adds the specified file to the access list.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <param name="metadata">The metadata to associate with the file.</param>
        /// <returns>A token used for future reference.</returns>
        public string Add(ITestableFile file, string metadata)
        {
            return this.accessList.Add(file.AsIStorageItem, metadata);
        }

        /// <summary>
        /// Fetches the specified file from the access list.
        /// </summary>
        /// <param name="token">A reference to use for fetching.</param>
        /// <returns>An operation that will result in the desired file.</returns>
        public async Task<ITestableFile> GetFileAsync(string token)
        {
            StorageFile file = await this.accessList.GetFileAsync(token)
                .AsTask().ConfigureAwait(false);

            return new StorageFileWrapper(file);
        }

        /// <summary>
        /// Whether this access list contains the specified file.
        /// </summary>
        /// <param name="token">A reference to use for comparison.</param>
        /// <returns>Whether the specified file is in the access list.</returns>
        public bool ContainsItem(string token)
        {
            return this.accessList.ContainsItem(token);
        }

        /// <summary>
        /// Removes the specified file from the access list.
        /// </summary>
        /// <param name="token">A reference to use for removal.</param>
        public void Remove(string token)
        {
            this.accessList.Remove(token);
        }

        /// <summary>
        /// A readonly listing of all entries in this access list.
        /// </summary>
        public IReadOnlyList<AccessListEntry> Entries
        {
            get
            {
                return this.accessList.Entries;
            }
        }
    }
}
