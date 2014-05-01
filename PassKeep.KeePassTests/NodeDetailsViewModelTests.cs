using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.KeePassTests.Attributes;
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.KeePassTests
{
    /// <summary>
    /// An abstract class allowing cohesive testing of all node detail ViewModels.
    /// </summary>
    /// <typeparam name="TViewModel">The type of ViewModel under test.</typeparam>
    /// <typeparam name="TNode">The type of node corresponding to the ViewModel.</typeparam>
    public abstract class NodeDetailsViewModelTests<TViewModel, TNode> : DatabasePersistenceViewModelTests<TViewModel>
        where TViewModel : INodeDetailsViewModel<TNode>
        where TNode : IKeePassNode
    {
        protected DateTime instantiationTime;
        protected IKeePassGroup expectedParent;
        protected KdbxDocument document;

        [TestInitialize]
        public async Task Initialize()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            MethodInfo testMethod = this.GetType().GetRuntimeMethod(
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

                        instantiationTime = DateTime.Now;
                        if (specAttr.IsNew)
                        {
                            this.expectedParent = this.document.Root.DatabaseGroup;
                            this.viewModel = GetNewViewModel(navVm, persistenceService, this.document, this.expectedParent);
                        }
                        else
                        {
                            this.expectedParent = this.document.Root.DatabaseGroup;
                            this.viewModel = GetExistingViewModel(
                                navVm,
                                persistenceService,
                                this.document,
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


        /// <summary>
        /// Verifies default values for a "new" node ViewModel.
        /// </summary>
        protected void Verify_New_Defaults()
        {
            Assert.IsFalse(this.viewModel.IsReadOnly);
            Assert.IsTrue(this.viewModel.IsNew);
            Assert.AreEqual(this.expectedParent, this.viewModel.WorkingCopy.Parent);
            Assert.IsFalse(GetParentNodeCollection(this.expectedParent).Contains(this.viewModel.WorkingCopy));

            // Validate times
            Assert.IsNotNull(this.viewModel.WorkingCopy.Times, "IKeePassTimes instance should not be null on new node");
            Assert.IsTrue(this.viewModel.WorkingCopy.Times.CreationTime >= this.instantiationTime, "CreationTime should be accurate");
            Assert.IsTrue(this.viewModel.WorkingCopy.Times.LastModificationTime >= this.instantiationTime, "LastModificationTime should be accurate");
        }

        /// <summary>
        /// Verifies default values for an existing node ViewModel.
        /// </summary>
        protected void Verify_Existing_Defaults()
        {
            Assert.IsFalse(this.viewModel.IsReadOnly);
            Assert.IsFalse(this.viewModel.IsNew);
            Assert.AreEqual(this.expectedParent, this.viewModel.WorkingCopy.Parent);

            TNode originalNode = GetParentNodeCollection(this.expectedParent)
                .Single(n => n.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));
            Assert.AreNotSame(originalNode, this.viewModel.WorkingCopy);

            Assert.AreEqual(originalNode.Times, this.viewModel.WorkingCopy.Times, "IKeePassTimes instances should be equal");
            Assert.IsTrue(this.viewModel.WorkingCopy.Times.CreationTime < this.instantiationTime, "CreationTime should be accurate");
            Assert.IsTrue(this.viewModel.WorkingCopy.Times.LastModificationTime < this.instantiationTime, "LastModificationTime should be accurate");
        }

        /// <summary>
        /// Verifies state when persisting a new node to the database.
        /// </summary>
        /// <returns>A Task representing the verification.</returns>
        protected async Task Verify_Persist_New()
        {
            int originalChildNodeCount = GetParentNodeCollection(this.expectedParent).Count;

            // Make some changes to the new group
            MutateNode(this.viewModel.WorkingCopy);

            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should be sticky");
            Assert.IsTrue(await this.viewModel.TrySave(), "Save operation should succeed");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Mutations should stick after save");

            Assert.AreEqual(originalChildNodeCount + 1, this.expectedParent.Groups.Count, "Saving a new group should add one child to the parent");
            Assert.AreSame(this.viewModel.WorkingCopy.Parent, this.expectedParent, "The WorkingCopy's parent should be wired correctly after a save");

            IList<TNode> nodeCollection = GetParentNodeCollection(this.expectedParent);
            int addedNodeIndex = nodeCollection.IndexOf(this.viewModel.WorkingCopy);
            Assert.IsTrue(addedNodeIndex >= 0, "The WorkingCopy should exist in the parent's node collection after a save");

            TNode addedNode = nodeCollection[addedNodeIndex];
            Assert.AreSame(addedNode, this.viewModel.WorkingCopy, "The saved node should be ref-equals to the WorkingCopy");

            Assert.AreEqual(addedNode.Times, this.viewModel.WorkingCopy.Times, "IKeePassTimes instances should be equal");
        }
        
        /// <summary>
        /// Verifies proper state after cancelling persistence of a new node.
        /// </summary>
        /// <returns>A Task representing the verification.</returns>
        protected async Task Verify_CancelPersist_New()
        {
            int originalChildNodeCount = GetParentNodeCollection(this.expectedParent).Count;

            // Make some changes to the new node
            MutateNode(this.viewModel.WorkingCopy);
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should be sticky");

            EventHandler<CancellableEventArgs> startedSaveHandler = null;
            startedSaveHandler = (s, e) =>
            {
                this.viewModel.StartedSave -= startedSaveHandler;
                e.Cts.Cancel();
            };
            this.viewModel.StartedSave += startedSaveHandler;
            
            Assert.IsFalse(await this.viewModel.TrySave(), "Save operation should have cancelled");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Mutations should stick after failed save");

            IList<TNode> nodeCollection = GetParentNodeCollection(this.expectedParent);
            Assert.AreEqual(originalChildNodeCount, nodeCollection.Count, "Cancelling saving a new node should not add children to the parent");
            Assert.AreSame(this.viewModel.WorkingCopy.Parent, this.expectedParent, "The WorkingCopy's parent should still be wired correctly after a save");

            int addedNodeIndex = nodeCollection.IndexOf(this.viewModel.WorkingCopy);
            Assert.IsTrue(addedNodeIndex == -1, "The WorkingCopy should not exist in the parent's node collection after a cancelled save");
        }

        /// <summary>
        /// Verifies state after persisting changes to an existing database node.
        /// </summary>
        /// <returns>A Task representing the verification.</returns>
        protected async Task Verify_Persist_Existing()
        {
            IList<TNode> nodeCollection = GetParentNodeCollection(this.expectedParent);
            int originalChildNodeCount = nodeCollection.Count;
            TNode originalChild = nodeCollection.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));
            DateTime? originalCreationTime = originalChild.Times.CreationTime;
            DateTime? originalModificationTime = originalChild.Times.LastModificationTime;

            Assert.AreNotSame(originalChild, this.viewModel.WorkingCopy, "The base node should not be ref-equal to the WorkingCopy");
            Assert.AreEqual(originalChild, this.viewModel.WorkingCopy, "The base node should be Equal to the WorkingCopy");

            // Make some changes to the existing group
            MutateNode(this.viewModel.WorkingCopy);
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should be sticky");
            Assert.IsFalse(HasAllMutations(originalChild), "Added node mutations should not apply to base group");
            Assert.AreNotEqual(originalChild, this.viewModel.WorkingCopy, "The base node should not be Equal to the WorkingCopy after mutations");

            // Save changes and verify they applied appropriately
            Assert.IsTrue(await this.viewModel.TrySave(), "Save operation should succeed");
            Assert.IsTrue(HasAnyMutations(this.viewModel.WorkingCopy), "Mutations should stick after save");

            nodeCollection = GetParentNodeCollection(this.expectedParent);
            TNode newChild = nodeCollection.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));
            Assert.AreSame(originalChild, newChild, "Base node instance should be the same after saving");
            Assert.IsTrue(HasAllMutations(newChild), "Mutations should apply to base node after save");
            Assert.AreEqual(originalChildNodeCount, nodeCollection.Count, "Parent child count should not change after a save");

            Assert.AreEqual(newChild.Times.CreationTime, originalCreationTime, "CreationTime should not have changed");
            Assert.IsTrue(newChild.Times.LastModificationTime > originalModificationTime, "LastModificationTime should have updated");
        }

        /// <summary>
        /// Verifies state after cancelling persistence of changes to an existing node.
        /// </summary>
        /// <returns>A Task representing the verification.</returns>
        protected async Task Verify_CancelPersist_Existing()
        {
            IList<TNode> nodeCollection = GetParentNodeCollection(this.expectedParent);
            int originalChildNodeCount = nodeCollection.Count;
            TNode originalChild = nodeCollection.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));
            DateTime? originalCreationTime = originalChild.Times.CreationTime;
            DateTime? originalModificationTime = originalChild.Times.LastModificationTime;

            Assert.AreNotSame(originalChild, this.viewModel.WorkingCopy, "The base node should not be ref-equal to the WorkingCopy");
            Assert.AreEqual(originalChild, this.viewModel.WorkingCopy, "The base node should be Equal to the WorkingCopy");

            // Make some changes to the existing group
            MutateNode(this.viewModel.WorkingCopy);
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should be sticky");
            Assert.IsFalse(HasAllMutations(originalChild), "Added node mutations should not apply to base group");
            Assert.AreNotEqual(originalChild, this.viewModel.WorkingCopy, "The base node should not be Equal to the WorkingCopy after mutations");

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

            nodeCollection = GetParentNodeCollection(this.expectedParent);
            TNode newChild = nodeCollection.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid));
            Assert.AreSame(originalChild, newChild, "Base node instance should be the same after cancelled save");
            Assert.IsFalse(HasAnyMutations(newChild), "Mutations should not apply to base node after cancelled save");
            Assert.AreEqual(originalChildNodeCount, nodeCollection.Count, "Parent child count should not change after a cancelled save");

            Assert.AreEqual(newChild.Times.CreationTime, originalCreationTime, "CreationTime should not have changed");
            Assert.AreEqual(newChild.Times.LastModificationTime, originalModificationTime, "LastModificationTime should not have changed");
        }

        /// <summary>
        /// Generates a ViewModel representing a new node.
        /// </summary>
        /// <param name="navigationViewModel"></param>
        /// <param name="persistenceService"></param>
        /// <param name="document"></param>
        /// <param name="parent"></param>
        /// <returns>A ViewModel representing a new node not already in the tree.</returns>
        protected abstract TViewModel GetNewViewModel(
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            KdbxDocument document,
            IKeePassGroup parent
        );
        
        /// <summary>
        /// Generats a ViewModel representing an existing node in the DOM.
        /// </summary>
        /// <param name="navigationViewModel"></param>
        /// <param name="persistenceService"></param>
        /// <param name="document"></param>
        /// <param name="openForReadOnly"></param>
        /// <returns>A ViewModel representing an existing node in the DOM.</returns>
        protected abstract TViewModel GetExistingViewModel(
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            KdbxDocument document,
            bool openForReadOnly
        );

        /// <summary>
        /// Gets the correct collection of TNodes from the parent.
        /// </summary>
        /// <param name="parent">The parent group we are interested in.</param>
        /// <returns>A collection of TNodes to search.</returns>
        protected abstract IList<TNode> GetParentNodeCollection(IKeePassGroup parent);

        /// <summary>
        /// Applies changes to a node for testing purposes.
        /// </summary>
        /// <param name="node">The node to modify.</param>
        protected abstract void MutateNode(TNode node);

        /// <summary>
        /// Determines whether the specific node has expected test changes.
        /// </summary>
        /// <param name="node">The node to verify.</param>
        /// <returns>Whether the node has expected mutations.</returns>
        protected abstract bool HasAllMutations(TNode node);

        /// <summary>
        /// Determines whether the specific node has expected test changes.
        /// </summary>
        /// <param name="node">The node to verify.</param>
        /// <returns>Whether the node has expected mutations.</returns>
        protected abstract bool HasAnyMutations(TNode node);
    }
}
