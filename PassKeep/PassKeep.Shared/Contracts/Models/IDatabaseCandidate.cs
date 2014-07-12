using PassKeep.Lib.Contracts.KeePass;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
        /// the contents of this file.
        /// </summary>
        /// <returns>A Task representing an IRandomAccessStream over the data.</returns>
        Task<IRandomAccessStream> GetRandomAccessStreamAsync();
    }
}
