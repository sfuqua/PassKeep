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
}
