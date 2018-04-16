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
    public class MasterKeyChangeViewModel : MasterKeyViewModel
    {
        private readonly KdbxDocument document;
        private readonly ITestableFile databaseFile;
        private readonly IDatabaseCredentialProvider credentialProvider;

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
