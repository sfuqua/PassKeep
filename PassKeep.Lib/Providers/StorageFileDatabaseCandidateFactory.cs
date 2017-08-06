// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Models;
using SariphLib.Files;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// A factory for assembling <see cref="StorageFileDatabaseCandidate"/> instances.
    /// </summary>
    public class StorageFileDatabaseCandidateFactory : IDatabaseCandidateFactory
    {
        private readonly IFileProxyProvider proxyProvider;

        /// <summary>
        /// Initializes the factory.
        /// </summary>
        /// <param name="proxyProvider">Provider to use for generating storage file proxies.</param>
        public StorageFileDatabaseCandidateFactory(IFileProxyProvider proxyProvider)
        {
            if (proxyProvider == null)
            {
                throw new ArgumentNullException(nameof(proxyProvider));
            }

            this.proxyProvider = proxyProvider;
        }

        /// <summary>
        /// Assembles a <see cref="StorageFileDatabaseCandidate"/> that wraps
        /// <paramref name="file"/> and initializes the cached file for reading.
        /// </summary>
        /// <param name="file">The file to wrap.</param>
        /// <returns>An initialized <see cref="StorageFileDatabaseCandidate"/>.</returns>
        public async Task<IDatabaseCandidate> AssembleAsync(ITestableFile file)
        {
            bool isAppOwned = await this.proxyProvider.PathIsInScopeAsync(file.AsIStorageItem2).ConfigureAwait(false);
            StorageFileDatabaseCandidate candidate = new StorageFileDatabaseCandidate(file, isAppOwned);
            await candidate.GenerateReadOnlyCachedCopyAsync();

            return candidate;
        }
    }
}
