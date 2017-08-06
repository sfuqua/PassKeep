// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using System;

namespace PassKeep.Lib.KeePass
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
