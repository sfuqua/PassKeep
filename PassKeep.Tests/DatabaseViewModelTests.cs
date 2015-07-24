using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Tests.Attributes;
using PassKeep.Lib.Contracts.Enums;
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
using Windows.ApplicationModel.Resources;
using Windows.Storage.Streams;
using PassKeep.Lib.Contracts.Services;
using Windows.ApplicationModel.DataTransfer;
using PassKeep.Lib.Contracts.Providers;

namespace PassKeep.Tests
{
    [TestClass]
    public class DatabaseViewModelTests : DatabasePersistenceViewModelTests<IDatabaseViewModel>
    {
        private const string StructureTestingDatabase = "StructureTesting.kdbx";
        private const string UnsearchableRootDatabase = "Unsearchable.kdbx";

        private IClipboardProvider clipboard = new InMemoryClipboardProvider();
        private ISensitiveClipboardService clipboardService;

        [TestInitialize]
        public async Task Initialize()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            this.clipboardService = new SensitiveClipboardService(this.clipboard);

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
                        ResourceLoader.GetForViewIndependentUse(),
                        reader.HeaderData.GenerateRng(),
                        new DatabaseNavigationViewModel(),
                        new DummyPersistenceService(),
                        new AppSettingsService(new InMemorySettingsProvider()),
                        this.clipboardService
                    );

                    this.viewModel.Activate();
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
            ICollection<IKeePassNode> searchable = this.viewModel.GetAllSearchableNodes(String.Empty);
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

