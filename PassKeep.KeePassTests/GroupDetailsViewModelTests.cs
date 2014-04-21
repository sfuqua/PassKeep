using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.KeePassTests
{
    [TestClass]
    public class GroupDetailsViewModelTests : DatabasePersistenceViewModelTests<IGroupDetailsViewModel>
    {
        private const string TestValue_GroupTitle = "TestTitleA";
        private const string TestValue_GroupNotes = "Test Notes\r\nTest test";
        private readonly bool? TestValue_AllowSearches = false;

        private const string StructureTestingDatabase = "StructureTesting.kdbx";
        private IKeePassGroup expectedParent;
        private KdbxDocument document;

        [TestInitialize]
        public async Task Initialize()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            MethodInfo testMethod = typeof(GroupDetailsViewModelTests).GetRuntimeMethod(
                this.TestContext.TestName, new Type[0]
            );

            var specAttr = testMethod.GetCustomAttribute<DetailsForAttribute>();
            var dataAttr = testMethod.GetCustomAttribute<TestDataAttribute>();
            Assert.IsTrue(specAttr != null || dataAttr != null);

            try
            {
                Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
                KdbxReader reader = new KdbxReader();

                using (IRandomAccessStream stream = await databaseInfo.Database.OpenReadAsync())
                {
                    Assert.IsFalse((await reader.ReadHeader(stream, cts.Token)).IsError);
                    KdbxDecryptionResult decryption = await reader.DecryptFile(stream, databaseInfo.Password, databaseInfo.Keyfile, cts.Token);
                    
                    Assert.IsFalse(decryption.Result.IsError);
                    this.document = decryption.GetDocument();

                    if (specAttr != null && (dataAttr == null || !dataAttr.SkipInitialization))
                    {
                        IDatabaseNavigationViewModel navVm = new DatabaseNavigationViewModel();
                        navVm.SetGroup(this.document.Root.DatabaseGroup);

                        IDatabasePersistenceService persistenceService = new DummyPersistenceService();

                        if (specAttr.IsNew)
                        {
                            this.expectedParent = this.document.Root.DatabaseGroup;
                            this.viewModel = new GroupDetailsViewModel(
                                navVm,
                                persistenceService,
                                this.document,
                                expectedParent
                            );
                        }
                        else
                        {
                            this.expectedParent = this.document.Root.DatabaseGroup;
                            this.viewModel = new GroupDetailsViewModel(
                                navVm,
                                persistenceService,
                                this.document,
                                this.document.Root.DatabaseGroup.Groups[2],
                                specAttr.IsOpenedReadOnly
                            );
                        }
                    }
                    else
                    {
                        this.expectedParent = null;
                        Assert.IsTrue(dataAttr.SkipInitialization);
                    }
                }
            }
            catch (InvalidOperationException) { }
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: false), Timeout(1000)]
        public async Task GroupDetailsViewModel_DoSave()
        {
            await ValidateSave();
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: false), Timeout(1000)]
        public async Task GroupDetailsViewModel_DoCancelledSave()
        {
            await ValidateCancelledSave();
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: true)]
        public void GroupDetailsViewModel_New_Defaults()
        {
            Assert.IsFalse(this.viewModel.IsReadOnly);
            Assert.IsTrue(this.viewModel.IsNew);
            Assert.AreEqual(this.expectedParent, this.viewModel.WorkingCopy.Parent);
            Assert.IsFalse(this.viewModel.WorkingCopy.Parent.Groups.Contains(this.viewModel.WorkingCopy));
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: false)]
        public void GroupDetailsViewModel_Existing_Defaults()
        {
            Assert.IsFalse(this.viewModel.IsReadOnly);
            Assert.IsFalse(this.viewModel.IsNew);
            Assert.AreEqual(this.expectedParent, this.viewModel.WorkingCopy.Parent);

            IKeePassGroup originalGroup =
                this.viewModel.WorkingCopy.Parent.Groups.First(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));
            Assert.IsNotNull(originalGroup);
            Assert.AreNotSame(originalGroup, this.viewModel.WorkingCopy);
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: true)]
        public async Task GroupDetailsViewModel_Persist_New()
        {
            int originalChildGroupCount = this.expectedParent.Groups.Count;

            // Make some changes to the new group
            MutateGroup(this.viewModel.WorkingCopy);

            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added group mutations should be sticky");
            Assert.IsTrue(await this.viewModel.TrySave(), "Save operation should succeed");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Mutations should stick after save");

            Assert.AreEqual(originalChildGroupCount + 1, this.expectedParent.Groups.Count, "Saving a new group should add one child to the parent");
            Assert.AreSame(this.viewModel.WorkingCopy.Parent, this.expectedParent, "The WorkingCopy's parent should be wired correctly after a save");

            int addedGroupIndex = this.expectedParent.Groups.IndexOf(this.viewModel.WorkingCopy);
            Assert.IsTrue(addedGroupIndex >= 0, "The WorkingCopy should exist in the parent's group collection after a save");

            IKeePassGroup addedGroup = this.expectedParent.Groups[addedGroupIndex];
            Assert.AreSame(addedGroup, this.viewModel.WorkingCopy, "The saved group should be ref-equals to the WorkingCopy");
        }
        
        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: true)]
        public async Task GroupDetailsViewModel_CancelPersist_New()
        {
            int originalChildGroupCount = this.expectedParent.Groups.Count;

            // Make some changes to the new group
            MutateGroup(this.viewModel.WorkingCopy);
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added group mutations should be sticky");

            EventHandler<CancellableEventArgs> startedSaveHandler = null;
            startedSaveHandler = (s, e) =>
            {
                this.viewModel.StartedSave -= startedSaveHandler;
                e.Cts.Cancel();
            };
            this.viewModel.StartedSave += startedSaveHandler;
            
            Assert.IsFalse(await this.viewModel.TrySave(), "Save operation should have cancelled");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Mutations should stick after failed save");

            Assert.AreEqual(originalChildGroupCount, this.expectedParent.Groups.Count, "Cancelling saving a new group should not add children to the parent");
            Assert.AreSame(this.viewModel.WorkingCopy.Parent, this.expectedParent, "The WorkingCopy's parent should still be wired correctly after a save");

            int addedGroupIndex = this.expectedParent.Groups.IndexOf(this.viewModel.WorkingCopy);
            Assert.IsTrue(addedGroupIndex == -1, "The WorkingCopy should not exist in the parent's group collection after a cancelled save");
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: false)]
        public async Task GroupDetailsViewModel_Persist_Existing()
        {
            int originalChildGroupCount = this.expectedParent.Groups.Count;
            IKeePassGroup originalChild = this.expectedParent.Groups.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));

            Assert.AreNotSame(originalChild, this.viewModel.WorkingCopy, "The base group should not be ref-equal to the WorkingCopy");
            Assert.AreEqual(originalChild, this.viewModel.WorkingCopy, "The base group should be Equal to the WorkingCopy");

            // Make some changes to the existing group
            MutateGroup(this.viewModel.WorkingCopy);
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added group mutations should be sticky");
            Assert.IsFalse(HasAllMutations(originalChild), "Added group mutations should not apply to base group");
            Assert.AreNotEqual(originalChild, this.viewModel.WorkingCopy, "The base group should not be Equal to the WorkingCopy after mutations");

            // Save changes and verify they applied appropriately
            Assert.IsTrue(await this.viewModel.TrySave(), "Save operation should succeed");
            Assert.IsTrue(HasAnyMutations(this.viewModel.WorkingCopy), "Mutations should stick after save");

            IKeePassGroup newChild = this.expectedParent.Groups.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));
            Assert.AreSame(originalChild, newChild, "Base group instance should be the same after saving");
            Assert.IsTrue(HasAllMutations(newChild), "Mutations should apply to base group after save");
            Assert.AreEqual(originalChildGroupCount, this.expectedParent.Groups.Count, "Parent child count should not change after a save");
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: false)]
        public async Task GroupDetailsViewModel_CancelPersist_Existing()
        {
            int originalChildGroupCount = this.expectedParent.Groups.Count;
            IKeePassGroup originalChild = this.expectedParent.Groups.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));

            Assert.AreNotSame(originalChild, this.viewModel.WorkingCopy, "The base group should not be ref-equal to the WorkingCopy");
            Assert.AreEqual(originalChild, this.viewModel.WorkingCopy, "The base group should be Equal to the WorkingCopy");

            // Make some changes to the existing group
            MutateGroup(this.viewModel.WorkingCopy);
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added group mutations should be sticky");
            Assert.IsFalse(HasAllMutations(originalChild), "Added group mutations should not apply to base group");
            Assert.AreNotEqual(originalChild, this.viewModel.WorkingCopy, "The base group should not be Equal to the WorkingCopy after mutations");

            EventHandler<CancellableEventArgs> startedSaveHandler = null;
            startedSaveHandler = (s, e) =>
            {
                this.viewModel.StartedSave -= startedSaveHandler;
                e.Cts.Cancel();
            };
            this.viewModel.StartedSave += startedSaveHandler;

            // Save changes and verify they applied appropriately
            Assert.IsFalse(await this.viewModel.TrySave(), "Save operation should be cancelled");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Mutations should stick to WorkingCopy after cancelled save");

            IKeePassGroup newChild = this.expectedParent.Groups.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));
            Assert.AreSame(originalChild, newChild, "Base group instance should be the same after cancelled save");
            Assert.IsFalse(HasAnyMutations(newChild), "Mutations should not apply to base group after cancelled save");
            Assert.AreEqual(originalChildGroupCount, this.expectedParent.Groups.Count, "Parent child count should not change after a cancelled save");
        }

        /// <summary>
        /// Applies changes to a group for testing purposes.
        /// </summary>
        /// <param name="group">The group to modify.</param>
        private void MutateGroup(IKeePassGroup group)
        {
            this.viewModel.WorkingCopy.Title.ClearValue = TestValue_GroupTitle;
            this.viewModel.WorkingCopy.Notes.ClearValue = TestValue_GroupNotes;
            this.viewModel.WorkingCopy.EnableSearching = TestValue_AllowSearches;
        }

        /// <summary>
        /// Determines whether the specific group has expected test changes.
        /// </summary>
        /// <param name="group">The group to verify.</param>
        /// <returns>Whether the group has expected mutations.</returns>
        private bool HasAllMutations(IKeePassGroup group)
        {
            bool hasTitle = (group.Title.ClearValue == TestValue_GroupTitle);
            if (!hasTitle)
            {
                Debug.WriteLine("Expected title: {0}, actual title: {1}", TestValue_GroupTitle, group.Title.ClearValue);
            }

            bool hasNotes = (group.Notes.ClearValue == TestValue_GroupNotes);
            if (!hasNotes)
            {
                Debug.WriteLine("Expected notes: {0}, actual notes: {1}", TestValue_GroupNotes, group.Notes.ClearValue);
            }

            bool hasSearchValue = (group.EnableSearching == TestValue_AllowSearches);
            if (!hasSearchValue)
            {
                Debug.WriteLine("Expected search flag: {0}, actual search flag: {1}", TestValue_AllowSearches, group.EnableSearching);
            }

            return hasTitle && hasNotes && hasSearchValue;
        }

        /// <summary>
        /// Determines whether the specific group has expected test changes.
        /// </summary>
        /// <param name="group">The group to verify.</param>
        /// <returns>Whether the group has expected mutations.</returns>
        private bool HasAnyMutations(IKeePassGroup group)
        {
            bool hasTitle = (group.Title.ClearValue == TestValue_GroupTitle);
            if (!hasTitle)
            {
                Debug.WriteLine("Expected title: {0}, actual title: {1}", TestValue_GroupTitle, group.Title.ClearValue);
            }

            bool hasNotes = (group.Notes.ClearValue == TestValue_GroupNotes);
            if (!hasNotes)
            {
                Debug.WriteLine("Expected notes: {0}, actual notes: {1}", TestValue_GroupNotes, group.Notes.ClearValue);
            }

            bool hasSearchValue = (group.EnableSearching == TestValue_AllowSearches);
            if (!hasSearchValue)
            {
                Debug.WriteLine("Expected search flag: {0}, actual search flag: {1}", TestValue_AllowSearches, group.EnableSearching);
            }

            return hasTitle || hasNotes || hasSearchValue;
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        protected class DetailsForAttribute : Attribute
        {
            /// <summary>
            /// Whether the ViewModel represents a new node (instead of an existing one).
            /// </summary>
            public bool IsNew
            {
                get;
                private set;
            }

            /// <summary>
            /// Whether an existing node is being opened in read-only mode.
            /// </summary>
            public bool IsOpenedReadOnly
            {
                get;
                private set;
            }

            /// <summary>
            /// Simple initialization constructor.
            /// </summary>
            /// <param name="isNew">Whether the node is new or existing.</param>
            /// <param name="isOpenedReadOnly">Whether the node is opened in read-only mode.</param>
            public DetailsForAttribute(
                bool isNew = true,
                bool isOpenedReadOnly = false
            )
            {
                if (isNew && isOpenedReadOnly)
                {
                    throw new ArgumentException("Cannot open a new node in read-only mode!");
                }

                this.IsNew = isNew;
                this.IsOpenedReadOnly = IsOpenedReadOnly;
            }
        }
    }
}
