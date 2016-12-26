using SariphLib.Files;
using System;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// An implementation of IStorageFile that doesn't hit the filesystem.
    /// Nothing is supported except Name and Path.
    /// </summary>
    public class MockStorageFile : ITestableFile, IStorageFile, IStorageFile2, IStorageItem, IStorageItem2
    {
        public FileAttributes Attributes
        {
            get { throw new NotImplementedException(); }
        }

        public DateTimeOffset DateCreated
        {
            get { throw new NotImplementedException(); }
        }

        public IAsyncAction DeleteAsync(StorageDeleteOption option)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction DeleteAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<BasicProperties> GetBasicPropertiesAsync()
        {
            throw new NotImplementedException();
        }

        public bool IsOfType(StorageItemTypes type)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get;
            set;
        }

        public string Path
        {
            get;
            set;
        }

        public IAsyncAction RenameAsync(string desiredName, NameCollisionOption option)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction RenameAsync(string desiredName)
        {
            throw new NotImplementedException();
        }

        public string ContentType
        {
            get { throw new NotImplementedException(); }
        }

        public IAsyncAction CopyAndReplaceAsync(IStorageFile fileToReplace)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<StorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName, NameCollisionOption option)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<StorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<StorageFile> CopyAsync(IStorageFolder destinationFolder)
        {
            throw new NotImplementedException();
        }

        public string FileType
        {
            get { throw new NotImplementedException(); }
        }

        public IStorageFile AsIStorageFile
        {
            get
            {
                return this;
            }
        }

        public IStorageFile2 AsIStorageFile2
        {
            get
            {
                return this;
            }
        }

        public IStorageItem AsIStorageItem
        {
            get
            {
                return this;
            }
        }

        public IStorageItem2 AsIStorageItem2
        {
            get
            {
                return this;
            }
        }

        public IAsyncAction MoveAndReplaceAsync(IStorageFile fileToReplace)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction MoveAsync(IStorageFolder destinationFolder, string desiredNewName, NameCollisionOption option)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction MoveAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            throw new NotImplementedException();
        }

        public IAsyncAction MoveAsync(IStorageFolder destinationFolder)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IRandomAccessStream> OpenAsync(FileAccessMode accessMode)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<StorageStreamTransaction> OpenTransactedWriteAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IRandomAccessStreamWithContentType> OpenReadAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IInputStream> OpenSequentialReadAsync()
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<IRandomAccessStream> OpenAsync(FileAccessMode accessMode, StorageOpenOptions options)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<StorageStreamTransaction> OpenTransactedWriteAsync(StorageOpenOptions options)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<StorageFolder> GetParentAsync()
        {
            throw new NotImplementedException();
        }

        public bool IsEqual(IStorageItem item)
        {
            throw new NotImplementedException();
        }
    }
}
