// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Kdf;
using SariphLib.Files;
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
        ulong KdfIterations
        {
            get;
            set;
        }

        /// <summary>
        /// If Argon2 is the <see cref="Cipher"/>, configures the degree
        /// of parallelism.
        /// </summary>
        uint ArgonParallelism
        {
            get;
            set;
        }

        /// <summary>
        /// If Argon2 is the <see cref="Cipher"/>, configures the amount of
        /// memory used.
        /// </summary>
        ulong ArgonBlockCount
        {
            get;
            set;
        }

        /// <summary>
        /// Retrieves the current <see cref="KdfParameters"/> represented by these settings.
        /// </summary>
        /// <returns></returns>
        KdfParameters GetKdfParameters();
    }
}
