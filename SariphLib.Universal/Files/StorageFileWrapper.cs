using System;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

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

        public FileAttributes Attributes
        {
            get
            {
                return WrappedFile.Attributes;
            }
        }

        public string ContentType
        {
            get
            {
                return WrappedFile.ContentType;
            }
        }

        public DateTimeOffset DateCreated
        {
            get
            {
                return WrappedFile.DateCreated;
            }
        }

        public string FileType
        {
            get
            {
                return WrappedFile.FileType;
            }
        }

        public string Name
        {
            get
            {
                return WrappedFile.Name;
            }
        }

        public string Path
        {
            get
            {
                return WrappedFile.Path;
            }
        }

        public IAsyncAction CopyAndReplaceAsync(IStorageFile fileToReplace)
        {
            return WrappedFile.CopyAndReplaceAsync(fileToReplace);
        }

        public IAsyncOperation<StorageFile> CopyAsync(IStorageFolder destinationFolder)
        {
            return WrappedFile.CopyAsync(destinationFolder);
        }

        public IAsyncOperation<StorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            return WrappedFile.CopyAsync(destinationFolder, desiredNewName);
        }

        public IAsyncOperation<StorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName, NameCollisionOption option)
        {
            return WrappedFile.CopyAsync(destinationFolder, desiredNewName, option);
        }

        public IAsyncAction DeleteAsync()
        {
            return WrappedFile.DeleteAsync();
        }

        public IAsyncAction DeleteAsync(StorageDeleteOption option)
        {
            return WrappedFile.DeleteAsync(option);
        }

        public IAsyncOperation<BasicProperties> GetBasicPropertiesAsync()
        {
            return WrappedFile.GetBasicPropertiesAsync();
        }

        public IAsyncOperation<StorageFolder> GetParentAsync()
        {
            return WrappedFile.GetParentAsync();
        }

        public bool IsEqual(IStorageItem item)
        {
            return WrappedFile.IsEqual(item);
        }

        public bool IsOfType(StorageItemTypes type)
        {
            return WrappedFile.IsOfType(type);
        }

        public IAsyncAction MoveAndReplaceAsync(IStorageFile fileToReplace)
        {
            return WrappedFile.MoveAndReplaceAsync(fileToReplace);
        }

        public IAsyncAction MoveAsync(IStorageFolder destinationFolder)
        {
            return WrappedFile.MoveAsync(destinationFolder);
        }

        public IAsyncAction MoveAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            return WrappedFile.MoveAsync(destinationFolder, desiredNewName);
        }

        public IAsyncAction MoveAsync(IStorageFolder destinationFolder, string desiredNewName, NameCollisionOption option)
        {
            return WrappedFile.MoveAsync(destinationFolder, desiredNewName, option);
        }

        public IAsyncOperation<IRandomAccessStream> OpenAsync(FileAccessMode accessMode)
        {
            return WrappedFile.OpenAsync(accessMode);
        }

        public IAsyncOperation<IRandomAccessStream> OpenAsync(FileAccessMode accessMode, StorageOpenOptions options)
        {
            return WrappedFile.OpenAsync(accessMode, options);
        }

        public IAsyncOperation<IRandomAccessStreamWithContentType> OpenReadAsync()
        {
            return WrappedFile.OpenReadAsync();
        }

        public IAsyncOperation<IInputStream> OpenSequentialReadAsync()
        {
            return WrappedFile.OpenSequentialReadAsync();
        }

        public IAsyncOperation<StorageStreamTransaction> OpenTransactedWriteAsync()
        {
            return WrappedFile.OpenTransactedWriteAsync();
        }

        public IAsyncOperation<StorageStreamTransaction> OpenTransactedWriteAsync(StorageOpenOptions options)
        {
            return WrappedFile.OpenTransactedWriteAsync(options);
        }

        public IAsyncAction RenameAsync(string desiredName)
        {
            return WrappedFile.RenameAsync(desiredName);
        }

        public IAsyncAction RenameAsync(string desiredName, NameCollisionOption option)
        {
            return WrappedFile.RenameAsync(desiredName, option);
        }
    }
}
