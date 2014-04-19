using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using System;
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
