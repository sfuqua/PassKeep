// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;

namespace PassKeep.Lib.Providers
{
    public class DatabaseCredentialProviderFactory : IDatabaseCredentialProviderFactory
    {
        private readonly ICredentialStorageProvider credentialStorage;

        public DatabaseCredentialProviderFactory(ICredentialStorageProvider credentialStorage)
        {
            this.credentialStorage = credentialStorage;
        }

        public IDatabaseCredentialProvider Assemble(IDatabasePersistenceService persistenceService)
        {
            return new DatabaseCredentialProvider(persistenceService, this.credentialStorage);
        }
    }
}
