using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A Service responsible for saving the database to its default location when necessary.
    /// </summary>
    public class DefaultFilePersistenceService : IDatabasePersistenceService
    {
        private IKdbxWriter fileWriter;
        private StorageFile defaultSaveFile;

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="writer">IKdbxWriter used to persist the database.</param>
        /// <param name="defaultSaveFile">Default location to save the database.</param>
        public DefaultFilePersistenceService(IKdbxWriter writer, StorageFile defaultSaveFile)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (defaultSaveFile == null)
            {
                throw new ArgumentNullException("defaultSaveFile");
            }

            this.fileWriter = writer;
            this.defaultSaveFile = defaultSaveFile;
        }

        /// <summary>
        /// Attempts to asynchronously persist the database to its default location.
        /// </summary>
        /// <param name="document">The KdbxDocument to persist.</param>
        /// <param name="token">A CancellationToken for the operation.</param>
        /// <returns>A Task representing whether the save was successful.</returns>
        public async Task<bool> Save(KdbxDocument document, CancellationToken token)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            return await this.fileWriter.Write(this.defaultSaveFile, document, token);
        }
    }
}
