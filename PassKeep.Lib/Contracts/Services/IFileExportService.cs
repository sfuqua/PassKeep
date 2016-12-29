using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using SariphLib.Files;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// Service for exporting cached files to other locations.
    /// </summary>
    public interface IFileExportService
    {
        /// <summary>
        /// Fired when the service is attempting an export and requires
        /// an export location from subscribers.
        /// </summary>
        event TypedEventHandler<IFileExportService, FileRequestedEventArgs> Exporting;

        /// <summary>
        /// Asynchronously exports the specified file to the specified location.
        /// </summary>
        /// <param name="file">The file to export.</param>
        /// <param name="targetLocation">The location to export to.</param>
        /// <returns>A task that resolves to the exported file locations.</returns>
        Task<IEnumerable<ITestableFile>> ExportAsync(StoredFileDescriptor file);
    }
}
