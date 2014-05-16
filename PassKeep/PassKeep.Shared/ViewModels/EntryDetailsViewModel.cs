using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.Linq;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel representing a detailed View of an IKeePassGroup, that allows for editing.
    /// </summary>
    public sealed class EntryDetailsViewModel : NodeDetailsViewModel<IKeePassEntry>, IEntryDetailsViewModel
    {
        /// <summary>
        /// Creates a ViewModel wrapping a brand new KdbxGroup as a child of the specified parent group.
        /// </summary>
        /// <param name="navigationViewModel">A ViewModel used for tracking navigation history.</param>
        /// <param name="persistenceService">A service used for persisting the document.</param>
        /// <param name="document">A KdbxDocument representing the database we are working on.</param>
        /// <param name="parentGroup">The IKeePassGroup to use as a parent for the new group.</param>
        /// <param name="rng">A random number generator used to protect strings in memory.</param>
        public EntryDetailsViewModel(
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            KdbxDocument document,
            IKeePassGroup parentGroup,
            IRandomNumberGenerator rng
        ) : base(
            navigationViewModel,
            persistenceService,
            document,
            new KdbxEntry(parentGroup, rng, document.Metadata),
            true,
            false
        )
        {
            if (parentGroup == null)
            {
                throw new ArgumentNullException("parentGroup");
            }

            if (rng == null)
            {
                throw new ArgumentNullException("rng");
            }
        }

        /// <summary>
        /// Creates a ViewModel wrapping an existing KdbxGroup.
        /// <param name="navigationViewModel">A ViewModel used for tracking navigation history.</param>
        /// <param name="persistenceService">A service used for persisting the document.</param>
        /// <param name="document">A KdbxDocument representing the database we are working on.</param>
        /// <param name="entryToEdit">The entry being viewed.</param>
        /// <param name="isReadOnly">Whether to open the group in read-only mode.</param>
        public EntryDetailsViewModel(
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            KdbxDocument document,
            IKeePassEntry entryToEdit,
            bool isReadOnly
        ) : base(navigationViewModel, persistenceService, document, entryToEdit, false, isReadOnly)
        {
            if (entryToEdit == null)
            {
                throw new ArgumentNullException("groupToEdit");
            }
        }

        /// <summary>
        /// Generates a shallow copy of the specified entry.
        /// </summary>
        /// <param name="nodeToClone">The entry to clone.</param>
        /// <returns>A shallow clone of the passed entry.</returns>
        protected override IKeePassEntry GetClone(IKeePassEntry nodeToClone)
        {
            return nodeToClone.Clone();
        }

        /// <summary>
        /// Syncs a entry to a master copy.
        /// </summary>
        /// <param name="masterCopy">The entry to update to.</param>
        protected override void SynchronizeWorkingCopy(IKeePassEntry masterCopy)
        {
            this.WorkingCopy.SyncTo(masterCopy, false);
        }

        /// <summary>
        /// Adds a entry to its parent's collection of entries.
        /// </summary>
        /// <param name="nodeToAdd">The entry being added.</param>
        protected override void AddToParent(IKeePassEntry nodeToAdd)
        {
            if (nodeToAdd == null)
            {
                throw new ArgumentNullException("nodeToAdd");
            }

            if (nodeToAdd.Parent == null)
            {
                throw new ArgumentException("nodeToAdd must have a parent.", "nodeToAdd");
            }

            nodeToAdd.Parent.Entries.Add(nodeToAdd);
        }

        /// <summary>
        /// Replaces a entry within its parent's collection.
        /// </summary>
        /// <param name="document">The document being updated.</param>
        /// <param name="parent">The parent to update.</param>
        /// <param name="child">The entry to use as a replacement.</param>
        /// <param name="touchesNode">Whether to treat the swap as an "update" (vs a revert).</param>
        protected override void SwapIntoParent(KdbxDocument document, IKeePassGroup parent, IKeePassEntry child, bool touchesNode)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            if (child == null)
            {
                throw new ArgumentNullException("child");
            }

            // Otherwise, we need to find the equivalent existing child (by UUID) and 
            // update that way.
            parent.Entries.First(g => g.Uuid.Equals(child.Uuid)).SyncTo(child, touchesNode);
        }

        /// <summary>
        /// Removes a entry from the parent's collection of entries.
        /// </summary>
        /// <param name="nodeToRemove">The entry being removed.</param>
        protected override void RemoveFromParent(IKeePassEntry nodeToRemove)
        {
            if (nodeToRemove == null)
            {
                throw new ArgumentNullException("nodeToRemove");
            }

            if (nodeToRemove.Parent == null)
            {
                throw new ArgumentException("nodeToRemove must have a parent.", "nodeToRemove");
            }

            nodeToRemove.Parent.Entries.Remove(nodeToRemove);
        }
    }
}
