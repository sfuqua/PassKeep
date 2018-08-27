using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.Tests.Attributes;
using PassKeep.Tests.Mocks;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Tests
{
    [TestClass]
    public class ReadOnlyStorageFileTests : TestClassBase
    {
        public override TestContext TestContext
        {
            get; set;
        }

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod, Timeout(5000), DatabaseInfo("ReadOnly_Password")]
        public async Task DbUnlockViewModelReadOnly()
        {
            Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(TestContext);
            Assert.IsTrue(databaseInfo.Database.AsIStorageItem.Attributes.HasFlag(FileAttributes.ReadOnly), "Database file should be read-only");

            IFileProxyProvider proxyProvider = new MockFileProxyProvider();
            StorageFileDatabaseCandidateFactory factory = new StorageFileDatabaseCandidateFactory(proxyProvider);

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            ICredentialStorageProvider credentialStorage = new MockCredentialProvider();
            IDatabaseUnlockViewModel viewModel = new DatabaseUnlockViewModel(
                new MockSyncContext(),
                await factory.AssembleAsync(databaseInfo.Database),
                false,
                new MockStorageItemAccessList(),
                new KdbxReader(),
                proxyProvider,
                factory,
                new MasterKeyChangeViewModelFactory(new DatabaseCredentialProviderFactory(credentialStorage), new MockFileService()),
                new TaskNotificationService(),
                new MockIdentityVerifier(),
                credentialStorage,
                new MockCredentialStorageViewModelFactory()
            );

            await viewModel.ActivateAsync();
            Assert.IsTrue(viewModel.IsReadOnly, "DatabaseUnlockViewModel should be read-only for a read-only file");
            
            await viewModel.UpdateCandidateFileAsync(await factory.AssembleAsync(await Utils.GetDatabaseByName("StructureTesting.kdbx")));
            Assert.IsFalse(viewModel.IsReadOnly, "DatabaseUnlockViewModel should not be read-only for a writable file");
        }
    }
}
