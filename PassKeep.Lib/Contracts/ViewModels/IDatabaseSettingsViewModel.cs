using PassKeep.Lib.Contracts.KeePass;
using System;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Exposes KDF and cipher settings to the view.
    /// </summary>
    public interface IDatabaseSettingsViewModel : IViewModel
    {
        /// <summary>
        /// Algorithm to use for encrypting the database.
        /// </summary>
        EncryptionAlgorithm Cipher
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier for the key derivation function to use.
        /// </summary>
        Guid KdfGuid
        {
            get;
            set;
        }

        /// <summary>
        /// Number of transform rounds to use for the KDF.
        /// </summary>
        int KdfIterations
        {
            get;
            set;
        }

        /// <summary>
        /// If Argon2 is the <see cref="Cipher"/>, configures the degree
        /// of parallelism.
        /// </summary>
        int ArgonParallelism
        {
            get;
            set;
        }

        /// <summary>
        /// If Argon2 is the <see cref="Cipher"/>, configures the amount of
        /// memory used.
        /// </summary>
        int ArgonBlockCount
        {
            get;
            set;
        }
    }
}
