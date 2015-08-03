using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Linq;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel representing a detailed View of an IKeePassGroup, that allows for editing.
    /// </summary>
    public sealed class EntryDetailsViewModel : NodeDetailsViewModel<IKeePassEntry>, IEntryDetailsViewModel
    {
        private IFieldEditorViewModel _fieldEditorViewModel;
        private IDatabaseEntryViewModel _workingCopyViewModel;

        private ResourceLoader resourceLoader;
        private ISensitiveClipboardService clipboardService;
        private IRandomNumberGenerator rng;

        /// <summary>
        /// Creates a ViewModel wrapping a brand new KdbxGroup as a child of the specified parent group.
        /// </summary>
        /// <param name="resourceLoader">ResourceLoader for localizing strings.</param>
        /// <param name="navigationViewModel">A ViewModel used for tracking navigation history.</param>
        /// <param name="persistenceService">A service used for persisting the document.</param>
        /// <param name="clipboardService">A service used for accessing the clipboard.</param>
        /// <param name="document">A KdbxDocument representing the database we are working on.</param>
        /// <param name="parentGroup">The IKeePassGroup to use as a parent for the new group.</param>
        /// <param name="rng">A random number generator used to protect strings in memory.</param>
        public EntryDetailsViewModel(
            ResourceLoader resourceLoader,
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            ISensitiveClipboardService clipboardService,
            KdbxDocument document,
            IKeePassGroup parentGroup,
            IRandomNumberGenerator rng
        ) : this(
            resourceLoader,
            navigationViewModel,
            persistenceService,
            clipboardService,
            document,
            new KdbxEntry(parentGroup, rng, document.Metadata),
            true,
            false,
            rng
        )
        {
            if (parentGroup == null)
            {
                throw new ArgumentNullException(nameof(parentGroup));
            }

            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }
        }

        /// <summary>
        /// Creates a ViewModel wrapping an existing KdbxGroup.
        /// </summary>
        /// <param name="resourceLoader">ResourceLoader for localizing strings.</param>
        /// <param name="navigationViewModel">A ViewModel used for tracking navigation history.</param>
        /// <param name="persistenceService">A service used for persisting the document.</param>
        /// <param name="clipboardService">A service used for accessing the clipboard.</param>
        /// <param name="document">A KdbxDocument representing the database we are working on.</param>
        /// <param name="entryToEdit">The entry being viewed.</param>
        /// <param name="isReadOnly">Whether to open the group in read-only mode.</param>
        /// <param name="rng">A random number generator used to protect strings in memory.</param>
        public EntryDetailsViewModel(
            ResourceLoader resourceLoader,
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            ISensitiveClipboardService clipboardService,
            KdbxDocument document,
            IKeePassEntry entryToEdit,
            bool isReadOnly,
            IRandomNumberGenerator rng
        ) : this(resourceLoader, navigationViewModel, persistenceService, clipboardService, document, entryToEdit, false, isReadOnly, rng)
        {
            if (entryToEdit == null)
            {
                throw new ArgumentNullException(nameof(entryToEdit));
            }
        }

        /// <summary>
        /// Passes provided parameters to the base constructor and initializes commands.
        /// </summary>
        /// <param name="resourceLoader">ResourceLoader for localizing strings.</param>
        /// <param name="navigationViewModel"></param>
        /// <param name="persistenceService"></param>
        /// <param name="clipboardService"></param>
        /// <param name="document"></param>
        /// <param name="entry"></param>
        /// <param name="isNew"></param>
        /// <param name="isReadOnly"></param>
        /// <param name="rng"></param>
        private EntryDetailsViewModel(
            ResourceLoader resourceLoader,
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            ISensitiveClipboardService clipboardService,
            KdbxDocument document,
            IKeePassEntry entry,
            bool isNew,
            bool isReadOnly,
            IRandomNumberGenerator rng
        ) : base(navigationViewModel, persistenceService, document, entry, isNew, isReadOnly)
        {
            this.resourceLoader = resourceLoader;
            this.clipboardService = clipboardService;
            this.rng = rng;

            this.CopyFieldValueCommand = new TypedCommand<IProtectedString>(
                str =>
                {
                    clipboardService.CopyCredential(str.ClearValue, ClipboardOperationType.Other);
                }
            );

            this.DeleteFieldCommand = new TypedCommand<IProtectedString>(
                str => !this.IsReadOnly,
                str =>
                {
                    Dbg.Assert(!this.IsReadOnly);
                    this.WorkingCopy.Fields.Remove(str);
                }
            );

            this.EditFieldCommand = new TypedCommand<IProtectedString>(
                str =>
                {
                    this.IsReadOnly = false;
                    this.FieldEditorViewModel = new FieldEditorViewModel(str, this.resourceLoader);
                }
            );

            this.NewFieldCommand = new ActionCommand(
                () =>
                {
                    this.IsReadOnly = false;
                    this.FieldEditorViewModel = new FieldEditorViewModel(this.rng, this.resourceLoader);
                }
            );

            this.CommitFieldCommand = new ActionCommand(
                () => this.FieldEditorViewModel != null && this.FieldEditorViewModel.CommitCommand.CanExecute(this.WorkingCopy),
                () =>
                {
                    this.FieldEditorViewModel.CommitCommand.Execute(this.WorkingCopy);
                    this.FieldEditorViewModel = null;
                }
            );

            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsReadOnly))
                {
                    ((TypedCommand<IProtectedString>)this.DeleteFieldCommand).RaiseCanExecuteChanged();
                }
                else if (e.PropertyName == nameof(WorkingCopy))
                {
                    OnPropertyChanged(nameof(WorkingCopyViewModel));
                }
            };
        }

        /// <summary>
        /// A ViewModel for interacting with the working copy.
        /// </summary>
        public IDatabaseEntryViewModel WorkingCopyViewModel
        {
            get
            {
                if (this._workingCopyViewModel?.Node != this.WorkingCopy)
                {
                    this._workingCopyViewModel = (this.WorkingCopy == null ? null :
                        new DatabaseEntryViewModel(this.WorkingCopy, this.clipboardService));
                }

                return this._workingCopyViewModel;
            }
        }

        /// <summary>
        /// A ViewModel over the current field being edited.
        /// </summary>
        public IFieldEditorViewModel FieldEditorViewModel
        {
            get { return this._fieldEditorViewModel; }
            private set
            {
                IFieldEditorViewModel oldViewModel = this._fieldEditorViewModel;
                if (TrySetProperty(ref this._fieldEditorViewModel, value))
                {
                    if (oldViewModel != null)
                    {
                        oldViewModel.Suspend();
                        oldViewModel.CommitCommand.CanExecuteChanged -= FieldEditViewModelCanCommitChanged;
                    }

                    if (value != null)
                    {
                        value.CommitCommand.CanExecuteChanged += FieldEditViewModelCanCommitChanged;
                        value.Activate();
                    }

                    ((ActionCommand)this.CommitFieldCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Copies the value of a field to the clipboard.
        /// </summary>
        public ICommand CopyFieldValueCommand
        {
            private set;
            get;
        }

        /// <summary>
        /// Deletes a field from the entry.
        /// </summary>
        public ICommand DeleteFieldCommand
        {
            private set;
            get;
        }

        /// <summary>
        /// Edits an existing field on the entry.
        /// </summary>
        public ICommand EditFieldCommand
        {
            private set;
            get;
        }

        /// <summary>
        /// Creates a new field for the entry and begins editing it.
        /// </summary>
        public ICommand NewFieldCommand
        {
            private set;
            get;
        }

        /// <summary>
        /// Commits the currently active field.
        /// </summary>
        public ICommand CommitFieldCommand
        {
            private set;
            get;
        }

        public override void Suspend()
        {
            base.Suspend();
            if (this.FieldEditorViewModel != null)
            {
                this.FieldEditorViewModel = null;
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
        /// Syncs an entry to a master copy.
        /// </summary>
        /// <param name="masterCopy">The entry to update to.</param>
        protected override void SynchronizeWorkingCopy(IKeePassEntry masterCopy)
        {
            this.WorkingCopy.SyncTo(masterCopy, false);
        }

        /// <summary>
        /// Adds an entry to its parent's collection of entries.
        /// </summary>
        /// <param name="nodeToAdd">The entry being added.</param>
        protected override void AddToParent(IKeePassEntry nodeToAdd)
        {
            if (nodeToAdd == null)
            {
                throw new ArgumentNullException(nameof(nodeToAdd));
            }

            if (nodeToAdd.Parent == null)
            {
                throw new ArgumentException("nodeToAdd must have a parent.", nameof(nodeToAdd));
            }

            nodeToAdd.Parent.Children.Add(nodeToAdd);
        }

        /// <summary>
        /// Replaces an entry within its parent's collection.
        /// </summary>
        /// <param name="document">The document being updated.</param>
        /// <param name="parent">The parent to update.</param>
        /// <param name="child">The entry to use as a replacement.</param>
        /// <param name="touchesNode">Whether to treat the swap as an "update" (vs a revert).</param>
        protected override void SwapIntoParent(KdbxDocument document, IKeePassGroup parent, IKeePassEntry child, bool touchesNode)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            // Otherwise, we need to find the equivalent existing child (by UUID) and 
            // update that way.
            IKeePassNode matchedNode = parent.Children.First(g => g.Uuid.Equals(child.Uuid));
            IKeePassEntry matchedEntry = matchedNode as IKeePassEntry;
            Dbg.Assert(matchedEntry != null);
            matchedEntry.SyncTo(child, touchesNode);
        }

        /// <summary>
        /// Removes an entry from the parent's collection of entries.
        /// </summary>
        /// <param name="nodeToRemove">The entry being removed.</param>
        protected override void RemoveFromParent(IKeePassEntry nodeToRemove)
        {
            if (nodeToRemove == null)
            {
                throw new ArgumentNullException(nameof(nodeToRemove));
            }

            if (nodeToRemove.Parent == null)
            {
                throw new ArgumentException("nodeToRemove must have a parent.", nameof(nodeToRemove));
            }

            nodeToRemove.Parent.Children.Remove(nodeToRemove);
        }

        /// <summary>
        /// Handler for when the <see cref="FieldEditorViewModel"/>'s CommitCommand's CanExecuteChange event fires.
        /// </summary>
        private void FieldEditViewModelCanCommitChanged(object sender, EventArgs e)
        {
            ((ActionCommand)this.CommitFieldCommand).RaiseCanExecuteChanged();
        }
    }
}
