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
