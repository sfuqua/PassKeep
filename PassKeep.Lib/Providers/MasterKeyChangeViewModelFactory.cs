using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.ViewModels;
using SariphLib.Files;
using System;

namespace PassKeep.Lib.Providers
{
    public class MasterKeyChangeViewModelFactory : IMasterKeyChangeViewModelFactory
    {
        private readonly IDatabaseCredentialProvider credentialProvider;
        private readonly IFileAccessService fileAccessService;

        public MasterKeyChangeViewModelFactory(IDatabaseCredentialProvider credentialProvider, IFileAccessService fileAccessService)
        {
            this.credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            this.fileAccessService = fileAccessService ?? throw new ArgumentNullException(nameof(fileAccessService));
        }

        public IMasterKeyViewModel Assemble(KdbxDocument document, ITestableFile databaseFile)
        {
            return new MasterKeyChangeViewModel(document, databaseFile, this.credentialProvider, this.fileAccessService);
        }
    }
}
