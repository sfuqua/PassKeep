using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Infrastructure;
using System;
using System.Linq;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel representing a detailed View of an IKeePassGroup, that allows for editing.
    /// </summary>
    public sealed class GroupDetailsViewModel : NodeDetailsViewModel<IKeePassGroup>, IGroupDetailsViewModel
    {
        /// <summary>
        /// Creates a ViewModel wrapping a brand new KdbxGroup as a child of the specified parent group.
        /// </summary>
        /// <param name="navigationViewModel">A ViewModel used for tracking navigation history.</param>
        /// <param name="persistenceService">A service used for persisting the document.</param>
        /// <param name="document">A KdbxDocument representing the database we are working on.</param>
        /// <param name="parentGroup">The IKeePassGroup to use as a parent for the new group.</param>
        public GroupDetailsViewModel(
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            KdbxDocument document,
            IKeePassGroup parentGroup
        ) : base(navigationViewModel, persistenceService,document, new KdbxGroup(parentGroup), true, false)
        {
            if (parentGroup == null)
            {
                throw new ArgumentNullException("parentGroup");
            }
        }

        /// <summary>
        /// Creates a ViewModel wrapping an existing KdbxGroup.
        /// <param name="navigationViewModel">A ViewModel used for tracking navigation history.</param>
        /// <param name="persistenceService">A service used for persisting the document.</param>
        /// <param name="document">A KdbxDocument representing the database we are working on.</param>
        /// <param name="groupToEdit">The group being viewed.</param>
        /// <param name="isReadOnly">Whether to open the group in read-only mode.</param>
        public GroupDetailsViewModel(
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            KdbxDocument document,
            IKeePassGroup groupToEdit,
            bool isReadOnly
        ) : base(navigationViewModel, persistenceService, document, groupToEdit, false, isReadOnly)
        {
            if (groupToEdit == null)
            {
                throw new ArgumentNullException("groupToEdit");
            }
        }

        /// <summary>
        /// Generates a shallow copy of the specified group.
        /// </summary>
        /// <param name="nodeToClone">The group to clone.</param>
        /// <returns>A shallow clone of the passed group.</returns>
        protected override IKeePassGroup GetClone(IKeePassGroup nodeToClone)
        {
            return nodeToClone.Clone();
        }

        /// <summary>
        /// Syncs a group to a master copy.
        /// </summary>
        /// <param name="masterCopy">The group to update to.</param>
        protected override void SynchronizeWorkingCopy(IKeePassGroup masterCopy)
        {
            this.WorkingCopy.SyncTo(masterCopy, false);
        }

        /// <summary>
        /// Adds a group to its parent's collection of groups.
        /// </summary>
        /// <param name="nodeToAdd">The group being added.</param>
        protected override void AddToParent(IKeePassGroup nodeToAdd)
        {
            if (nodeToAdd == null)
            {
                throw new ArgumentNullException("nodeToAdd");
            }

            if (nodeToAdd.Parent == null)
            {
                throw new ArgumentException("nodeToAdd must have a parent.", "nodeToAdd");
            }

            // Count the current number of groups; we want to insert at the index following the last
            // group.
            int insertIndex = nodeToAdd.Parent.Children.Count(node => node is IKeePassGroup);
            nodeToAdd.Parent.Children.Insert(insertIndex, nodeToAdd);
        }

        /// <summary>
        /// Replaces a group within its parent's collection.
        /// </summary>
        /// <param name="document">The document being updated.</param>
        /// <param name="parent">The parent to update.</param>
        /// <param name="child">The group to use as a replacement.</param>
        /// <param name="touchesNode">Whether to treat the swap as an "update" (vs a revert).</param>
        protected override void SwapIntoParent(KdbxDocument document, IKeePassGroup parent, IKeePassGroup child, bool touchesNode)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (child == null)
            {
                throw new ArgumentNullException("child");
            }

            if (child.Parent != parent)
            {
                throw new ArgumentException("child.Parent != parent");
            }

            if (parent == null)
            {
                // If there is no parent, we are updating the root database group.
                // So, just update it.
                document.Root.DatabaseGroup.SyncTo(child, touchesNode);
            }
            else
            {
                // Otherwise, we need to find the equivalent existing child (by UUID) and 
                // update that way.
                IKeePassNode matchedNode = parent.Children.First(node => node.Uuid.Equals(child.Uuid));
                IKeePassGroup matchedGroup = matchedNode as IKeePassGroup;
                Dbg.Assert(matchedGroup != null);
                matchedGroup.SyncTo(child, touchesNode);
            }
        }

        /// <summary>
        /// Removes a group from the parent's collection of groups.
        /// </summary>
        /// <param name="nodeToRemove">The group being removed.</param>
        protected override void RemoveFromParent(IKeePassGroup nodeToRemove)
        {
            if (nodeToRemove == null)
            {
                throw new ArgumentNullException("nodeToRemove");
            }

            if (nodeToRemove.Parent == null)
            {
                throw new ArgumentException("nodeToRemove must have a parent.", "nodeToRemove");
            }

            nodeToRemove.Parent.Children.Remove(nodeToRemove);
        }
    }
}
