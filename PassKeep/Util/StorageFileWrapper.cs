using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Util
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

        public IStorageItem AsIStorageItem
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
            throw new NotImplementedException();
            //return WrappedFile.ClearFileAttributesAsync(FileAttributes.ReadOnly);
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
