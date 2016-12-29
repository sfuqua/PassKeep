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
        private readonly List<ITestableFile> providedFiles;

        /// <summary>
        /// Initializes the event args instance with an empty list of files.
        /// </summary>
        public FileRequestedEventArgs()
        {
            this.providedFiles = new List<ITestableFile>();
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
