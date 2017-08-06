// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Files;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace PassKeep.Contracts.Models
{
    /// <summary>
    /// An interface for allowing access to saved databases.
    /// </summary>
    /// <remarks>
    /// Serves as an abstraction over WinRT file access lists,
    /// for test purposes.
    /// </remarks>
    public interface IDatabaseAccessList
    {
        /// <summary>
        /// Adds the specified file to the access list.
        /// </summary>
        /// <param name="file">The file to add.</param>
        /// <param name="metadata">The metadata to associate with the file.</param>
        /// <returns>A token used for future reference.</returns>
        string Add(ITestableFile file, string metadata);

        /// <summary>
        /// Fetches the specified file from the access list.
        /// </summary>
        /// <param name="token">A reference to use for fetching.</param>
        /// <returns>An operation that will result in the desired file.</returns>
        Task<ITestableFile> GetFileAsync(string token);

        /// <summary>
        /// Whether this access list contains the specified file.
        /// </summary>
        /// <param name="token">A reference to use for comparison.</param>
        /// <returns>Whether the specified file is in the access list.</returns>
        bool ContainsItem(string token);

        /// <summary>
        /// Removes the specified file from the access list.
        /// </summary>
        /// <param name="token">A reference to use for removal.</param>
        void Remove(string token);

        /// <summary>
        /// A readonly listing of all entries in this access list.
        /// </summary>
        IReadOnlyList<AccessListEntry> Entries
        {
            get;
        }
    }
}
