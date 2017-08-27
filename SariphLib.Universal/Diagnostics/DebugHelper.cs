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
    public class DebugHelper
    {
        private readonly ITraceLogger logger;

        public DebugHelper(ITraceLogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asserts a condition, breaking if it is false.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        [Conditional("DEBUG")]
        public void Assert(bool condition)
        {
            if (!condition)
            {
                this.logger.LogEvent("AssertionFailed");
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
        public void Assert(bool condition, string message)
        {
            if (!condition)
            {
                LoggingFields fields = new LoggingFields();
                fields.AddString("Message", message);
                this.logger.LogEvent("AssertionFailed", fields);

                Debug.WriteLine("AssertionFailed: " + message);
                Debugger.Break();
            }
        }

        /// <summary>
        /// Debug traces the given message.
        /// </summary>
        /// <param name="message"></param>
        [Conditional("DEBUG")]
        public void Trace(string message)
        {
            LoggingFields fields = new LoggingFields();
            fields.AddString("Message", message);
            this.logger.LogEvent("DebugTrace", fields);
            Debug.WriteLine(message);
        }

        /// <summary>
        /// Debug traces the given format string.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [Conditional("DEBUG")]
        public void Trace(string format, params object[] args)
        {
            Trace(String.Format(format, args));
        }
    }
}
