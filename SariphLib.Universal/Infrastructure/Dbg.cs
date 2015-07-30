using System.Diagnostics;

namespace SariphLib.Infrastructure
{
    /// <summary>
    /// Helpers for debugging.
    /// </summary>
    public class Dbg
    {
        /// <summary>
        /// Asserts a condition, breaking if it is false.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            if (!condition)
            {
                Dbg.Trace("ASSERTION FAILED!");
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
                Dbg.Trace("ASSERTION FAILED: {0}", message);
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
            Debug.WriteLine(format, args);
        }
    }
}
