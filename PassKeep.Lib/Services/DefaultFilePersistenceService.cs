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
    public class DefaultFilePersistenceService : IDatabasePersistenceService
    {
        private IKdbxWriter fileWriter;
        private IDatabaseCandidate defaultSaveFile;

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="writer">IKdbxWriter used to persist the document.</param>
        /// <param name="candidate">Default location to save the document.</param>
        /// <param name="canSave">Stupid dumb hack since StorageFiles suck on phone and have inaccurate attributes.</param>
        public DefaultFilePersistenceService(IKdbxWriter writer, IDatabaseCandidate candidate, bool canSave)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            this.fileWriter = writer;
            this.defaultSaveFile = candidate;
            this.CanSave = canSave;
        }

        /// <summary>
        /// Whether a document is persistable. False for readonly files.
        /// </summary>
        public bool CanSave
        {
            get; private set;
        }

        /// <summary>
        /// Attempts to asynchronously persist the document to its default location.
        /// </summary>
        /// <param name="document">The KdbxDocument to persist.</param>
        /// <param name="token">A CancellationToken for the operation.</param>
        /// <returns>A Task representing whether the save was successful.</returns>
        public async Task<bool> Save(KdbxDocument document, CancellationToken token)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }


            if (!CanSave)
            {
                return false;
            }

            // Do the write to a temporary file until it's finished successfully.
            StorageFile outputFile = await GetTemporaryFile();
            bool writeResult = false;

            using (IRandomAccessStream fileStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
                {
                    writeResult = await this.fileWriter.Write(fileStream, document, token);
                }
            }

            if (writeResult)
            {
                await this.defaultSaveFile.ReplaceWithAsync(outputFile);
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
