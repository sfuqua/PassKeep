// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Diagnostics;
using SariphLib.Files;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// Handles updating credentials for a database.
    /// </summary>
    public class DatabaseCredentialProvider : IDatabaseCredentialProvider
    {
        private readonly IDatabasePersistenceService persistenceService;
        private readonly ICredentialStorageProvider credentialStorage;
        private IEventLogger logger;

        /// <summary>
        /// Initializes the provider with the provided dependencies.
        /// </summary>
        /// <param name="persistenceService">Used to save the database after updating credentials.</param>
        /// <param name="credentialStorage">Used to manage previously saved credentials.</param>
        public DatabaseCredentialProvider(IDatabasePersistenceService persistenceService, ICredentialStorageProvider credentialStorage)
        {
            this.persistenceService = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));
            this.credentialStorage = credentialStorage ?? throw new ArgumentNullException(nameof(credentialStorage));
        }

        /// <summary>
        /// Allows for diagnostic logging.
        /// </summary>
        public IEventLogger Logger
        {
            get => this.logger ?? NullLogger.Instance;
            set { this.logger = value; }
        }

        /// <summary>
        /// Updates the credentials used to encrypt the provided database.
        /// </summary>
        /// <param name="document">The document being encrypted.</param>
        /// <param name="databaseToken">Token used to lookup the database in credential storage.</param>
        /// <param name="tokens">Security tokens used to encrypt the database.</param>
        /// <returns>A Task that completes once the update is finished.</returns>
        public async Task UpdateCredentialsAsync(KdbxDocument document, ITestableFile databaseFile, IEnumerable<ISecurityToken> tokens)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (databaseFile == null)
            {
                throw new ArgumentNullException(nameof(databaseFile));
            }

            // Fails silently, always safe to call
            await this.credentialStorage.DeleteAsync(databaseFile).ConfigureAwait(false);

            await this.persistenceService.SettingsProvider.UpdateSecurityTokensAsync(tokens).ConfigureAwait(false);
            document.Metadata.NotifyMasterKeyUpdated();

            await this.persistenceService.Save(document).ConfigureAwait(false);
        }
    }
}
