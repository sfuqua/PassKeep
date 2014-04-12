using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.KeePassTests
{
    [TestClass]
    public class DatabaseViewModelTests : TestClassBase
    {
        private const string StructureTestingDatabase = "StructureTesting.kdbx";
        private const string UnsearchableRootDatabase = "Unsearchable.kdbx";

        private IDatabaseViewModel viewModel;

        [TestInitialize]
        public async Task Initialize()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
                KdbxReader reader = new KdbxReader();

                using(IRandomAccessStream stream = await databaseInfo.Database.OpenReadAsync())
                {
                    Assert.IsFalse((await reader.ReadHeader(stream, cts.Token)).IsError);
                    KdbxDecryptionResult decryption = await reader.DecryptFile(stream, databaseInfo.Password, databaseInfo.Keyfile, cts.Token);

                    Assert.IsFalse(decryption.Result.IsError);
                    this.viewModel = new DatabaseViewModel(
                        decryption.GetDocument(),
                        new DatabaseNavigationViewModel(),
                        new AppSettingsService(new InMemorySettingsProvider()),
                        new DummyPersistenceService()
                    );
                }
            }
            catch(InvalidOperationException) { }
        }

        [TestMethod, DatabaseInfo(UnsearchableRootDatabase)]
        public void DatabaseViewModel_GetSearchableNodes_Unsearchable()
        {
            ICollection<IKeePassNode> searchable = this.viewModel.GetAllSearchableNodes();
            Assert.IsNotNull(searchable, "GetAllSearchableNodes should not return null");

            IEnumerable<IKeePassNode> entries = searchable.Where(node => node is IKeePassEntry);
            IEnumerable<IKeePassNode> groups = searchable.Where(node => node is IKeePassGroup);

            Assert.AreEqual(0, entries.Count(), "Number of searchable entries should be 0");
            Assert.AreEqual(7, groups.Count(), "Number of searchable groups should be 7");
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseViewModel_GetSearchableNodes_Depth()
        {
            // This database has a special search structure.
            // We expect ALL (10) groups to show up in the search.
            // We expect only 4 entries to show up in the search.

            ICollection<IKeePassNode> searchable = this.viewModel.GetAllSearchableNodes();
            Assert.IsNotNull(searchable, "GetAllSearchableNodes should not return null");

            IEnumerable<IKeePassNode> entries = searchable.Where(node => node is IKeePassEntry);
            IEnumerable<IKeePassNode> groups = searchable.Where(node => node is IKeePassGroup);

            Assert.AreEqual(4, entries.Count(), "Number of searchable entries should be 4");
            Assert.AreEqual(10, groups.Count(), "Number of searchable groups should be 10");
        }
    }
}
