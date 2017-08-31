// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Files;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SariphLib.Diagnostics
{
    /// <summary>
    /// Represents a trace session that can record events.
    /// </summary>
    public interface IEventTracer : IDisposable
    {
        /// <summary>
        /// Starts a recording session that allows saving events to a file.
        /// </summary>
        void StartTrace();

        /// <summary>
        /// Ends a previously started recording session without saving.
        /// </summary>
        void CancelTrace();

        /// <summary>
        /// Writes recorded events to the desired folder.
        /// </summary>
        /// <param name="folder">The folder to save the log to.</param>
        /// <param name="fileName">The name of the file to save.</param>
        /// <returns>A Task that resolves to the generated log file.</returns>
        Task<ITestableFile> StopTraceAndSaveAsync(IStorageFolder folder, string fileName);
    }
}
