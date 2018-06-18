using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.ViewModels;
using PassKeep.Tests.Mocks;
using SariphLib.Mvvm;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace PassKeep.Tests
{
    [TestClass]
    public sealed class SavedCredentialsViewModelTests : TestClassBase
    {
        // Data stored for each credential - placeholder/dummy
        private readonly IBuffer storedKey = CryptographicBuffer.GenerateRandom(32);

        // The actual credentials to store in the provider
        private readonly IDatabaseCandidate[] storedData = new IDatabaseCandidate[]
        {
            new MockDatabaseCandidate { FileName = "A" },
            new MockDatabaseCandidate { FileName = "B" },
            new MockDatabaseCandidate { FileName = "D" },
            new MockDatabaseCandidate { FileName = "C" }
        };

        private ICredentialStorageProvider credentialsProvider;
        private ISavedCredentialsViewModel viewModel;

        public override TestContext TestContext
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes the credential provider and ViewModel.
        /// </summary>
        /// <returns></returns>
        [TestInitialize]
        public async Task Init()
        {
            this.credentialsProvider = new MockCredentialProvider();
            foreach (IDatabaseCandidate candidate in this.storedData)
            {
                await this.credentialsProvider.TryStoreRawKeyAsync(candidate.File, this.storedKey);
            }

            this.viewModel = new SavedCredentialsViewModel(this.credentialsProvider);
            await this.viewModel.ActivateAsync();
        }

        /// <summary>
        /// Validates that the ViewModel and credential provider are in agreement
        /// about the initial set of stored credentials.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ValidateInitialCollection()
        {
            // Normalize the lists by sorting.
            // This is because the provider does not make any guarantees about ordering.
            IList<string> storedCredentials = (await this.credentialsProvider.GetAllEntriesAsync())
                .OrderBy(cred => cred).ToList();

            IList<string> vmCredentials = this.viewModel.CredentialTokens
                .OrderBy(cred => cred).ToList();

            IList<string> expectedCredentials = this.storedData
                .Select(db => db.FileName).OrderBy(cred => cred).ToList();

            // Validate credential list lengths.
            Assert.AreEqual(expectedCredentials.Count, storedCredentials.Count);
            Assert.AreEqual(expectedCredentials.Count, vmCredentials.Count);

            // Validate credential list content.
            for (int i = 0; i < expectedCredentials.Count; i++)
            {
                Assert.AreEqual(expectedCredentials[i], storedCredentials[i]);
                Assert.AreEqual(expectedCredentials[i], vmCredentials[i]);
            }
        }

        /// <summary>
        /// Validates that ViewModel.DeleteAllAsyncCommand clears
        /// the observable collection, both in content and by notification event.
        /// </summary>
        /// <returns></returns>
        [TestMethod, Timeout(5000)]
        public async Task DeleteAllCommand()
        {
            Task evtFired = this.viewModel.CredentialTokens.WaitForChangeAsync(NotifyCollectionChangedAction.Reset);

            await this.viewModel.DeleteAllAsyncCommand.ExecuteAsync(null);
            await evtFired;

            Assert.AreEqual(0, this.viewModel.CredentialTokens.Count, "Credential collection should be empty on clear");
            Assert.AreEqual(0, (await this.credentialsProvider.GetAllEntriesAsync()).Count, "Provider collection should be empty on clear");
        }

        /// <summary>
        /// Validates that the ViewModel.DeleteCredentialAsyncCommand deletes
        /// an element, both in content and by notification event.
        /// </summary>
        /// <returns></returns>
        [TestMethod, Timeout(5000)]
        public async Task DeleteOneCommand()
        {
            Task evtFired = this.viewModel.CredentialTokens.WaitForChangeAsync(NotifyCollectionChangedAction.Remove);

            string toDelete = this.storedData[1].FileName;
            Assert.IsTrue(this.viewModel.CredentialTokens.Contains(toDelete));

            await this.viewModel.DeleteCredentialAsyncCommand.ExecuteAsync(toDelete);
            await evtFired;

            Assert.IsFalse(this.viewModel.CredentialTokens.Contains(toDelete));
            Assert.AreEqual(this.storedData.Length - 1, this.viewModel.CredentialTokens.Count);

            IReadOnlyCollection<string> newProviderCreds = await this.credentialsProvider.GetAllEntriesAsync();
            Assert.IsFalse(newProviderCreds.Contains(toDelete));
            Assert.AreEqual(this.storedData.Length - 1, newProviderCreds.Count);
        }
    }
}
