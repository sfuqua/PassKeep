using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.KeePassTests
{
    [TestClass]
    public class DatabaseNavigationViewModelTests : TestClassBase
    {
        private const string StructureTestingDatabase = "StructureTesting.kdbx";

        private KdbxDocument document;
        private IDatabaseNavigationViewModel viewModel;

        private enum UriState
        {
            Valid,
            Invalid,
            Missing
        }

        [TestInitialize]
        public async Task Initialize()
        {
            try
            {
                Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
                KdbxReader reader = new KdbxReader();

                using(IRandomAccessStream stream = await databaseInfo.Database.OpenReadAsync())
                {
                    Assert.IsFalse((await reader.ReadHeader(stream)).IsError);
                    KdbxDecryptionResult decryption = await reader.DecryptFile(stream, databaseInfo.Password, databaseInfo.Keyfile);

                    Assert.IsFalse(decryption.Result.IsError);
                    this.document = decryption.GetDocument();
                }
            }
            catch(InvalidOperationException) { }

            this.viewModel = new DatabaseNavigationViewModel();
        }

        [TestMethod]
        public void DatabaseNavigationViewModel_InitialState()
        {
            Assert.IsNotNull(this.viewModel.Breadcrumbs, "Breadcrumbs should not be null at initialization");
            Assert.AreEqual(0, this.viewModel.Breadcrumbs.Count, "Breadcrumbs should be empty at initialization");
            Assert.IsNull(this.viewModel.ActiveGroup, "ActiveGroup should be null at initialization");
            Assert.IsNull(this.viewModel.ActiveLeaf, "ActiveLeaf should be null at initialization");

            // Calling Prune should be safe, even with no data
            this.viewModel.Prune();
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseNavigationViewModel_SetGroup_Root()
        {
            this.viewModel.SetGroup(this.document.Root.DatabaseGroup);
            Assert.IsNotNull(this.viewModel.Breadcrumbs, "Breadcrumbs should not be null after SetGroup");
            Assert.AreEqual(1, this.viewModel.Breadcrumbs.Count, "Breadcrumbs should have the proper Count after SetGroup");
            Assert.AreSame(
                this.document.Root.DatabaseGroup,
                this.viewModel.Breadcrumbs[0],
                "Breadcrumbs should reflect the instance passed to SetGroup"
            );
            Assert.AreSame(
                this.document.Root.DatabaseGroup,
                this.viewModel.ActiveGroup,
                "ActiveGroup should be the value that SetGroup was passed"
            );

            Assert.IsNull(this.viewModel.ActiveLeaf, "ActiveLeaf should still be null");
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseNavigationViewModel_SetGroup_Grandchild()
        {
            this.viewModel.SetGroup(this.document.Root.DatabaseGroup.Groups[0].Groups[0]);
            Assert.AreEqual(3, this.viewModel.Breadcrumbs.Count, "Breadcrumbs should have the proper Count after SetGroup");

            Assert.AreSame(
                this.document.Root.DatabaseGroup,
                this.viewModel.Breadcrumbs[0],
                "Breadcrumbs should reflect the instance passed to SetGroup"
            );
            Assert.AreSame(
                this.document.Root.DatabaseGroup.Groups[0],
                this.viewModel.Breadcrumbs[1],
                "Breadcrumbs should reflect the instance passed to SetGroup"
            );
            Assert.AreSame(
                this.document.Root.DatabaseGroup.Groups[0].Groups[0],
                this.viewModel.Breadcrumbs[2],
                "Breadcrumbs should reflect the instance passed to SetGroup"
            );
            Assert.AreSame(
                this.document.Root.DatabaseGroup.Groups[0].Groups[0],
                this.viewModel.ActiveGroup,
                "ActiveGroup should be the value that SetGroup was passed"
            );

            Assert.IsNull(this.viewModel.ActiveLeaf, "ActiveLeaf should still be null");
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseNavigationViewModel_SetEntryAndPrune()
        {
            this.viewModel.SetEntry(this.document.Root.DatabaseGroup.Groups[1].Groups[1].Entries[1]);
            Assert.AreEqual(3, this.viewModel.Breadcrumbs.Count, "Breadcrumbs should have the proper Count after SetGroup");

            Assert.AreSame(
                this.document.Root.DatabaseGroup,
                this.viewModel.Breadcrumbs[0],
                "Breadcrumbs should reflect the instance passed to SetGroup"
            );
            Assert.AreSame(
                this.document.Root.DatabaseGroup.Groups[1],
                this.viewModel.Breadcrumbs[1],
                "Breadcrumbs should reflect the instance passed to SetGroup"
            );
            Assert.AreSame(
                this.document.Root.DatabaseGroup.Groups[1].Groups[1],
                this.viewModel.Breadcrumbs[2],
                "Breadcrumbs should reflect the instance passed to SetGroup"
            );
            Assert.AreSame(
                this.document.Root.DatabaseGroup.Groups[1].Groups[1],
                this.viewModel.ActiveGroup,
                "ActiveGroup should be the parent of the active entry"
            );

            Assert.AreSame(
                this.document.Root.DatabaseGroup.Groups[1].Groups[1].Entries[1],
                this.viewModel.ActiveLeaf,
                "ActiveLeaf should be the value that SetEntry was passed"
            );

            this.viewModel.Prune();
            Assert.IsNull(this.viewModel.ActiveLeaf, "ActiveLeaf should be null after pruning");
            Assert.AreEqual(3, this.viewModel.Breadcrumbs.Count, "Breadcrumbs should be unaffected by pruning");
            Assert.IsNotNull(this.viewModel.ActiveGroup, "ActiveGroup should be unaffected by pruning");
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(1000)]
        public async Task DatabaseNavigationViewModel_LeavesChangedFiresOnChildChange()
        {
            IKeePassGroup group = this.document.Root.DatabaseGroup.Groups[0].Groups[1];
            this.viewModel.SetGroup(group);

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            EventHandler eventHandler = null;
            eventHandler = (s, e) =>
            {
                this.viewModel.LeavesChanged -= eventHandler;
                tcs.SetResult(null);
            };

            this.viewModel.LeavesChanged += eventHandler;
            group.Entries.Add(new KdbxEntry(group, new Salsa20(new byte[32]), this.document.Metadata));

            await tcs.Task;
            Assert.IsNull(this.viewModel.ActiveLeaf, "There should still be no ActiveLeaf after appending an entry");

            tcs = new TaskCompletionSource<object>();

            eventHandler = (s, e) =>
            {
                this.viewModel.LeavesChanged -= eventHandler;
                tcs.SetResult(null);
            };

            this.viewModel.LeavesChanged += eventHandler;
            group.Entries.Clear();

            await tcs.Task;
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(1000)]
        public async Task DatabaseNavigationViewModel_LeavesChangedFiresOnGroupSwap1()
        {
            IKeePassGroup group = this.document.Root.DatabaseGroup.Groups[0].Groups[1];
            this.viewModel.SetGroup(group);

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            EventHandler eventHandler = null;
            eventHandler = (s, e) =>
            {
                this.viewModel.LeavesChanged -= eventHandler;
                tcs.SetResult(null);
            };

            this.viewModel.LeavesChanged += eventHandler;

            this.viewModel.SetGroup(group.Parent.Groups[0]);

            await tcs.Task;
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), Timeout(1000)]
        public async Task DatabaseNavigationViewModel_LeavesChangedFiresOnGroupSwap2()
        {
            IKeePassGroup group = this.document.Root.DatabaseGroup.Groups[0].Groups[0];
            this.viewModel.SetGroup(group);

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            EventHandler eventHandler = null;
            eventHandler = (s, e) =>
            {
                this.viewModel.LeavesChanged -= eventHandler;
                tcs.SetResult(null);
            };

            this.viewModel.LeavesChanged += eventHandler;

            this.viewModel.SetGroup(this.document.Root.DatabaseGroup.Groups[1].Groups[1]);

            await tcs.Task;
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public async Task DatabaseNavigationViewModel_LeavesChangedDoesntFireNeedlessly()
        {
            IKeePassGroup group = this.document.Root.DatabaseGroup.Groups[0].Groups[1];
            this.viewModel.SetGroup(group);

            bool eventFired = false;

            EventHandler eventHandler = null;
            eventHandler = (s, e) =>
            {
                eventFired = true;
            };

            this.viewModel.LeavesChanged += eventHandler;
            this.viewModel.SetGroup(group);

            await AwaitableTimeout(1000);
            Assert.IsFalse(eventFired, "LeavesChanged should not have fired");
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public async Task DatabaseNavigationViewModel_LeavesChangedUnsubscribes()
        {
            IKeePassGroup originalGroup = this.document.Root.DatabaseGroup.Groups[0].Groups[1];

            this.viewModel.SetGroup(originalGroup);
            this.viewModel.SetGroup(originalGroup.Parent);

            bool eventFired = false;

            EventHandler eventHandler = null;
            eventHandler = (s, e) =>
            {
                eventFired = true;
            };

            this.viewModel.LeavesChanged += eventHandler;
            originalGroup.Entries.Add(new KdbxEntry(originalGroup, new Salsa20(new byte[32]), this.document.Metadata));

            await AwaitableTimeout(1000);
            Assert.IsFalse(eventFired, "LeavesChanged should not have fired");
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseNavigationViewModel_UriLaunchConditions()
        {
            IList<Tuple<UriState, UriState>> uriStates = new List<Tuple<UriState, UriState>>
            {
                new Tuple<UriState, UriState>(UriState.Valid, UriState.Missing),
                new Tuple<UriState, UriState>(UriState.Valid, UriState.Valid),
                new Tuple<UriState, UriState>(UriState.Valid, UriState.Invalid),
                new Tuple<UriState, UriState>(UriState.Invalid, UriState.Missing),
                new Tuple<UriState, UriState>(UriState.Invalid, UriState.Valid),
                new Tuple<UriState, UriState>(UriState.Invalid, UriState.Invalid),
                new Tuple<UriState, UriState>(UriState.Missing, UriState.Missing),
                new Tuple<UriState, UriState>(UriState.Missing, UriState.Valid),
                new Tuple<UriState, UriState>(UriState.Missing, UriState.Invalid),
            };

            IKeePassGroup group = this.document.Root.DatabaseGroup.Groups[0].Groups[0];
            Assert.IsTrue(group.Entries.Count >= 9);

            this.viewModel.SetGroup(group);
            for(int i = 0; i < 9; i++)
            {
                this.viewModel.SetEntry(group.Entries[i]);

                bool expectedLaunchValue = ShouldUriBeLaunchable(uriStates[i].Item1, uriStates[i].Item2);
                bool actualLaunchValue = this.viewModel.UrlLaunchCommand.CanExecute(null);

                Assert.AreEqual(
                    expectedLaunchValue,
                    actualLaunchValue,
                    "Expected and actual values of UrlLaunchCommand.CanExecute should be equal in various states"
                );
            }
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseNavigationViewModel_SetEntryNegativeEvents()
        {
            IKeePassGroup group = this.document.Root.DatabaseGroup.Groups[0].Groups[0];
            Assert.IsTrue(group.Entries.Count > 1);

            bool eventFired = false;

            EventHandler leavesChangedEventHandler = null;
            leavesChangedEventHandler = (s, e) =>
            {
                this.viewModel.LeavesChanged -= leavesChangedEventHandler;
                eventFired = true;
            };

            PropertyChangedEventHandler propertyChangedEventHandler = null;
            propertyChangedEventHandler = (s, e) =>
            {
                if (e.PropertyName == "ActiveGroup")
                {
                    this.viewModel.PropertyChanged -= propertyChangedEventHandler;
                    eventFired = true;
                }
            };

            this.viewModel.SetGroup(group);

            this.viewModel.LeavesChanged += leavesChangedEventHandler;
            this.viewModel.PropertyChanged += propertyChangedEventHandler;

            for (int i = 0; i < group.Entries.Count; i++)
            {
                this.viewModel.SetEntry(group.Entries[i]);
                Assert.IsFalse(eventFired, "Calling SetEntry with a child of the ActiveGroup should not fire needless events");
            }
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase)]
        public void DatabaseNavigationViewModel_SetGroupPrunes()
        {
            IKeePassEntry activeEntry = this.document.Root.DatabaseGroup.Groups[0].Groups[0].Entries[0];
            this.viewModel.SetEntry(activeEntry);
            Assert.AreEqual(activeEntry, this.viewModel.ActiveLeaf, "ActiveLeaf should be the expected Entry after setting");

            this.viewModel.SetGroup(activeEntry.Parent);
            Assert.AreEqual(activeEntry, this.viewModel.ActiveLeaf, "ActiveLeaf should not change when SetGroup is a no-op");

            this.viewModel.SetGroup(this.document.Root.DatabaseGroup);
            Assert.IsNull(this.viewModel.ActiveLeaf, "ActiveLeaf should be null after setting a different active group");
        }

        /// <summary>
        /// Helper method to determine whether an entry has a launchable URL.
        /// </summary>
        /// <param name="entryUrl">The state of the entry's primary URL.</param>
        /// <param name="entryOverrideUrl">The state of the entry's override URL.</param>
        /// <returns></returns>
        private bool ShouldUriBeLaunchable(UriState entryUrl, UriState entryOverrideUrl)
        {
            if (entryOverrideUrl == UriState.Missing)
            {
                return entryUrl == UriState.Valid;
            }
            else
            {
                return entryOverrideUrl == UriState.Valid;
            }
        }
    }
}
