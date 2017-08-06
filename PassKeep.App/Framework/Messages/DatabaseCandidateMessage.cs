// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Files;
using Windows.Storage;

namespace PassKeep.Framework.Messages
{
    /// <summary>
    /// A message for communicating that a new database file was selected for unlocking.
    /// </summary>
    public sealed class DatabaseCandidateMessage : MessageBase
    {
        /// <summary>
        /// Constructs the message.
        /// </summary>
        public DatabaseCandidateMessage(ITestableFile file, bool isSample)
        {
            File = file;
            IsSample = isSample;
        }

        public ITestableFile File
        {
            get;
            private set;
        }

        public bool IsSample
        {
            get;
            private set;
        }
    }
}