            ICollection<IKeePassNode> searchable = this.viewModel.GetAllSearchableNodes(String.Empty);
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
                this.viewModel.Document.Root.DatabaseGroup.GetChildGroup(2)
            );

            VerifySortGroupChildren(DatabaseSortMode.Mode.DatabaseOrder);
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseViewModel_VerifyAllSortModes()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.GetChildGroup(2)
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
                this.viewModel.Document.Root.DatabaseGroup.GetChildGroup(2)
            );

            this.viewModel.SortMode = this.viewModel.AvailableSortModes.First(
                m => m.SortMode == DatabaseSortMode.Mode.AlphabetAscending
            );

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            ((INotifyCollectionChanged)this.viewModel.SortedChildren).CollectionChanged += (s, e) =>
            {
                tcs.SetResult(null);
            };

            this.viewModel.DeleteNodeAndSave(
                this.viewModel.NavigationViewModel.ActiveGroup.Children.First(
                    g => g.Title.ClearValue == "Delta" && g is IKeePassGroup
                )
                as IKeePassGroup
            );

            await tcs.Task;

            VerifyListsMatch(
                new List<string> { "Alpha", "Beta", "Gamma", "Alpha", "Beta", "Delta", "Gamma" },
                this.viewModel.SortedChildren.Select(node => node.Node).ToList()
            );
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(1000)]
        public async Task DatabaseViewModel_DeleteEntry()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.GetChildGroup(2)
            );

            this.viewModel.SortMode = this.viewModel.AvailableSortModes.First(
                m => m.SortMode == DatabaseSortMode.Mode.AlphabetDescending
            );

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            ((INotifyCollectionChanged)this.viewModel.SortedChildren).CollectionChanged += (s, e) =>
            {
                tcs.SetResult(null);
            };

            this.viewModel.DeleteNodeAndSave(
                this.viewModel.NavigationViewModel.ActiveGroup.Children.First(g => g.Title.ClearValue == "Beta")
                as IKeePassEntry
            );

            await tcs.Task;

            VerifyListsMatch(
                new List<string> { "Gamma", "Delta", "Beta", "Alpha", "Gamma", "Delta", "Alpha" },
                this.viewModel.SortedChildren.Select(node => node.Node).ToList()
            );
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public async Task DatabaseViewModel_CancelDeleteGroup()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.GetChildGroup(2)
            );

            this.viewModel.SortMode = this.viewModel.AvailableSortModes.First(
                m => m.SortMode == DatabaseSortMode.Mode.AlphabetAscending
            );

            this.viewModel.StartedSave += (s, e) =>
            {
                e.Cts.Cancel();
            };

            bool eventFired = false;
            ((INotifyCollectionChanged)this.viewModel.SortedChildren).CollectionChanged += (s, e) =>
            {
                eventFired = true;
            };

            this.viewModel.DeleteNodeAndSave(
                this.viewModel.NavigationViewModel.ActiveGroup.Children.First(
                    g => g.Title.ClearValue == "Delta" && g is IKeePassGroup
                )
                as IKeePassGroup
            );

            await AwaitableTimeout(1000);

            Assert.IsFalse(eventFired, "CollectionChanged should not have fired!");
            VerifyListsMatch(
                new List<string> { "Alpha", "Beta", "Delta", "Gamma", "Alpha", "Beta", "Delta", "Gamma" },
                this.viewModel.SortedChildren.Select(node => node.Node).ToList()
            );
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public async Task DatabaseViewModel_CancelDeleteEntry()
        {
            this.viewModel.NavigationViewModel.SetGroup(
                this.viewModel.Document.Root.DatabaseGroup.GetChildGroup(2)
            );

            this.viewModel.SortMode = this.viewModel.AvailableSortModes.First(
                m => m.SortMode == DatabaseSortMode.Mode.AlphabetDescending
            );

            this.viewModel.StartedSave += (s, e) =>
            {
                e.Cts.Cancel();
            };

            bool eventFired = false;
            ((INotifyCollectionChanged)this.viewModel.SortedChildren).CollectionChanged += (s, e) =>
            {
                eventFired = true;
            };

            this.viewModel.DeleteNodeAndSave(
                this.viewModel.NavigationViewModel.ActiveGroup.Children.First(
                    g => g.Title.ClearValue == "Delta" && g is IKeePassEntry
                )
                as IKeePassEntry
            );

            await AwaitableTimeout(1000);

            Assert.IsFalse(eventFired, "CollectionChanged should not have fired!");
            VerifyListsMatch(
                new List<string> { "Gamma", "Delta", "Beta", "Alpha", "Gamma", "Delta", "Beta", "Alpha" },
                this.viewModel.SortedChildren.Select(node => node.Node).ToList()
            );
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(5000)]
        public async Task DatabaseViewModel_CopyUsername()
        {
            IKeePassEntry entry = this.viewModel.Document.Root.DatabaseGroup.GetChildEntry(1, 1, 1);

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            this.clipboardService.CredentialCopied += async (cs, typ) =>
            {
                Assert.AreEqual(ClipboardOperationType.UserName, typ, "CopyType should have been Username");

                string clipboardContent = await this.clipboard.GetContentAsText();
                Assert.AreSame(entry.UserName.ClearValue, clipboardContent, "Clipboard content should be the entry's username");

                tcs.SetResult(null);
            };
            
            this.viewModel.RequestCopyUsernameCommand.Execute(entry);
            await tcs.Task;
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(5000)]
        public async Task DatabaseViewModel_CopyPassword()
        {
            IKeePassEntry entry = this.viewModel.Document.Root.DatabaseGroup.GetChildEntry(1, 1, 1);

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            this.clipboardService.CredentialCopied += async (cs, typ) =>
            {
                Assert.AreEqual(ClipboardOperationType.Password, typ, "CopyType should have been Password");

                string clipboardContent = await this.clipboard.GetContentAsText();
                Assert.AreEqual(entry.Password.ClearValue, clipboardContent, "Clipboard content should be the entry's password");

                tcs.SetResult(null);
            };

            this.viewModel.RequestCopyPasswordCommand.Execute(entry);
            await tcs.Task;
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

            VerifyListsMatch(groupNames.Concat(entryNames).ToList(),
                this.viewModel.SortedChildren.Select(node => node.Node).ToList());
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
