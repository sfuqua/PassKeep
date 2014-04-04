using PassKeep.Lib.Contracts.KeePass;
using System;

namespace PassKeep.Lib.KeePass
{
    /// <summary>
    /// An Exception representing a failure to parse a KeePass database file.
    /// </summary>
    public class KdbxParseException : Exception
    {
        /// <summary>
        /// The ReaderResult that caused this Exception.
        /// </summary>
        public ReaderResult Error
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the Exception with the specified ReaderResult.
        /// </summary>
        /// <param name="error">The ReaderResult causing this Exception.</param>
        public KdbxParseException(ReaderResult error)
        {
            if (!error.IsError)
            {
                throw new ArgumentException("The provided ReaderResult is not an error.", "error");
            }

            this.Error = error;
        }

        /// <summary>
        /// Initializes the Exception directly from a parser code.
        /// </summary>
        /// <param name="error">The parser code causing this Exception.</param>
        public KdbxParseException(KdbxParserCode error)
            : this(new ReaderResult(error))
        { }
    }
}
