using PassKeep.Lib.Contracts.KeePass;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Contracts.Models
{
    /// <summary>
    /// Represents a file that is a potential KeePass database.
    /// </summary>
    public interface IDatabaseCandidate : INotifyPropertyChanged
    {
        /// <summary>
        /// The name of the candidate file.
        /// </summary>
        string FileName
        {
            get;
        }

        /// <summary>
        /// When the candidate file was last updated.
        /// </summary>
        DateTimeOffset? LastModified
        {
            get;
        }

        /// <summary>
        /// The size of the candidate file in bytes.
        /// </summary>
        ulong Size
        {
            get;
        }

        /// <summary>
        /// Asynchronously opens and returns a random access stream over
        /// the contents of this file, for reading.
        /// </summary>
        /// <returns>A Task representing an IRandomAccessStream over the data.</returns>
        Task<IRandomAccessStream> GetRandomReadAccessStreamAsync();

        /// <summary>
        /// Asynchronously stomps the content of this candidate with the contents
        /// of the specified IStorageFile.
        /// </summary>
        /// <param name="file">The file with which to replace this data.</param>
        /// <returns>A Task representing the operation.</returns>
        Task ReplaceWithAsync(IStorageFile file);
    }
}
