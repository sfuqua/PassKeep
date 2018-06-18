// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.SecurityTokens;
using SariphLib.Files;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel used to handle updating the master key to an existing database.
    /// </summary>
    public class MasterKeyChangeViewModel : MasterKeyViewModel
    {
        private readonly KdbxDocument document;
        private readonly ITestableFile databaseFile;
        private readonly IDatabaseCredentialProvider credentialProvider;

        /// <summary>
        /// Initializes dependencies.
        /// </summary>
        /// <param name="document">The document whose master key may be updated.</param>
        /// <param name="databaseFile">The underlying file to persist changes to.</param>
        /// <param name="credentialProvider">A provider that manages changes to credentials for the database.</param>
        /// <param name="fileService">The service used to pick a new keyfile.</param>
        public MasterKeyChangeViewModel(
            KdbxDocument document,
            ITestableFile databaseFile,
            IDatabaseCredentialProvider credentialProvider,
            IFileAccessService fileService
        ) : base(fileService)
        {
            this.document = document ?? throw new ArgumentNullException(nameof(document));
            this.databaseFile = databaseFile;
            this.credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
        }

        protected override async Task HandleCredentialsAsync(string confirmedPassword, ITestableFile chosenKeyFile)
        {
            LogCurrentFunction();

            IList<ISecurityToken> tokens = new List<ISecurityToken>();
            if (!String.IsNullOrEmpty(confirmedPassword))
            {
                tokens.Add(new MasterPassword(confirmedPassword));
            }

            if (chosenKeyFile != null)
            {
                tokens.Add(new KeyFile(chosenKeyFile));
            }

            await this.credentialProvider.UpdateCredentialsAsync(this.document, this.databaseFile, tokens).ConfigureAwait(false);
            LogEventWithContext("UpdateComplete");
        }
    }
}
