using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
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
            Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
            Assert.IsTrue(databaseInfo.Database.Attributes.HasFlag(FileAttributes.ReadOnly), "Database file should be read-only");
            
            StorageFileDatabaseCandidateFactory factory = new StorageFileDatabaseCandidateFactory();

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            IDatabaseUnlockViewModel viewModel = new DatabaseUnlockViewModel(
                await factory.AssembleAsync(databaseInfo.Database),
                false,
                new MockStorageItemAccessList(),
                new KdbxReader(),
                new TaskNotificationService(),
                new MockIdentityVerifier(),
                new MockCredentialProvider(),
                new MockCredentialStorageViewModelFactory()
            );

            await viewModel.ActivateAsync();
            Assert.IsTrue(viewModel.IsReadOnly, "DatabaseUnlockViewModel should be read-only for a read-only file");
            
            await viewModel.UpdateCandidateFileAsync(await factory.AssembleAsync(await Utils.GetDatabaseByName("StructureTesting.kdbx")));
            Assert.IsFalse(viewModel.IsReadOnly, "DatabaseUnlockViewModel should not be read-only for a writable file");
        }
    }
}
