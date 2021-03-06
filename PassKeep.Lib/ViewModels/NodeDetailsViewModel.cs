﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Diagnostics;
using System;
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

        private T _workingCopy;
        private bool _isNew;
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
            if (isNew && item.Parent == null)
            {
                throw new ArgumentException("Cannot create a new node with no parent!");
            }

            if (navigationViewModel.ActiveGroup != item.Parent)
            {
                throw new ArgumentException("The database's active group must be the node's parent!");
            }

            NavigationViewModel = navigationViewModel ?? throw new ArgumentNullException(nameof(navigationViewModel));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            IsNew = isNew;

            if (!isNew)
            {
                this.masterCopy = GetClone(item);
                WorkingCopy = GetClone(this.masterCopy);
            }
            else
            {
                this.masterCopy = null;
                WorkingCopy = GetClone(item);
            }

            IsReadOnly = isReadOnly;
        }

        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();

            NavigationViewModel.SetGroup(WorkingCopy.Parent);
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
            get { return this._isNew; }
            private set { TrySetProperty(ref this._isNew, value); }
        }

        /// <summary>
        /// The Node being viewed in detail.
        /// </summary>
        public T WorkingCopy
        {
            get { return this._workingCopy; }
            private set { SetProperty(ref this._workingCopy, value); }
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
        public override Task Save()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Cannot commit, the node is in read-only mode.");
            }

            if (IsNew)
            {
                // If this is a new child, the only needed step is to 
                // add it as a child of its parent. 
                // It is guaranteed that parent is not null in this situation.
                DebugHelper.Assert(WorkingCopy.Parent != null);
                AddToParent(WorkingCopy);
            }
            else
            {
                // If this is an existing child, and it is the root, we need to swap it into 
                // the document.
                SwapIntoParent(Document, this.masterCopy.Parent, WorkingCopy, true);
            }

            Task saveTask = base.Save();

            // On save, update the master copy.
            // This ViewModel is also no longer "new".
            this.masterCopy = WorkingCopy;
            WorkingCopy = GetClone(this.masterCopy);
            IsNew = false;
            IsReadOnly = true;

            return saveTask;
        }

        /// <summary>
        /// Reverts any pending changes to the underlying document.
        /// </summary>
        public void Revert()
        {
            // Reverting a new node is a no-op.
            if (IsReadOnly || IsNew)
            {
                return;
            }

            if (IsDirty())
            {
                SynchronizeWorkingCopy(this.masterCopy);
            }

            IsReadOnly = true;
        }

        /// <summary>
        /// Whether or not the WorkingCopy is different from the master.
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            return !WorkingCopy.Equals(this.masterCopy);
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
