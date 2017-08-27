// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Files;
using System;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

namespace SariphLib.Diagnostics
{
    /// <summary>
    /// Represents a logging target that an app can write to.
    /// </summary>
    public interface ITraceLogger : IDisposable
    {
        /// <summary>
        /// Logs an event with the specified name, with no payload.
        /// </summary>
        /// <param name="eventName">The name of the event to log.</param>
        void LogEvent(string eventName);

        /// <summary>
        /// Logs an event with the desired payload.
        /// </summary>
        /// <param name="eventName">The name of the event to log.</param>
        /// <param name="fields">Payload to associate with the event.</param>
        void LogEvent(string eventName, LoggingFields fields);

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
