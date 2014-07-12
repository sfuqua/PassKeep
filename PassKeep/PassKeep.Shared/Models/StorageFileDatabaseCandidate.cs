using PassKeep.Contracts.Models;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace PassKeep.Models
{
    /// <summary>
    /// A candidate database file represented by an IStorageFile.
    /// </summary>
    public class StorageFileDatabaseCandidate : BindableBase, IDatabaseCandidate
    {
        private IStorageFile candidate;

        private DateTimeOffset? _lastModified;
        private ulong _size;

        /// <summary>
        /// Constructs a database candidate form the specified IStorageFile.
        /// </summary>
        /// <param name="candidate">The file representing the database candidate.</param>
        public StorageFileDatabaseCandidate(IStorageFile candidate)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException("candidate");
            }

            this.candidate = candidate;
            this.LastModified = null;
            this.Size = 0;
        }

        /// <summary>
        /// The name of the candidate file.
        /// </summary>
        public string FileName
        {
            get
            {
                return this.candidate.Name;
            }
        }

        /// <summary>
        /// When the candidate file was last updated.
        /// </summary>
        public DateTimeOffset? LastModified
        {
            get
            {
                return this._lastModified;
            }
            set
            {
                TrySetProperty(ref this._lastModified, value);
            }
        }

        /// <summary>
        /// The size of the candidate file in bytes.
        /// </summary>
        public ulong Size
        {
            get
            {
                return this._size;
            }
            set
            {
                TrySetProperty(ref this._size, value);
            }
        }

        /// <summary>
        /// Asynchronously opens and returns a random access stream over
        /// the contents of this file.
        /// </summary>
        /// <returns>A Task representing an IRandomAccessStream over the data.</returns>
        public async Task<IRandomAccessStream> GetRandomAccessStreamAsync()
        {
            BasicProperties properties = await this.candidate.GetBasicPropertiesAsync();
            this.LastModified = properties.DateModified;
            this.Size = properties.Size;

            return await this.candidate.OpenReadAsync();
        }
    }
}
