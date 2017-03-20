using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Kdf;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Allows the user to configure settings for a specific database.
    /// </summary>
    public interface IDatabaseSettingsProvider
    {
        /// <summary>
        /// Configures the cipher used to encrypt the database.
        /// </summary>
        EncryptionAlgorithm Cipher { get; set; }

        /// <summary>
        /// Configures the parameters used to transform the key.
        /// </summary>
        KdfParameters KdfParameters { get; set; }
    }
}
