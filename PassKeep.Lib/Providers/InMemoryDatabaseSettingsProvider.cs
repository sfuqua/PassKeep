// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.KeePass.Kdf;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// Stores database settings temporarily in memory before associating them with a file.
    /// </summary>
    public class InMemoryDatabaseSettingsProvider : IDatabaseSettingsProvider
    {
        /// <summary>
        /// The cipher used to encrypt the database.
        /// </summary>
        public EncryptionAlgorithm Cipher { get; set; }

        /// <summary>
        /// Parameters used for deriving the database's master key.
        /// </summary>
        public KdfParameters KdfParameters { get; set; }
    }
}
