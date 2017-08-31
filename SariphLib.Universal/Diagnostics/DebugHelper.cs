// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Diagnostics;
using Windows.Foundation.Diagnostics;

namespace SariphLib.Diagnostics
{
    /// <summary>
    /// Helpers for debugging - assertions and tracing.
    /// </summary>
    public static class DebugHelper
    {
        private static IEventLogger _Logger = NullLogger.Instance;

        /// <summary>
        /// Allows an app to specify the logger to use for debug tracing.
        /// </summary>
        public static IEventLogger Logger
        {
            set
            {
                _Logger = value ?? throw new ArgumentNullException(nameof(value));
            }
            private get { return _Logger; }
        }

        /// <summary>
        /// Asserts a condition, breaking if it is false.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            if (!condition)
            {
                Logger.LogEvent("AssertionFailed", EventVerbosity.Critical);
                Debug.WriteLine("AssertionFailed");
                Debugger.Break();
            }
        }

        /// <summary>
        /// Asserts a condition with an explanation, breaking if it is false.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="message">The statement being asserted.</param>
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                LoggingFields fields = new LoggingFields();
                fields.AddString("Message", message);
                Logger.LogEvent("AssertionFailed", fields, EventVerbosity.Critical);

                Debug.WriteLine("AssertionFailed: " + message);
                Debugger.Break();
            }
        }

        /// <summary>
        /// Debug traces the given message.
        /// </summary>
        /// <param name="message"></param>
        [Conditional("DEBUG")]
        public static void Trace(string message)
        {
            LoggingFields fields = new LoggingFields();
            fields.AddString("Message", message);
            Logger.LogEvent("DebugTrace", fields, EventVerbosity.Verbose);
            Debug.WriteLine(message);
        }

        /// <summary>
        /// Debug traces the given format string.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [Conditional("DEBUG")]
        public static void Trace(string format, params object[] args)
        {
            Trace(String.Format(format, args));
        }
    }
}
