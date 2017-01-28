using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Models;
using SariphLib.Files;
using SariphLib.Infrastructure;
using System;
using System.Threading.Tasks;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A basic implementation of <see cref="IFileExportService"/> that copies the
    /// specified file to N desired locations.
    /// </summary>
    public sealed class FileExportService : IFileExportService
    {
        private readonly IDatabaseAccessList accessList;
        private readonly IFileAccessService fileService;

        /// <summary>
        /// Initializes the service using the provided database access list.
        /// </summary>
        /// <param name="accessList">The list from which to retrieve database candidates.</param>
        /// <param name="fileService">The service used to access the filesystem.</param>
        public FileExportService(IDatabaseAccessList accessList, IFileAccessService fileService)
        {
            if (accessList == null)
            {
                throw new ArgumentNullException(nameof(accessList));
            }

            if (fileService == null)
            {
                throw new ArgumentNullException(nameof(fileService));
            }

            this.accessList = accessList;
            this.fileService = fileService;
        }

        /// <summary>
        /// Asynchronously exports the specified file to the specified location.
        /// </summary>
        /// <param name="file">The file to export.</param>
        /// <param name="targetLocation">The location to export to.</param>
        /// <returns>A task that resolves to the exported file location.</returns>
        public async Task<ITestableFile> ExportAsync(StoredFileDescriptor file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            // If this is not a valid descriptor it should be forgotten
            ITestableFile fileToCopy = await this.accessList.GetFileAsync(file.Token).ConfigureAwait(false);
            if (fileToCopy == null)
            {
                await file.ForgetCommand.ExecuteAsync(null).ConfigureAwait(false);
                return null;
            }

            ITestableFile savedFile = await this.fileService.PickFileForSaveAsync(file.Metadata)
                .ConfigureAwait(false);

            if (savedFile != null)
            {
                try
                {
                    await fileToCopy.AsIStorageFile.CopyAndReplaceAsync(savedFile.AsIStorageFile)
                        .AsTask().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Dbg.Assert(false, "Should not have problems exporting files");
                    Dbg.Trace($"Failed to export: {ex}");
                    return null;
                }
            }

            return savedFile;
        }
    }
}
