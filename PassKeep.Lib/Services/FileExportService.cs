using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using SariphLib.Files;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A basic implementation of <see cref="IFileExportService"/> that copies the
    /// specified file to N desired locations.
    /// </summary>
    public sealed class FileExportService : IFileExportService
    {
        private readonly IDatabaseAccessList accessList;

        /// <summary>
        /// Initializes the service using the provided database access list.
        /// </summary>
        /// <param name="accessList">The list from which to retrieve database candidates.</param>
        public FileExportService(IDatabaseAccessList accessList)
        {
            if (accessList == null)
            {
                throw new ArgumentNullException(nameof(accessList));
            }

            this.accessList = accessList;
        }

        /// <summary>
        /// Fired when the service is attempting an export and requires
        /// an export location from subscribers.
        /// </summary>
        public event TypedEventHandler<IFileExportService, FileRequestedEventArgs> Exporting;

        /// <summary>
        /// Asynchronously exports the specified file to the specified location.
        /// </summary>
        /// <param name="file">The file to export.</param>
        /// <param name="targetLocation">The location to export to.</param>
        /// <returns>A task that resolves to the exported file locations.</returns>
        public async Task<IEnumerable<ITestableFile>> ExportAsync(StoredFileDescriptor file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            // If this is not a valid descriptor it should be forgotten
            ITestableFile fileToCopy = await this.accessList.GetFileAsync(file.Token).ConfigureAwait(false);
            if (fileToCopy == null)
            {
                file.ForgetCommand.Execute(null);
                return Enumerable.Empty<ITestableFile>();
            }

            FileRequestedEventArgs eventArgs = new FileRequestedEventArgs(file.Metadata);
            Exporting?.Invoke(this, eventArgs);

            // After resolving deferrals, we should have a list of the files we should
            // be copying "file" to.
            await eventArgs.DeferAsync().ConfigureAwait(false);

            List<ITestableFile> copiedFiles = new List<ITestableFile>();

            // Copy fileToCopy as needed
            foreach (ITestableFile target in eventArgs.Files)
            {
                try
                {
                    await fileToCopy.AsIStorageFile.CopyAndReplaceAsync(target.AsIStorageFile)
                        .AsTask().ConfigureAwait(false);
                    copiedFiles.Add(target);
                }
                catch (Exception ex)
                {
                    Dbg.Assert(false, "Should not have problems exporting files");
                    Dbg.Trace($"Failed to export: {ex}");
                }
            }

            return copiedFiles;
        }
    }
}
