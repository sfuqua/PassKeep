// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Models;
using SariphLib.Files;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// Service for exporting cached files to other locations.
    /// </summary>
    public interface IFileExportService
    {
        /// <summary>
        /// Asynchronously exports the specified file to the specified location.
        /// </summary>
        /// <param name="file">The file to export.</param>
        /// <param name="targetLocation">The location to export to.</param>
        /// <returns>A task that resolves to the exported file location.</returns>
        Task<ITestableFile> ExportAsync(StoredFileDescriptor file);
    }
}
