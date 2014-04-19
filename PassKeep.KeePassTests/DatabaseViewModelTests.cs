using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.KeePassTests
{
    [TestClass]
    public class DatabaseViewModelTests : DatabasePersistenceViewModelTests<IDatabaseViewModel>
    {
        private const string StructureTestingDatabase = "StructureTesting.kdbx";
        private const string UnsearchableRootDatabase = "Unsearchable.kdbx";

        [TestInitialize]
        public async Task Initialize()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
                KdbxReader reader = new KdbxReader();

                using (IRandomAccessStream stream = await databaseInfo.Database.OpenReadAsync())
                {
                    Assert.IsFalse((await reader.ReadHeader(stream, cts.Token)).IsError);
                    KdbxDecryptionResult decryption = await reader.DecryptFile(stream, databaseInfo.Password, databaseInfo.Keyfile, cts.Token);

                    Assert.IsFalse(decryption.Result.IsError);
                    this.viewModel = new DatabaseViewModel(
                        decryption.GetDocument(),
                        new DatabaseNavigationViewModel(),
                        new DummyPersistenceService(),
                        new AppSettingsService(new InMemorySettingsProvider())
                    );;
                }
            }
            catch (InvalidOperationException) { }
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(1000)]
        public async Task DatabaseViewModel_DoSave()
        {
            await ValidateSave();
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(1000)]
        public async Task DatabaseViewModel_DoCancelledSave()
        {
            await ValidateCancelledSave();
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
            // This document has a special search structure.
            // We expect ALL (15) groups to show up in the search.
            // We expect only 8 entries to show up in the search.

            ICollection<IKeePassNode> searchable = this.viewModel.GetAllSearchableNodes();
            Assert.IsNotNull(searchable, "GetAllSearchableNodes should not return null");

            IEnumerable<IKeePassNode> entries = searchable.Where(node => node is IKeePassEntry);
            IEnumerable<IKeePassNode> groups = searchable.Where(node => node is IKeePassGroup);

            Assert.AreEqual(8, entries.Count(), "Number of searchable entries should be 8");
            Assert.AreEqual(15, groups.Count(), "Number of searchable groups should be 15");
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseViewModel_VerifyDefaultSortMode()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.Groups[2]
            );

            VerifySortGroupChildren(DatabaseSortMode.Mode.DatabaseOrder);
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseViewModel_VerifyAllSortModes()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.Groups[2]
            );

            foreach(DatabaseSortMode mode in this.viewModel.AvailableSortModes)
            {
                this.viewModel.SortMode = mode;

                Debug.WriteLine("Verifying SortMode: {0}", mode);
                VerifySortGroupChildren(mode.SortMode);
            }
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(1000)]
        public async Task DatabaseViewModel_DeleteGroup()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.Groups[2]
            );

            this.viewModel.SortMode = this.viewModel.AvailableSortModes.First(
                m => m.SortMode == DatabaseSortMode.Mode.AlphabetAscending
            );

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            ((INotifyCollectionChanged)this.viewModel.SortedGroups).CollectionChanged += (s, e) =>
            {
                tcs.SetResult(null);
            };

            this.viewModel.DeleteGroupAndSave(
                this.viewModel.NavigationViewModel.ActiveGroup.Groups.First(g => g.Title.ClearValue == "Delta")
            );

            await tcs.Task;

            VerifyListsMatch(
                new List<string> { "Alpha", "Beta", "Gamma" },
                this.viewModel.SortedGroups
            );
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(1000)]
        public async Task DatabaseViewModel_DeleteEntry()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.Groups[2]
            );

            this.viewModel.SortMode = this.viewModel.AvailableSortModes.First(
                m => m.SortMode == DatabaseSortMode.Mode.AlphabetDescending
            );

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            ((INotifyCollectionChanged)this.viewModel.SortedEntries).CollectionChanged += (s, e) =>
            {
                tcs.SetResult(null);
            };

            this.viewModel.DeleteEntryAndSave(
                this.viewModel.NavigationViewModel.ActiveGroup.Entries.First(g => g.Title.ClearValue == "Beta")
            );

            await tcs.Task;

            VerifyListsMatch(
                new List<string> { "Gamma", "Delta", "Alpha" },
                this.viewModel.SortedEntries
            );
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public async Task DatabaseViewModel_CancelDeleteGroup()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.Groups[2]
            );

            this.viewModel.SortMode = this.viewModel.AvailableSortModes.First(
                m => m.SortMode == DatabaseSortMode.Mode.AlphabetAscending
            );

            this.viewModel.StartedSave += (s, e) =>
            {
                e.Cts.Cancel();
            };

            bool eventFired = false;
            ((INotifyCollectionChanged)this.viewModel.SortedGroups).CollectionChanged += (s, e) =>
            {
                eventFired = true;
            };

            this.viewModel.DeleteGroupAndSave(
                this.viewModel.NavigationViewModel.ActiveGroup.Groups.First(g => g.Title.ClearValue == "Delta")
            );

            await AwaitableTimeout(1000);

            Assert.IsFalse(eventFired, "CollectionChanged should not have fired!");
            VerifyListsMatch(
                new List<string> { "Alpha", "Beta", "Delta", "Gamma" },
                this.viewModel.SortedGroups
            );
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public async Task DatabaseViewModel_CancelDeleteEntry()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.Groups[2]
            );

            this.viewModel.SortMode = this.viewModel.AvailableSortModes.First(
                m => m.SortMode == DatabaseSortMode.Mode.AlphabetDescending
            );

            this.viewModel.StartedSave += (s, e) =>
            {
                e.Cts.Cancel();
            };

            bool eventFired = false;
            ((INotifyCollectionChanged)this.viewModel.SortedEntries).CollectionChanged += (s, e) =>
            {
                eventFired = true;
            };

            this.viewModel.DeleteEntryAndSave(
                this.viewModel.NavigationViewModel.ActiveGroup.Entries.First(g => g.Title.ClearValue == "Delta")
            );

            await AwaitableTimeout(1000);

            Assert.IsFalse(eventFired, "CollectionChanged should not have fired!");
            VerifyListsMatch(
                new List<string> { "Gamma", "Delta", "Beta", "Alpha" },
                this.viewModel.SortedEntries
            );
        }

        /// <summary>
        /// Validates sorting for the special "Sort Testing" group.
        /// </summary>
        private void VerifySortGroupChildren(DatabaseSortMode.Mode mode)
        {
            List<string> groupNames;
            List<string> entryNames;

            switch(mode)
            {
                case DatabaseSortMode.Mode.AlphabetAscending:
                    groupNames = new List<string> { "Alpha", "Beta", "Delta", "Gamma" };
                    entryNames = groupNames;
                    break;
                case DatabaseSortMode.Mode.AlphabetDescending:
                    groupNames = new List<string> { "Gamma", "Delta", "Beta", "Alpha" };
                    entryNames = groupNames;
                    break;
                case DatabaseSortMode.Mode.DatabaseOrder:
                    groupNames = new List<string> { "Beta", "Alpha", "Gamma", "Delta" };
                    entryNames = new List<string> { "Delta", "Gamma", "Alpha", "Beta" };
                    break;
                default:
                    Assert.Fail("Unknown sort mode.");
                    return;
            }

            VerifyListsMatch(groupNames, this.viewModel.SortedGroups);
            VerifyListsMatch(entryNames, this.viewModel.SortedEntries);
        }

        private void VerifyListsMatch(IReadOnlyList<string> expectedTitles, IReadOnlyList<IKeePassNode> nodes)
        {
            Assert.AreEqual(expectedTitles.Count, nodes.Count);

            for (int i = 0; i < expectedTitles.Count; i++)
            {
                Debug.WriteLine("Verifying index {0}", i);
                Debug.WriteLine("Expected title: {0}", expectedTitles[i]);
                Debug.WriteLine("Actual title: {0}", nodes[i].Title);
                Assert.AreEqual(expectedTitles[i], nodes[i].Title.ClearValue);
            }
        }
    }
}
