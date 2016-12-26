using PassKeep.Contracts.Models;
using SariphLib.Files;
using SariphLib.Infrastructure;
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
        private const string TempFolderSubdirectory = "ReadOnlyCache";
        private const string OneDrivePathFragment = @"\microsoft.microsoftskydrive_8wekyb3d8bbwe\";

        private readonly bool isAppOwned;
        private readonly ITestableFile candidate;

        private StorageFile cachedReadOnlyCopy;

        private DateTimeOffset? _lastModified;
        private ulong _size;

        /// <summary>
        /// Constructs a database candidate from the specified IStorageFile.
        /// </summary>
        /// <param name="candidate">The file representing the database candidate.</param>
        /// <param name="isAppOwned">Whether this file is controlled by the app.</param>
        public StorageFileDatabaseCandidate(ITestableFile candidate, bool isAppOwned)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            this.candidate = candidate;
            this.isAppOwned = isAppOwned;
            LastModified = null;
            Size = 0;

            // XXX:
            // This is horrible, obviously. It's a hack and it isn't localized.
            // That's because it should be temporary, until Microsoft fixes OneDrive.
            this.CannotRememberText = null;
            if (this.candidate.AsIStorageItem.Path.Contains(OneDrivePathFragment))
            {
                this.CannotRememberText =
                    "Disabled for OneDrive on phone - it currently does not provide apps with persistent access to your cloud files";
            }
        }

        public string CannotRememberText
        {
            get; private set;
        }

        /// <summary>
        /// The name of the candidate file.
        /// </summary>
        public string FileName
        {
            get
            {
                return this.candidate.AsIStorageItem.Name;
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
        /// Whether PassKeep "owns" or controls this file. Can be used 
        /// to notify the user of which candidates are roaming.
        /// </summary>
        public bool IsAppOwned
        {
            get
            {
                return this.isAppOwned;
            }
        }

        /// <summary>
        /// The StorageItem represented by this candidate.
        /// </summary>
        public ITestableFile File
        {
            get
            {
                return this.candidate;
            }
        }

        /// <summary>
        /// Creates a readonly copy of the database for later access.
        /// </summary>
        /// <remarks>This works around a OneDrive bug where the database is deleted once
        /// a keyfile is selected.</remarks>
        /// <returns></returns>
        public async Task GenerateReadOnlyCachedCopyAsync()
        {
            StorageFolder folder = ApplicationData.Current.TemporaryFolder;
            try
            {
                folder = await folder.GetFolderAsync(TempFolderSubdirectory);
            }
            catch (System.IO.FileNotFoundException)
            {
                folder = await folder.CreateFolderAsync(TempFolderSubdirectory);
            }

            // If a cached file already exists, make sure we can write over it.
            try
            {
                StorageFile existingFile = await folder.GetFileAsync(File.AsIStorageItem.Name);
                await existingFile.ClearFileAttributesAsync(FileAttributes.ReadOnly);
            }
            catch (Exception e)
            {
                Dbg.Trace(
                    "Warning: Could not clear readonly flag on existing readonly cached file {0}. Exception: {1}",
                    File.AsIStorageItem.Name,
                    e
                );
            }
            
            StorageFile copy = await this.File.AsIStorageFile.CopyAsync(folder, File.AsIStorageItem.Name, NameCollisionOption.ReplaceExisting);
            await copy.SetReadOnlyAsync();

            this.cachedReadOnlyCopy = copy;
        }

        /// <summary>
        /// Asynchronously opens and returns a random access stream over
        /// the contents of this file, for reading, seeked to 0.
        /// </summary>
        /// <returns>A Task representing an IRandomAccessStream over the data.</returns>
        public async Task<IRandomAccessStream> GetRandomReadAccessStreamAsync()
        {
            Dbg.Assert(this.cachedReadOnlyCopy != null);

            BasicProperties properties = await this.candidate.AsIStorageFile.GetBasicPropertiesAsync();
            this.LastModified = properties.DateModified;
            this.Size = properties.Size;

            IRandomAccessStream stream = await this.cachedReadOnlyCopy.OpenReadAsync();

            return stream;
        }

        /// <summary>
        /// Asynchronously stomps the content of this candidate with the contents
        /// of the specified IStorageFile.
        /// </summary>
        /// <param name="file">The file with which to replace this data.</param>
        /// <returns>A Task representing the operation.</returns>
        public Task ReplaceWithAsync(IStorageFile file)
       { 
            return file.CopyAndReplaceAsync(this.candidate.AsIStorageFile).AsTask();
        }
    }
}
