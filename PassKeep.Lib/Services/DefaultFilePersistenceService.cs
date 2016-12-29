using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Files;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A Service responsible for saving the document to its default location when necessary.
    /// </summary>
    public class DefaultFilePersistenceService : BindableBase, IDatabasePersistenceService
    {
        private readonly SemaphoreSlim saveSemaphore;
        private readonly IKdbxWriter fileWriter;
        private readonly IDatabaseCandidate defaultSaveFile;
        private readonly ISyncContext syncContext;
        private readonly object ctsLock = new object();

        private int pendingRequests;
        private CancellationTokenSource currentSaveCts;

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="writer">IKdbxWriter used to persist the document.</param>
        /// <param name="candidate">Default location to save the document.</param>
        /// <param name="syncContext">ISyncContext for property change notifications.</param>
        /// <param name="canSave">Stupid dumb hack since StorageFiles suck on phone and have inaccurate attributes.</param>
        public DefaultFilePersistenceService(IKdbxWriter writer, IDatabaseCandidate candidate, ISyncContext syncContext, bool canSave)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            if (syncContext == null)
            {
                throw new ArgumentNullException(nameof(syncContext));
            }

            this.saveSemaphore = new SemaphoreSlim(1, 1);
            this.fileWriter = writer;
            this.defaultSaveFile = candidate;
            this.syncContext = syncContext;
            this.CanSave = canSave;
        }

        /// <summary>
        /// Whether a document is persistable. False for readonly files.
        /// </summary>
        public bool CanSave
        {
            get; private set;
        }

        public bool IsSaving
        {
            get { return this.currentSaveCts != null; }
        }

        /// <summary>
        /// Attempts to asynchronously persist the document to its default location.
        /// If a save is already in progress, it is cancelled and the more recent save should
        /// override it.
        /// </summary>
        /// <param name="document">The KdbxDocument to persist.</param>
        /// <returns>A Task representing whether the save was successful.</returns>
        public async Task<bool> Save(KdbxDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (!this.CanSave)
            {
                return false;
            }

            // Lock to avoid a race condition between checking not null and cancelling
            bool firePropertyChanged = false;
            lock (this.ctsLock)
            {
                if (this.IsSaving)
                {
                    this.currentSaveCts.Cancel();
                }
                else
                {
                    // We only fire PropertyChanged for false -> true if the
                    // transition is actually happening. If we are pre-empting
                    // a save in progress, then it is not useful to fire the
                    // event.
                    firePropertyChanged = true;
                }

                this.pendingRequests++;
            }

            // Cancelling above may cause a previous save to wrap up faster, but we do still need 
            // to wait for it to wrap up.
            await this.saveSemaphore.WaitAsync();

            // Inside the semaphore it is impossible for a save to already be in progress.
            // This is because we clean up the current save at the end of the semaphore, so if
            // we just entered it, there is no pending operation.
            // However, we still want to lock around the CTS in case another save starts right
            // away and needs to cancel this one.
            lock (this.ctsLock)
            {
                Dbg.Assert(this.currentSaveCts == null);
                
                this.pendingRequests--;
                if (this.pendingRequests == 0)
                {
                    // If pendingRequests > 0, then at least one more recent call to
                    // Save is currently stalled at the semaphore.
                    // We only kick off this save if that's NOT true.
                    this.currentSaveCts = new CancellationTokenSource();
                }
            }

            // Only proceed with this save attempt if there are no pending requests...
            // otherwise this block is skipped and we immediately release the semaphore
            // and return false.
            bool writeResult = false;
            if (this.currentSaveCts != null)
            {
                if (firePropertyChanged)
                {
#pragma warning disable CS4014 // No need to await this to continue saving.
                    this.syncContext.Post(() => OnPropertyChanged(nameof(this.IsSaving)));
#pragma warning restore CS4014
                }

                // Do the write to a temporary file until it's finished successfully.
                StorageFile outputFile = await GetTemporaryFile();
                using (IRandomAccessStream fileStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
                    {
                        writeResult = await this.fileWriter.Write(fileStream, document, this.currentSaveCts.Token);
                    }
                }

                if (writeResult)
                {
                    Task replaceTask = this.defaultSaveFile.ReplaceWithAsync(outputFile);
                    try
                    {
                        await replaceTask;
                    }
                    catch (Exception)
                    {
                    }
                }

                try
                {
                    // Make a good-faith effort to delete the temp file, due
                    // to reports that Windows might not handle this automatically.
                    await outputFile.DeleteAsync();
                }
                catch (Exception e)
                {
                    Dbg.Trace($"Caught exception during temp file cleanup: {e}");
                }

                // At this point we are done with all file IO - clean up and let any
                // pending saves do their thing.
                firePropertyChanged = false;
                lock (this.ctsLock)
                {
                    this.currentSaveCts.Dispose();
                    this.currentSaveCts = null;
                    if (this.pendingRequests == 0)
                    {
                        firePropertyChanged = true;
                    }
                }

                // We only update IsSaving if nothing else is pending - if another save
                // is already queued, it's just going to flip this back to true immediately.
                if (firePropertyChanged)
                {
#pragma warning disable CS4014 // No need to await this to continue saving.
                    this.syncContext.Post(() => OnPropertyChanged(nameof(this.IsSaving)));
#pragma warning restore CS4014
                }
            }

            this.saveSemaphore.Release();

            return writeResult;
        }

        /// <summary>
        /// Generates a writable file in the %temp% directory.
        /// </summary>
        /// <returns>A StorageFile that can be used for temporary writing.</returns>
        private async Task<StorageFile> GetTemporaryFile()
        {
            return await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                $"{Guid.NewGuid()}.kdbx",
                CreationCollisionOption.ReplaceExisting
            );
        }
    }
}
