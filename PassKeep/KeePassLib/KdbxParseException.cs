﻿using System;

namespace PassKeep.KeePassLib
{
    /// <summary>
    /// An Exception representing a failure to parse a KeePass document file.
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

            Error = error;
        }
    }
}
