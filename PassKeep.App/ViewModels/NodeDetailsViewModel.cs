using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// An abstract base class that encapsulates common functionality for editing/viewing
    /// groups and entries.
    /// </summary>
    /// <remarks>
    /// This class is instantiated with a reference to a child in the KDBX tree (or a brand new child).
    /// If the child is an existing child, the "WorkingCopy" being edited is backed up and the actual data source
    /// is cloned. This means that no edits are persisted in the "live" tree until a save occurs.
    /// </remarks>
    /// <typeparam name="T">The type of IKeePassNode wrapped by this ViewModel.</typeparam>
    public abstract class NodeDetailsViewModel<T> : DatabasePersistenceViewModel, INodeDetailsViewModel<T>
        where T : class, IKeePassNode
    {
        // The "backup" or "real" child that is kept pristine until a commit happens.
        private T masterCopy;

        private bool _isReadOnly;

        /// <summary>
        /// Initializes the base class from a subclass.
        /// </summary>
        /// <param name="navigationViewModel">A ViewModel used for tracking navigation history.</param>
        /// <param name="persistenceService">A service used for persisting the document.</param>
        /// <param name="document">A KdbxDocument representing the database we are working on.</param>
        /// <param name="item">The item this ViewModel wraps.</param>
        /// <param name="isNew">Whether the child is being created for the first time.</param>
        /// <param name="isReadOnly">Whether the child is being accessed as read-only.</param>
        protected NodeDetailsViewModel(
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            KdbxDocument document,
            T item,
            bool isNew,
            bool isReadOnly
        ) : base(document, persistenceService)
        {
            if (navigationViewModel == null)
            {
                throw new ArgumentNullException("navigationViewModel");
            }

            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (isNew && item.Parent == null)
            {
                throw new ArgumentException("Cannot create a new node with no parent!");
            }

            if (navigationViewModel.ActiveGroup != item.Parent)
            {
                throw new ArgumentException("The database's active group must be the node's parent!");
            }

            this.NavigationViewModel = navigationViewModel;
            this.Document = document;
            this.IsNew = isNew;

            if (!isNew)
            {
                this.masterCopy = GetClone(item);
                this.WorkingCopy = GetClone(this.masterCopy);
            }
            else
            {
                this.masterCopy = null;
                this.WorkingCopy = GetClone(item);
            }

            this.IsReadOnly = isReadOnly;
        }

        /// <summary>
        /// Raised when the ViewModel wishes to revert changes.
        /// </summary>
        public event EventHandler RevertRequired;
        protected void FireRevertRequired()
        {
            RevertRequired?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The KeePass Document which this node belongs to.
        /// </summary>
        public KdbxDocument Document
        {
            get;
            private set;
        }

        /// <summary>
        /// A ViewModel used for tracking breadcrumbs to the current child.
        /// </summary>
        public IDatabaseNavigationViewModel NavigationViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether or not editing is enabled for the View.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return this._isReadOnly;
            }
            set
            {
                // Going from ReadOnly -> Edit is always allowed
                if (this._isReadOnly)
                {
                    TrySetProperty(ref this._isReadOnly, value);
                }
                else if (value)
                {
                    // If we're not dirty, we can go ReadOnly
                    if (!IsDirty())
                    {
                        TrySetProperty(ref this._isReadOnly, true);
                    }
                    else
                    {
                        // If we are dirty, we can't allow Dave to do that.
                        // Fire the RevertRequired event and let the View try again later.
                        FireRevertRequired();
                    }
                }
            }
        }

        /// <summary>
        /// Whether or not this child is new to the document (and so cannot be reverted).
        /// </summary>
        public bool IsNew
        {
            get;
            private set;
        }

        /// <summary>
        /// The Node being viewed in detail.
        /// </summary>
        public T WorkingCopy
        {
            get;
            private set;
        }

        /// <summary>
        /// Commits changes to the underlying document.
        /// </summary>
        /// <remarks>
        /// This method is responsible for replacing the master copy with the working copy
        /// in the DOM, in the case of an update, or just inserting a new child if needed.
        /// The tricky part is if the save is cancelled, the surgery needs to be reverted.
        /// </remarks>
        /// <returns>A Task representing whether the commit was successful.</returns>
        public override async Task<bool> TrySave()
        {
            if (this.IsReadOnly)
            {
                throw new InvalidOperationException("Cannot commit, the node is in read-only mode.");
            }

            if (this.IsNew)
            {
                // If this is a new child, the only needed step is to 
                // add it as a child of its parent. 
                // It is guaranteed that parent is not null in this situation.
                Dbg.Assert(this.WorkingCopy.Parent != null);
                AddToParent(this.WorkingCopy);
            }
            else
            {
                // If this is an existing child, and it is the root, we need to swap it into 
                // the document.
                SwapIntoParent(this.Document, this.masterCopy.Parent, this.WorkingCopy, true);
            }

            if (await base.TrySave())
            {
                // On a successful save, update the master copy.
                // This ViewModel is also no longer "new".
                this.masterCopy = this.WorkingCopy;
                this.WorkingCopy = GetClone(this.masterCopy);
                this.IsNew = false;
                this.IsReadOnly = true;
                return true;
            }

            // The save was cancelled. Revert...
            if (this.IsNew)
            {
                // If this was a new child, we simply need to remove it from the parent.
                RemoveFromParent(this.WorkingCopy);
            }
            else
            {
                // Otherwise, for existing nodes, we need to revert the group we previously updated.
                SwapIntoParent(this.Document, this.masterCopy.Parent, this.masterCopy, false);
            }

            return false;
        }

        /// <summary>
        /// Reverts any pending changes to the underlying document.
        /// </summary>
        public void Revert()
        {
            // Reverting a new node is a no-op.
            if (this.IsReadOnly || this.IsNew)
            {
                return;
            }

            if (this.IsDirty())
            {
                SynchronizeWorkingCopy(this.masterCopy);
            }

            this.IsReadOnly = true;
        }

        /// <summary>
        /// Whether or not the WorkingCopy is different from the master.
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            return !this.WorkingCopy.Equals(this.masterCopy);
        }

        /// <summary>
        /// Generates a shallow copy of the specified object.
        /// </summary>
        /// <param name="nodeToClone">The child to clone.</param>
        /// <returns>A shallow clone of the passed child.</returns>
        protected abstract T GetClone(T nodeToClone);

        /// <summary>
        /// Syncs a child to a master copy.
        /// </summary>
        /// <param name="masterCopy">The child to update to.</param>
        protected abstract void SynchronizeWorkingCopy(T masterCopy);

        /// <summary>
        /// Adds a child to the appropriate collection of its parent's.
        /// </summary>
        /// <param name="nodeToAdd">The child being added.</param>
        protected abstract void AddToParent(T nodeToAdd);

        /// <summary>
        /// Replaces a child within its parent's collection.
        /// </summary>
        /// <param name="document">The document being updated.</param>
        /// <param name="parent">The parent to update.</param>
        /// <param name="child">The child to use as a replacement.</param>
        /// <param name="touchesNode">Whether to treat the swap as an "update" (vs a revert).</param>
        protected abstract void SwapIntoParent(KdbxDocument document, IKeePassGroup parent, T child, bool touchesNode);

        /// <summary>
        /// Removes a child from the parent's appropriate collection.
        /// </summary>
        /// <param name="nodeToRemove">The child being removed.</param>
        protected abstract void RemoveFromParent(T nodeToRemove);
    }
}
