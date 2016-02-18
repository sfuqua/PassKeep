using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.Models;
using PassKeep.Tests.Attributes;
using PassKeep.Tests.Mocks;
using SariphLib.Files;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

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

        [TestMethod, DatabaseInfo("ReadOnly_Password")]
        public async Task PersistenceServiceCannotSaveReadOnly()
        {
            Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);

            Assert.IsTrue(databaseInfo.Database.Attributes.HasFlag(FileAttributes.ReadOnly), "Database file should be read-only");
            IKdbxReader reader = new KdbxReader();
            using (IRandomAccessStream stream = await databaseInfo.Database.OpenReadAsync())
            {
                await reader.ReadHeader(stream, CancellationToken.None);
                await reader.DecryptFile(stream, databaseInfo.Password, null, CancellationToken.None);
            }

            IDatabasePersistenceService service = new DefaultFilePersistenceService(
                reader.GetWriter(),
                new StorageFileDatabaseCandidate(databaseInfo.Database),
                await databaseInfo.Database.CheckWritableAsync()
            );

            Assert.IsFalse(service.CanSave, "Should not be able to save a read-only file");
        }

        [TestMethod, DatabaseInfo("StructureTesting")]
        public async Task PersistenceServiceCanSaveNotReadOnly()
        {
            Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);

            Assert.IsFalse(databaseInfo.Database.Attributes.HasFlag(FileAttributes.ReadOnly), "Database file should not be read-only");
            IKdbxReader reader = new KdbxReader();
            using (IRandomAccessStream stream = await databaseInfo.Database.OpenReadAsync())
            {
                await reader.ReadHeader(stream, CancellationToken.None);
                await reader.DecryptFile(stream, databaseInfo.Password, null, CancellationToken.None);
            }

            IDatabasePersistenceService service = new DefaultFilePersistenceService(
                reader.GetWriter(),
                new StorageFileDatabaseCandidate(databaseInfo.Database),
                await databaseInfo.Database.CheckWritableAsync()
            );

            Assert.IsTrue(service.CanSave, "Should be able to save a not read-only file");
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
                new MockSyncContext()
            );

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.IsReadOnly))
                {
                    tcs.SetResult(true);
                }
            };

            await tcs.Task;
            Assert.IsTrue(viewModel.IsReadOnly, "DatabaseUnlockViewModel should be read-only for a read-only file");

            tcs = new TaskCompletionSource<bool>();
            viewModel.CandidateFile = await factory.AssembleAsync(await Utils.GetDatabaseByName("StructureTesting.kdbx"));
            await tcs.Task;
            Assert.IsFalse(viewModel.IsReadOnly, "DatabaseUnlockViewModel should not be read-only for a writable file");
        }
    }
}
