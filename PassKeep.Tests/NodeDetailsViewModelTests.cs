using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Tests.Attributes;
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

namespace PassKeep.Tests
{
    /// <summary>
    /// An abstract class allowing cohesive testing of all child detail ViewModels.
    /// </summary>
    /// <typeparam name="TViewModel">The type of ViewModel under test.</typeparam>
    /// <typeparam name="TNode">The type of child corresponding to the ViewModel.</typeparam>
    public abstract class NodeDetailsViewModelTests<TViewModel, TNode> : DatabasePersistenceViewModelTests<TViewModel>
        where TViewModel : INodeDetailsViewModel<TNode>
        where TNode : class, IKeePassNode
    {
        protected DateTime instantiationTime;
        protected IKeePassGroup expectedParent;
        protected KdbxDocument document;

        [TestInitialize]
        public async Task Initialize()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            MethodInfo testMethod = GetType().GetRuntimeMethod(
                TestContext.TestName, new Type[0]
            );

            var specAttr = testMethod.GetCustomAttribute<DetailsForAttribute>();
            var dataAttr = testMethod.GetCustomAttribute<TestDataAttribute>();
            Assert.IsTrue(specAttr != null || dataAttr != null);

            try
            {
                Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(TestContext);
                KdbxReader reader = new KdbxReader();

                using (IRandomAccessStream stream = await databaseInfo.Database.AsIStorageFile.OpenReadAsync())
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
        /// Verifies default values for a "new" child ViewModel.
        /// </summary>
        protected void Verify_New_Defaults()
        {
            Assert.IsFalse(this.viewModel.IsReadOnly);
            Assert.IsTrue(this.viewModel.IsNew);
            Assert.AreEqual(this.expectedParent, this.viewModel.WorkingCopy.Parent);
            Assert.IsFalse(this.expectedParent.Children.Contains(this.viewModel.WorkingCopy));
            Assert.IsTrue(this.viewModel.IsDirty());

            // Validate times
            Assert.IsNotNull(this.viewModel.WorkingCopy.Times, "IKeePassTimes instance should not be null on new node");
            Assert.IsTrue(this.viewModel.WorkingCopy.Times.CreationTime >= this.instantiationTime, "CreationTime should be accurate");
            Assert.IsTrue(this.viewModel.WorkingCopy.Times.LastModificationTime >= this.instantiationTime, "LastModificationTime should be accurate");
        }

        /// <summary>
        /// Verifies default values for an existing child ViewModel.
        /// </summary>
        protected void Verify_Existing_Defaults()
        {
            Assert.IsFalse(this.viewModel.IsReadOnly);
            Assert.IsFalse(this.viewModel.IsNew);
            Assert.AreEqual(this.expectedParent, this.viewModel.WorkingCopy.Parent);
            Assert.IsFalse(this.viewModel.IsDirty());

            TNode originalNode = this.expectedParent.Children
                .Single(n => n.Uuid.Equals(this.viewModel.WorkingCopy.Uuid))
                as TNode;
            Assert.AreNotSame(originalNode, this.viewModel.WorkingCopy);

            Assert.AreEqual(originalNode.Times, this.viewModel.WorkingCopy.Times, "IKeePassTimes instances should be equal");
            Assert.IsTrue(this.viewModel.WorkingCopy.Times.CreationTime < this.instantiationTime, "CreationTime should be accurate");
            Assert.IsTrue(this.viewModel.WorkingCopy.Times.LastModificationTime < this.instantiationTime, "LastModificationTime should be accurate");
        }

        /// <summary>
        /// Verifies state when persisting a new child to the database.
        /// </summary>
        /// <returns>A Task representing the verification.</returns>
        protected async Task Verify_Persist_New()
        {
            int originalChildNodeCount = this.expectedParent.Children.Count;

            // Make some changes to the new group
            MutateNode(this.viewModel.WorkingCopy);

            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should be sticky");

            Assert.IsTrue(this.viewModel.IsDirty(), "ViewModel should be dirty before saving");
            await this.viewModel.Save();
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Mutations should stick after save");
            Assert.IsFalse(this.viewModel.IsDirty(), "ViewModel should not be dirty right after saving");
            Assert.IsFalse(this.viewModel.IsNew, "ViewModel should no longer be new after a save");

            Assert.AreEqual(originalChildNodeCount + 1, this.expectedParent.Children.Count, "Saving a new group should add one child to the parent");
            Assert.AreSame(this.viewModel.WorkingCopy.Parent, this.expectedParent, "The WorkingCopy's parent should be wired correctly after a save");

            int addedNodeIndex = this.expectedParent.Children.IndexOf(this.viewModel.WorkingCopy);
            Assert.IsTrue(addedNodeIndex >= 0, "The WorkingCopy should exist in the parent's node collection after a save");

            TNode addedNode = this.expectedParent.Children[addedNodeIndex] as TNode;
            Assert.AreNotSame(addedNode, this.viewModel.WorkingCopy, "The saved node should not be ref-equals to the new WorkingCopy");
            Assert.AreEqual(addedNode, this.viewModel.WorkingCopy, "The saved node should be Equals to the new WorkingCopy");

            Assert.AreEqual(addedNode.Times, this.viewModel.WorkingCopy.Times, "IKeePassTimes instances should be equal");
        }

        /// <summary>
        /// Verifies state after persisting changes to an existing database child.
        /// </summary>
        /// <returns>A Task representing the verification.</returns>
        protected async Task Verify_Persist_Existing()
        {
            int originalChildNodeCount = this.expectedParent.Children.Count;
            TNode originalChild = this.expectedParent.Children.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid)) as TNode;
            DateTime? originalCreationTime = originalChild.Times.CreationTime;
            DateTime? originalModificationTime = originalChild.Times.LastModificationTime;

            Assert.AreNotSame(originalChild, this.viewModel.WorkingCopy, "The base node should not be ref-equal to the WorkingCopy");
            Assert.AreEqual(originalChild, this.viewModel.WorkingCopy, "The base node should be Equal to the WorkingCopy");

            // Make some changes to the existing group
            Assert.IsFalse(this.viewModel.IsDirty(), "ViewModel should not be dirty before mutations");
            MutateNode(this.viewModel.WorkingCopy);
            Assert.IsTrue(this.viewModel.IsDirty(), "ViewModel should be dirty after mutations");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should be sticky");
            Assert.IsFalse(HasAllMutations(originalChild), "Added node mutations should not apply to base group");
            Assert.AreNotEqual(originalChild, this.viewModel.WorkingCopy, "The base node should not be Equal to the WorkingCopy after mutations");

            // Save changes and verify they applied appropriately
            await this.viewModel.Save();
            Assert.IsTrue(HasAnyMutations(this.viewModel.WorkingCopy), "Mutations should stick after save");
            Assert.IsFalse(this.viewModel.IsDirty(), "ViewModel should no longer be dirty after save");

            TNode newChild = this.expectedParent.Children.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid)) as TNode;
            Assert.AreSame(originalChild, newChild, "Base node instance should be the same after saving");
            Assert.IsTrue(HasAllMutations(newChild), "Mutations should apply to base node after save");
            Assert.AreEqual(originalChildNodeCount, this.expectedParent.Children.Count, "Parent child count should not change after a save");

            Assert.AreEqual(newChild.Times.CreationTime, originalCreationTime, "CreationTime should not have changed");
            Assert.IsTrue(newChild.Times.LastModificationTime > originalModificationTime, "LastModificationTime should have updated");
        }

        /// <summary>
        /// Verifies state after reverting changes to an existing database child.
        /// </summary>
        protected void Verify_PermittedRevert_Existing()
        {
            int originalChildNodeCount = this.expectedParent.Children.Count;
            TNode originalChild = this.expectedParent.Children.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid)) as TNode;
            DateTime? originalCreationTime = originalChild.Times.CreationTime;
            DateTime? originalModificationTime = originalChild.Times.LastModificationTime;

            Assert.AreNotSame(originalChild, this.viewModel.WorkingCopy, "The base node should not be ref-equal to the WorkingCopy");
            Assert.AreEqual(originalChild, this.viewModel.WorkingCopy, "The base node should be Equal to the WorkingCopy");

            // Make some changes to the existing group
            Assert.IsFalse(this.viewModel.IsDirty(), "ViewModel should not be dirty before mutations");
            MutateNode(this.viewModel.WorkingCopy);
            Assert.IsTrue(this.viewModel.IsDirty(), "ViewModel should be dirty after mutations");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should be sticky");
            Assert.IsFalse(HasAllMutations(originalChild), "Added node mutations should not apply to base group");
            Assert.AreNotEqual(originalChild, this.viewModel.WorkingCopy, "The base node should not be Equal to the WorkingCopy after mutations");

            // Revert the ViewModel and permit the revert operation.
            bool eventFired = false;
            this.viewModel.RevertRequired += (s, e) =>
            {
                eventFired = true;
                this.viewModel.Revert();
            };
            this.viewModel.IsReadOnly = true;
            Assert.IsTrue(eventFired, "RevertRequested should have fired");
            Assert.IsFalse(HasAnyMutations(this.viewModel.WorkingCopy), "Mutations should be gone after revert");
            Assert.IsFalse(this.viewModel.IsDirty(), "ViewModel should no longer be dirty after revert");
            Assert.IsTrue(this.viewModel.IsReadOnly, "ViewModel should be read-only after revert");

            TNode newChild = this.expectedParent.Children.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid)) as TNode;
            Assert.AreSame(originalChild, newChild, "Base node instance should be the same after revert");

            Assert.IsTrue(newChild.Times.LastModificationTime == originalModificationTime, "LastModificationTime should not have changed");
        }

        /// <summary>
        /// Verifies state after ignoring a request torevert changes to an existing database child.
        /// </summary>
        protected void Verify_IgnoredRevert_Existing()
        {
            int originalChildNodeCount = this.expectedParent.Children.Count;
            TNode originalChild = this.expectedParent.Children.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid)) as TNode;
            DateTime? originalCreationTime = originalChild.Times.CreationTime;
            DateTime? originalModificationTime = originalChild.Times.LastModificationTime;

            Assert.AreNotSame(originalChild, this.viewModel.WorkingCopy, "The base node should not be ref-equal to the WorkingCopy");
            Assert.AreEqual(originalChild, this.viewModel.WorkingCopy, "The base node should be Equal to the WorkingCopy");

            // Make some changes to the existing group
            Assert.IsFalse(this.viewModel.IsDirty(), "ViewModel should not be dirty before mutations");
            MutateNode(this.viewModel.WorkingCopy);
            Assert.IsTrue(this.viewModel.IsDirty(), "ViewModel should be dirty after mutations");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should be sticky");
            Assert.IsFalse(HasAllMutations(originalChild), "Added node mutations should not apply to base group");
            Assert.AreNotEqual(originalChild, this.viewModel.WorkingCopy, "The base node should not be Equal to the WorkingCopy after mutations");

            // Revert the ViewModel and ignore the revert operation.
            bool eventFired = false;
            this.viewModel.RevertRequired += (s, e) =>
            {
                eventFired = true;
            };
            this.viewModel.IsReadOnly = true;
            Assert.IsTrue(eventFired, "RevertRequested should have fired");
            Assert.IsTrue(this.viewModel.IsDirty(), "ViewModel should be dirty after ignored revert");
            Assert.IsFalse(this.viewModel.IsReadOnly, "ViewModel should still be editable after ignored revert");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should still be sticky");
            Assert.IsFalse(HasAllMutations(originalChild), "Added node mutations should still not apply to base group");
            Assert.AreNotEqual(originalChild, this.viewModel.WorkingCopy, "The base node should still not be Equal to the WorkingCopy after mutations");
        }

        /// <summary>
        /// Verifies state after saving changes to an existing database child, from the revert handler.
        /// </summary>
        protected void Verify_PermittedRevertAfterSave_Existing()
        {
            int originalChildNodeCount = this.expectedParent.Children.Count;
            TNode originalChild = this.expectedParent.Children.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid)) as TNode;
            DateTime? originalCreationTime = originalChild.Times.CreationTime;
            DateTime? originalModificationTime = originalChild.Times.LastModificationTime;

            Assert.AreNotSame(originalChild, this.viewModel.WorkingCopy, "The base node should not be ref-equal to the WorkingCopy");
            Assert.AreEqual(originalChild, this.viewModel.WorkingCopy, "The base node should be Equal to the WorkingCopy");

            // Make some changes to the existing group
            Assert.IsFalse(this.viewModel.IsDirty(), "ViewModel should not be dirty before mutations");
            MutateNode(this.viewModel.WorkingCopy);
            Assert.IsTrue(this.viewModel.IsDirty(), "ViewModel should be dirty after mutations");
            Assert.IsTrue(HasAllMutations(this.viewModel.WorkingCopy), "Added node mutations should be sticky");
            Assert.IsFalse(HasAllMutations(originalChild), "Added node mutations should not apply to base group");
            Assert.AreNotEqual(originalChild, this.viewModel.WorkingCopy, "The base node should not be Equal to the WorkingCopy after mutations");

            // Revert the ViewModel and ignore the revert operation.
            bool eventFired = false;
            this.viewModel.RevertRequired += async (s, e) =>
            {
                eventFired = true;
                await this.viewModel.Save();
                this.viewModel.Revert();
            };
            this.viewModel.IsReadOnly = true;
            Assert.IsTrue(eventFired, "RevertRequested should have fired");
            Assert.IsTrue(HasAnyMutations(this.viewModel.WorkingCopy), "Mutations should stick after save");
            Assert.IsFalse(this.viewModel.IsDirty(), "ViewModel should no longer be dirty after save");
            Assert.IsTrue(this.viewModel.IsReadOnly, "ViewModel should be read-only after save");

            TNode newChild = this.expectedParent.Children.Single(g => g.Uuid.Equals(this.viewModel.WorkingCopy.Uuid)) as TNode;
            Assert.AreSame(originalChild, newChild, "Base node instance should be the same after saving");
            Assert.IsTrue(HasAllMutations(newChild), "Mutations should apply to base node after save");
            Assert.AreEqual(originalChildNodeCount, this.expectedParent.Children.Count, "Parent child count should not change after a save");

            Assert.AreEqual(newChild.Times.CreationTime, originalCreationTime, "CreationTime should not have changed");
            Assert.IsTrue(newChild.Times.LastModificationTime > originalModificationTime, "LastModificationTime should have updated");
        }

        /// <summary>
        /// Generates a ViewModel representing a new child.
        /// </summary>
        /// <param name="navigationViewModel"></param>
        /// <param name="persistenceService"></param>
        /// <param name="document"></param>
        /// <param name="parent"></param>
        /// <returns>A ViewModel representing a new child not already in the tree.</returns>
        protected abstract TViewModel GetNewViewModel(
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            KdbxDocument document,
            IKeePassGroup parent
        );
        
        /// <summary>
        /// Generats a ViewModel representing an existing child in the DOM.
        /// </summary>
        /// <param name="navigationViewModel"></param>
        /// <param name="persistenceService"></param>
        /// <param name="document"></param>
        /// <param name="openForReadOnly"></param>
        /// <returns>A ViewModel representing an existing child in the DOM.</returns>
        protected abstract TViewModel GetExistingViewModel(
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            KdbxDocument document,
            bool openForReadOnly
        );

        /// <summary>
        /// Applies changes to a child for testing purposes.
        /// </summary>
        /// <param name="child">The child to modify.</param>
        protected abstract void MutateNode(TNode node);

        /// <summary>
        /// Determines whether the specific child has expected test changes.
        /// </summary>
        /// <param name="child">The child to verify.</param>
        /// <returns>Whether the child has expected mutations.</returns>
        protected abstract bool HasAllMutations(TNode node);

        /// <summary>
        /// Determines whether the specific child has expected test changes.
        /// </summary>
        /// <param name="child">The child to verify.</param>
        /// <returns>Whether the child has expected mutations.</returns>
        protected abstract bool HasAnyMutations(TNode node);
    }
}
