using SariphLib.Diagnostics;
using SariphLib.Files;
using System;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// An event tracer that fires .NET events when tracing occurs.
    /// </summary>
    public class MockEventLogger : IEventTracer, IEventLogger
    {
        private bool isTracing;

        /// <summary>
        /// Indicates that an event has been logged by this instance.
        /// </summary>
        public event EventHandler EventTraced;

        /// <summary>
        /// Fired when the tracing session opens.
        /// </summary>
        public event EventHandler TraceStarted;

        /// <summary>
        /// Fired when the tracing session stops.
        /// </summary>
        public event EventHandler TraceStopped;

        public bool IsTracing
        {
            get { return this.isTracing; }
            set
            {
                bool wasTracing = this.isTracing;
                this.isTracing = value;

                if (wasTracing && !value)
                {
                    TraceStopped?.Invoke(this, EventArgs.Empty);
                }
                else if (!wasTracing && value)
                {
                    TraceStarted?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Stops emitting events.
        /// </summary>
        public void CancelTrace()
        {
            IsTracing = false;
        }

        /// <summary>
        /// Stops emitting events.
        /// </summary>
        public void Dispose()
        {
            IsTracing = false;
        }

        /// <summary>
        /// Fires <see cref="EventTraced"/> if the instance is currently tracing.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="verbosity"></param>
        public void LogEvent(string eventName, EventVerbosity verbosity)
        {
            if (IsTracing)
            {
                EventTraced?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Fires <see cref="EventTraced"/> if the instance is currently tracing.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="fields"></param>
        /// <param name="verbosity"></param>
        public void LogEvent(string eventName, LoggingFields fields, EventVerbosity verbosity)
        {
            LogEvent(eventName, verbosity);
        }

        /// <summary>
        /// Starts emitting events.
        /// </summary>
        public void StartTrace()
        {
            IsTracing = true;
        }

        /// <summary>
        /// Stops tracing and returns a <see cref="MockStorageFile"/>.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Task<ITestableFile> StopTraceAndSaveAsync(IStorageFolder folder, string fileName)
        {
            IsTracing = false;
            return Task.FromResult<ITestableFile>(new MockStorageFile { Name = fileName });
        }
    }
}
