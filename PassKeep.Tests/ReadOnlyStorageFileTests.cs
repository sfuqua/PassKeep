using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.Models;
using PassKeep.Tests.Attributes;
using PassKeep.Tests.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
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
                new StorageFileDatabaseCandidate(databaseInfo.Database)
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
                new StorageFileDatabaseCandidate(databaseInfo.Database)
            );

            Assert.IsTrue(service.CanSave, "Should be able to save a not read-only file");
        }

        [TestMethod, DatabaseInfo("ReadOnly_Password")]
        public async Task DbUnlockViewModelReadOnly()
        {
            Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
            Assert.IsTrue(databaseInfo.Database.Attributes.HasFlag(FileAttributes.ReadOnly), "Database file should be read-only");

            IDatabaseUnlockViewModel viewModel = new DatabaseUnlockViewModel(
                new StorageFileDatabaseCandidate(databaseInfo.Database),
                false,
                new MockStorageItemAccessList(),
                new KdbxReader()
            );

            Assert.IsTrue(viewModel.IsReadOnly, "DatabaseUnlockViewModel should be read-only for a read-only file");

            bool propChangedFired = false;
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.IsReadOnly))
                {
                    propChangedFired = true;
                }
            };
            viewModel.CandidateFile = new StorageFileDatabaseCandidate(await Utils.GetDatabaseByName("StructureTesting.kdbx"));
            Assert.IsFalse(viewModel.IsReadOnly, "DatabaseUnlockViewModel should not be read-only for a writable file");
            Assert.IsTrue(propChangedFired, $"PropertyChanged should have fired with name {nameof(viewModel.IsReadOnly)}");
        }
    }
}
