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
    /// A trace logger built on Event Tracing for Windows (ETW).
    /// </summary>
    public sealed class EtwTraceLogger : IEventLogger, IEventTracer
    {
        private LoggingChannel logger;
        private LoggingSession traceSession;

        /// <summary>
        /// Initializes an ETW provider with desired attributes.
        /// </summary>
        /// <param name="providerName">The provider's name.</param>
        /// <param name="providerId">The provider's GUID.</param>
        public EtwTraceLogger(string providerName, Guid providerId)
        {
            this.logger = new LoggingChannel(providerName, null, providerId);
        }

        /// <summary>
        /// Logs an ETW event with the desired name.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="verbosity">The verbosity to log the event with.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void LogEvent(string eventName, EventVerbosity verbosity)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("this");
            }

            this.logger.LogEvent(eventName, null, GetLoggingLevel(verbosity));
        }

        /// <summary>
        /// Logs an event with the desired payload.
        /// </summary>
        /// <param name="eventName">The name of the event to log.</param>
        /// <param name="fields">Payload to associate with the event.</param>
        /// <param name="verbosity">The verbosity to log the event with.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void LogEvent(string eventName, LoggingFields fields, EventVerbosity verbosity)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("this");
            }

            this.logger.LogEvent(eventName, fields, GetLoggingLevel(verbosity));
        }

        /// <summary>
        /// Ends the current trace session and saves data to a file.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <returns>A Task that resolves to the saved file.</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException">A trace has not been started by <see cref="StartTrace"/>.</exception>
        public async Task<ITestableFile> StopTraceAndSaveAsync(IStorageFolder folder, string fileName)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("this");
            }

            if (this.traceSession == null)
            {
                throw new InvalidOperationException("Cannot save trace; it was never started");
            }
            
            StorageFile file = await this.traceSession.SaveToFileAsync(folder, fileName)
                .AsTask().ConfigureAwait(false);

            CancelTrace();

            return file.AsWrapper();
        }

        /// <summary>
        /// Starts a new trace.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException">A trace is currently ongoing.</exception>
        public void StartTrace()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("this");
            }

            if (this.traceSession != null)
            {
                throw new InvalidOperationException("Already tracing");
            }

            this.traceSession = new LoggingSession("etwTrace");
            this.traceSession.AddLoggingChannel(this.logger);
        }

        /// <summary>
        /// Ends a trace without saving.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException">No trace is active.</exception>
        public void CancelTrace()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("this");
            }

            if (this.traceSession == null)
            {
                throw new InvalidOperationException("No trace is in progress");
            }

            this.traceSession.RemoveLoggingChannel(this.logger);
            this.traceSession.Dispose();
            this.traceSession = null;
        }

        /// <summary>
        /// Maps SariphLib <see cref="EventVerbosity"/> values to ETW <see cref="LoggingLevel"/> values.
        /// </summary>
        /// <param name="verbosity"></param>
        /// <returns></returns>
        private static LoggingLevel GetLoggingLevel(EventVerbosity verbosity)
        {
            switch (verbosity)
            {
                case EventVerbosity.Critical:
                    return LoggingLevel.Critical;
                case EventVerbosity.Info:
                    return LoggingLevel.Information;
                case EventVerbosity.Verbose:
                    return LoggingLevel.Verbose;
                default:
                    throw new InvalidOperationException("Invalid enum value");
            }
        }

        #region IDisposable Support

        private bool disposed = false;

        /// <summary>
        /// Stops any active traces and cleans up the ETW resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        // Private implementation of IDisposable pattern.
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Free managed ETW resources
                    if (this.traceSession != null)
                    {
                        CancelTrace();
                    }

                    this.logger?.Dispose();
                    this.logger = null;
                }

                this.disposed = true;
            }
        }

        #endregion
    }
}
