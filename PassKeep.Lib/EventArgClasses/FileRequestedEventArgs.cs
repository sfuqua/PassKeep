// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Eventing;
using SariphLib.Files;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// Handles asynchronously providing files for an export operation.
    /// </summary>
    public class FileRequestedEventArgs : DeferrableEventArgs
    {
        private readonly string suggestedName;
        private readonly List<ITestableFile> providedFiles;

        /// <summary>
        /// Initializes the event args instance with an empty list of files.
        /// </summary>
        /// <param name="suggestedName">The default name to use when asking for a file.</param>
        public FileRequestedEventArgs(string suggestedName)
        {
            this.suggestedName = suggestedName;
            this.providedFiles = new List<ITestableFile>();
        }

        /// <summary>
        /// The default name to use when asking for a file.
        /// </summary>
        public string SuggestedName
        {
            get { return this.suggestedName; }
        }

        /// <summary>
        /// Provides read-only access to the files provided by subscribers.
        /// </summary>
        public ReadOnlyCollection<ITestableFile> Files
        {
            get
            {
                lock (this.providedFiles)
                {
                    return this.providedFiles.AsReadOnly();
                }
            }
        }

        /// <summary>
        /// Adds a file to the list of files provided by subscribers. Null values are ignored.
        /// </summary>
        /// <param name="file">The file to add and eventually return to the event raiser.</param>
        public void AddFile(ITestableFile file)
        {
            if (file != null)
            {
                lock (this.providedFiles)
                {
                    this.providedFiles.Add(file);
                }
            }
        }
    }
}
