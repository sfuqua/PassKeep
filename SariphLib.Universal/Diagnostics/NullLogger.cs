// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using Windows.Foundation.Diagnostics;

namespace SariphLib.Diagnostics
{
    /// <summary>
    /// An <see cref="IEventLogger"/> implementation that does nothing.
    /// </summary>
    public class NullLogger : IEventLogger
    {
        /// <summary>
        /// Static helper instance.
        /// </summary>
        public static readonly NullLogger Instance = new NullLogger();

        /// <summary>
        /// No-op.
        /// </summary>
        public void Dispose()
        {
            return;
        }

        /// <summary>
        /// No-op.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="verbosity"></param>
        public void LogEvent(string eventName, EventVerbosity verbosity)
        {
            return;
        }

        /// <summary>
        /// No-op.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="fields"></param>
        /// <param name="verbosity"></param>
        public void LogEvent(string eventName, LoggingFields fields, EventVerbosity verbosity)
        {
            return;
        }

        /// <summary>
        /// No-op.
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="context"></param>
        /// <param name="verbosity"></param>
        /// <param name="method"></param>
        public void Trace(string caller, string context, EventVerbosity verbosity, string method)
        {
            return;
        }
    }
}
