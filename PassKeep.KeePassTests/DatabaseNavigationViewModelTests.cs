using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.ViewModels;
using PassKeep.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.KeePassTests
{
    [TestClass]
    public class DatabaseNavigationViewModelTests : TestClassBase
    {
        private const string StructureTestingDatabase = "StructureTesting.kdbx";

        private KdbxDocument document;
        private IDatabaseNavigationViewModel viewModel;

        public TestContext TestContext
        {
            get;
            set;
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
            this.viewModel.SetGroup(this.document.Root.DatabaseGroup.Groups[1].Groups[0]);

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

        /// <summary>
        /// Returns a Task that completes after the specified time.
        /// </summary>
        /// <param name="milliseconds">The number of seconds to spin the Task.</param>
        /// <returns>An awaitable Task that takes the specified amount of time to complete.</returns>
        private Task AwaitableTimeout(int milliseconds = 2000)
        {
            return Task.Run(() => new ManualResetEvent(false).WaitOne(milliseconds));
        }
    }
}
