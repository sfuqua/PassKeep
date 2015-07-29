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
        public DatabaseCandidateMessage(IStorageFile file, bool isSample)
        {
            this.File = file;
            this.IsSample = isSample;
        }

        public IStorageFile File
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
