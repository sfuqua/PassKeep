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
