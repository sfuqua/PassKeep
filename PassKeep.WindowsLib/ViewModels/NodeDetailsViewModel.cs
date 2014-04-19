using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// An abstract base class that encapsulates common functionality for editing/viewing
    /// groups and entries.
    /// </summary>
    /// <remarks>
    /// This class is instantiated with a reference to a node in the KDBX tree (or a brand new node).
    /// If the node is an existing node, the "WorkingCopy" being edited is backed up and the actual data source
    /// is cloned. This means that no edits are persisted in the "live" tree until a save occurs.
    /// </remarks>
    /// <typeparam name="T">The type of IKeePassNode wrapped by this ViewModel.</typeparam>
    public abstract class NodeDetailsViewModel<T> : DatabasePersistenceViewModel, INodeDetailsViewModel<T>
        where T : IKeePassNode
    {
        private KdbxDocument document;
        // The "backup" or "real" node that is kept pristine until a commit happens.
        private T masterCopy;

        private bool _isReadOnly;

        /// <summary>
        /// Initializes the base class from a subclass.
        /// </summary>
        /// <param name="navigationViewModel">A ViewModel used for tracking navigation history.</param>
        /// <param name="persistenceService">A service used for persisting the document.</param>
        /// <param name="document">A KdbxDocument representing the database we are working on.</param>
        /// <param name="item">The item this ViewModel wraps.</param>
        /// <param name="isNew">Whether the node is being created for the first time.</param>
        /// <param name="isReadOnly">Whether the node is being accessed as read-only.</param>
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
            this.document = document;
            this.IsNew = isNew;
            this.IsReadOnly = isReadOnly;
            this.masterCopy = item;

            if (!isNew)
            {
                this.WorkingCopy = GetClone(this.masterCopy);
            }
            else
            {
                this.WorkingCopy = this.masterCopy;
            }
        }

        /// <summary>
        /// A ViewModel used for tracking breadcrumbs to the current node.
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
                SetProperty(ref this._isReadOnly, value);
            }
        }

        /// <summary>
        /// Whether or not this node is new to the document (and so cannot be reverted).
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
        /// in the DOM, in the case of an update, or just inserting a new node if needed.
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
                // If this is a new node, the only needed step is to 
                // add it as a child of its parent. 
                // It is guaranteed that parent is not null in this situation.
                Debug.Assert(this.WorkingCopy.Parent != null);
                AddToParent(this.WorkingCopy);
            }
            else
            {
                // If this is an existing node, and it is the root, we need to swap it into 
                // the document.
                SwapIntoParent(this.document, this.masterCopy.Parent, this.WorkingCopy);
            }

            if (await base.TrySave())
            {
                return true;
            }

            // The save was cancelled. Revert...
            if (this.IsNew)
            {
                // If this was a new node, we simply need to remove it from the parent.
                RemoveFromParent(this.WorkingCopy);
            }
            else
            {
                // Otherwise, for existing nodes, we need to revert the group we previously updated.
                SwapIntoParent(this.document, this.masterCopy.Parent, this.masterCopy);
            }

            return false;
        }

        /// <summary>
        /// Reverts any pending changes to the underlying document.
        /// </summary>
        public void Revert()
        {
            if (this.IsReadOnly)
            {
                throw new InvalidOperationException("Cannot revert while in read-only mode.");
            }

            if (this.IsNew)
            {
                throw new InvalidOperationException("Cannot revert a new node.");
            }

            SynchronizeWorkingCopy(this.masterCopy);
        }

        /// <summary>
        /// Generates a shallow copy of the specified object.
        /// </summary>
        /// <param name="nodeToClone">The node to clone.</param>
        /// <returns>A shallow clone of the passed node.</returns>
        protected abstract T GetClone(T nodeToClone);

        /// <summary>
        /// Syncs a node to a master copy.
        /// </summary>
        /// <param name="masterCopy">The node to update to.</param>
        protected abstract void SynchronizeWorkingCopy(T masterCopy);

        /// <summary>
        /// Adds a node to the appropriate collection of its parent's.
        /// </summary>
        /// <param name="nodeToAdd">The node being added.</param>
        protected abstract void AddToParent(T nodeToAdd);

        /// <summary>
        /// Replaces a node within its parent's collection.
        /// </summary>
        /// <param name="document">The document being updated.</param>
        /// <param name="parent">The parent to update.</param>
        /// <param name="child">The node to use as a replacement.</param>
        protected abstract void SwapIntoParent(KdbxDocument document, IKeePassGroup parent, T child);

        /// <summary>
        /// Removes a node from the parent's appropriate collection.
        /// </summary>
        /// <param name="nodeToRemove">The node being removed.</param>
        protected abstract void RemoveFromParent(T nodeToRemove);
    }
}
