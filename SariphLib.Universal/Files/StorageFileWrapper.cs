// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace SariphLib.Files
{
    /// <summary>
    /// Helper that abstracts the functionality of a <see cref="StorageFile"/> against
    /// a testable interface.
    /// </summary>
    public sealed class StorageFileWrapper : ITestableFile
    {
        private readonly StorageFile wrappedFile;

        /// <summary>
        /// Initializes the wrapper.
        /// </summary>
        /// <param name="wrappedFile">The actual file to wrap.</param>
        public StorageFileWrapper(StorageFile wrappedFile)
        {
            if (wrappedFile == null)
            {
                throw new ArgumentNullException(nameof(wrappedFile));
            }

            this.wrappedFile = wrappedFile;
        }

        public IStorageFile AsIStorageFile
        {
            get
            {
                return WrappedFile;
            }
        }

        public IStorageFile2 AsIStorageFile2
        {
            get
            {
                return WrappedFile;
            }
        }

        public IStorageItem AsIStorageItem
        {
            get
            {
                return WrappedFile;
            }
        }

        public IStorageItem2 AsIStorageItem2
        {
            get
            {
                return WrappedFile;
            }
        }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string Name
        {
            get { return WrappedFile.Name; }
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        public string Path
        {
            get { return WrappedFile.Path; }
        }

        /// <summary>
        /// Gets the time that this file was created.
        /// </summary>
        public DateTimeOffset DateCreated
        {
            get { return WrappedFile.DateCreated; }
        }

        /// <summary>
        /// Asynchronously retrieves the time that this file was last modified.
        /// </summary>
        /// <returns>A task that resolves to when the file was last modified.</returns>
        public async Task<DateTimeOffset> GetLastModifiedAsync()
        {
            return (await WrappedFile.GetBasicPropertiesAsync()).DateModified;
        }

        /// <summary>
        /// Asynchronously clears the read-only flag of the file.
        /// </summary>
        /// <returns>A task that completes when the file is updated.</returns>
        public Task ClearReadOnlyFlag()
        {
            return WrappedFile.ClearFileAttributesAsync(FileAttributes.ReadOnly);
        }

        /// <summary>
        /// Provides acccess to the <see cref="StorageFile"/> that provides
        /// functionality for this instance.
        /// </summary>
        public StorageFile WrappedFile
        {
            get
            {
                return this.wrappedFile;
            }
        }
    }
}
