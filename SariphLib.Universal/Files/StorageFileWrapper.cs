using System;
using Windows.Storage;

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
