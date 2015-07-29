using Windows.Storage.AccessCache;

namespace PassKeep.Models
{
    /// <summary>
    /// Class that proxies AccessListEntry to allow databinding.
    /// </summary>
    public class StoredFileDescriptor
    {
        private AccessListEntry accessListEntry;

        /// <summary>
        /// Initializes the model from the provided struct.
        /// </summary>
        /// <param name="accessListEntry">The AccessListEntry to use as a reference.</param>
        public StoredFileDescriptor(AccessListEntry accessListEntry)
        {
            this.accessListEntry = accessListEntry;
        }

        /// <summary>
        /// Returns an access token for the stored file.
        /// </summary>
        public string Token
        {
            get
            {
                return this.accessListEntry.Token;
            }
        }

        /// <summary>
        /// Returns metadata about the stored file.
        /// </summary>
        public string Metadata
        {
            get
            {
                return this.accessListEntry.Metadata;
            }
        }
    }
}
