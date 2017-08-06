// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SariphLib.Files
{
    /// <summary>
    /// Helper interface that aggregates the interfaces that <see cref="StorageFile"/>
    /// implements. Methods that take a storage file should use this instead in conjunction
    /// with <see cref="StorageFileWrapper"/>.
    /// </summary>
    public interface ITestableFile
    {
        IStorageFile AsIStorageFile { get; }

        IStorageFile2 AsIStorageFile2 { get; }

        IStorageItem AsIStorageItem { get; }

        IStorageItem2 AsIStorageItem2 { get; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the time that this file was created.
        /// </summary>
        DateTimeOffset DateCreated { get; }

        /// <summary>
        /// Asynchronously retrieves the time that this file was last modified.
        /// </summary>
        /// <returns>A task that resolves to when the file was last modified.</returns>
        Task<DateTimeOffset> GetLastModifiedAsync();

        /// <summary>
        /// Asynchronously clears the read-only flag of the file.
        /// </summary>
        /// <returns>A task that completes when the file is updated.</returns>
        Task ClearReadOnlyFlag();
    }
}
