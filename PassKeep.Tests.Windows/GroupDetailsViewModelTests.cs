using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.KeePassTests.Attributes;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PassKeep.KeePassTests
{
    [TestClass]
    public sealed class GroupDetailsViewModelTests : NodeDetailsViewModelTests<IGroupDetailsViewModel, IKeePassGroup>
    {
        private const string TestValue_GroupTitle = "TestTitleA";
        private const string TestValue_GroupNotes = "Test Notes\r\nTest test";
        private readonly bool? TestValue_AllowSearches = false;

        private const string StructureTestingDatabase = "StructureTesting.kdbx";

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
            Verify_New_Defaults();
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: false)]
        public void GroupDetailsViewModel_Existing_Defaults()
        {
            Verify_Existing_Defaults();
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: true)]
        public async Task GroupDetailsViewModel_Persist_New()
        {
            await Verify_Persist_New();
        }
        
        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: true)]
        public async Task GroupDetailsViewModel_CancelPersist_New()
        {
            await Verify_CancelPersist_New();
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: false)]
        public async Task GroupDetailsViewModel_Persist_Existing()
        {
            await Verify_Persist_Existing();
        }

        [TestMethod, DatabaseInfo(StructureTestingDatabase), DetailsFor(isNew: false)]
        public async Task GroupDetailsViewModel_CancelPersist_Existing()
        {
            await Verify_CancelPersist_Existing();
        }

        /// <summary>
        /// Creates an IGroupDetailsViewModel wrapping a new IKeePassGroup.
        /// </summary>
        /// <param name="navigationViewModel"></param>
        /// <param name="persistenceService"></param>
        /// <param name="document"></param>
        /// <param name="parent"></param>
        /// <returns>An IGroupDetailsViewModel to test.</returns>
        protected override IGroupDetailsViewModel GetNewViewModel(IDatabaseNavigationViewModel navigationViewModel, Lib.Contracts.Services.IDatabasePersistenceService persistenceService, Lib.KeePass.Dom.KdbxDocument document, IKeePassGroup parent)
        {
            return new GroupDetailsViewModel(
                navigationViewModel,
                persistenceService,
                document,
                document.Root.DatabaseGroup
            );
        }

        /// <summary>
        /// Creates an IGroupDetailsViewModel wrapping a specific existing IKeePassGroup.
        /// </summary>
        /// <param name="navigationViewModel"></param>
        /// <param name="persistenceService"></param>
        /// <param name="document"></param>
        /// <param name="openForReadOnly"></param>
        /// <returns>An IGroupDetailsViewModel to test.</returns>
        protected override IGroupDetailsViewModel GetExistingViewModel(IDatabaseNavigationViewModel navigationViewModel, Lib.Contracts.Services.IDatabasePersistenceService persistenceService, Lib.KeePass.Dom.KdbxDocument document, bool openForReadOnly)
        {
            return new GroupDetailsViewModel(
                navigationViewModel,
                persistenceService,
                document,
                document.Root.DatabaseGroup.Children[2] as IKeePassGroup,
                openForReadOnly
            );
        }

        /// <summary>
        /// Applies changes to a group for testing purposes.
        /// </summary>
        /// <param name="group">The group to modify.</param>
        protected override void MutateNode(IKeePassGroup group)
        {
            group.Title.ClearValue = TestValue_GroupTitle;
            group.Notes.ClearValue = TestValue_GroupNotes;
            group.EnableSearching = TestValue_AllowSearches;
        }

        /// <summary>
        /// Determines whether the specific group has expected test changes.
        /// </summary>
        /// <param name="group">The group to verify.</param>
        /// <returns>Whether the group has expected mutations.</returns>
        protected override bool HasAllMutations(IKeePassGroup group)
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
        protected override bool HasAnyMutations(IKeePassGroup group)
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
    }
}
