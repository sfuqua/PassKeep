// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
        private readonly IDatabaseCredentialProviderFactory credentialProviderFactory;
        private readonly IFileAccessService fileAccessService;

        public MasterKeyChangeViewModelFactory(IDatabaseCredentialProviderFactory credentialProviderFactory, IFileAccessService fileAccessService)
        {
            this.credentialProviderFactory = credentialProviderFactory ?? throw new ArgumentNullException(nameof(credentialProviderFactory));
            this.fileAccessService = fileAccessService ?? throw new ArgumentNullException(nameof(fileAccessService));
        }

        public IMasterKeyViewModel Assemble(KdbxDocument document, IDatabasePersistenceService persistenceService, ITestableFile databaseFile)
        {
            IDatabaseCredentialProvider provider = this.credentialProviderFactory.Assemble(persistenceService);
            return new MasterKeyChangeViewModel(document, databaseFile, provider, this.fileAccessService);
        }
    }
}
