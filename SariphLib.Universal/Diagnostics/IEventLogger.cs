// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using Windows.Foundation.Diagnostics;

namespace SariphLib.Diagnostics
{
    /// <summary>
    /// Represents a logging target that an app can write to.
    /// </summary>
    public interface IEventLogger : IDisposable
    {
        /// <summary>
        /// Logs an event with the specified name, with no payload.
        /// </summary>
        /// <param name="eventName">The name of the event to log.</param>
        /// <param name="verbosity">The verbosity to log the event with.</param>
        void LogEvent(string eventName, EventVerbosity verbosity);

        /// <summary>
        /// Logs an event with the desired payload.
        /// </summary>
        /// <param name="eventName">The name of the event to log.</param>
        /// <param name="fields">Payload to associate with the event.</param>
        /// <param name="verbosity">The verbosity to log the event with.</param>
        void LogEvent(string eventName, LoggingFields fields, EventVerbosity verbosity);
    }

    /// <summary>
    /// Helpers for creating fields for logging.
    /// </summary>
    public static partial class LoggingExtensions
    {
        /// <summary>
        /// Given an existing <see cref="LoggingFields"/> reference, adds additional fields for an exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static LoggingFields AugmentLoggingFields(this Exception exception, LoggingFields fields)
        {
            fields.AddString("ExceptionType", exception?.GetType()?.ToString() ?? "[None]");
            fields.AddString("ExceptionMessage", exception?.Message ?? "[None]");
            fields.AddString("ExceptionStack", exception?.StackTrace ?? "[None]");
            fields.AddString("InnerException", exception?.InnerException?.ToString() ?? "[None]");

            return fields;
        }

        /// <summary>
        /// Creates a <see cref="LoggingFields"/> instance representing an exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static LoggingFields ToLoggingFields(this Exception exception)
        {
            return exception.AugmentLoggingFields(new LoggingFields());
        }
    }
}
