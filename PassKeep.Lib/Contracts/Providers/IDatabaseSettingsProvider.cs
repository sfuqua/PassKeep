﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Kdf;
using System.Collections.Generic;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Allows the user to configure settings for a specific database.
    /// </summary>
    public interface IDatabaseSettingsProvider
    {
        /// <summary>
        /// Updates the security tokens that will be used to persist this databae.
        /// </summary>
        /// <param name="tokens">The tokens to use.</param>
        void UpdateSecurityTokens(IEnumerable<ISecurityToken> tokens);

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
