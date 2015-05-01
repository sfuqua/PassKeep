using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Eventing;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents an interface for a ViewModel used to edit custom fields of an entry.
    /// </summary>
    public class FieldEditorViewModel : BindableBase, IFieldEditorViewModel
    {
        public const string Error_MissingKey = "Please enter a field name.";
        public const string Error_ReservedKey = "That field name is reserved, please use a different one.";
        public const string Error_DuplicateKey = "That field name is already being used, please use a different one.";

        // The list of reserved string keys
        private static readonly List<string> InvalidNames = new List<string> { "UserName", "Password", "Title", "Notes", "URL" };

        private IProtectedString originalString;
        private TypedCommand<IKeePassEntry> commitCommand;

        private IProtectedString _workingCopy;
        private string _validationError;

        /// <summary>
        /// Initializes this ViewModel as an edit view over the specified protected string.
        /// </summary>
        /// <param name="stringToEdit">The string that this ViewModel wraps.</param>
        public FieldEditorViewModel(IProtectedString stringToEdit)
        {
            if (stringToEdit == null)
            {
                throw new ArgumentNullException("stringToEdit");
            }

            this.originalString = stringToEdit;
            this.WorkingCopy = this.originalString.Clone();
            this.commitCommand = new TypedCommand<IKeePassEntry>(CanSave, DoSave);

            this.WorkingCopy.PropertyChanged +=
                new WeakEventHandler<PropertyChangedEventArgs>(OnWorkingCopyPropertyChanged).Handler;

            // Evaluate whether it's currently possible to save the string
            this.CanSave(null);
        }

        /// <summary>
        /// Initializes this ViewModel as an edit view over a new protected string.
        /// </summary>
        /// <param name="rng">The random number generator to use for the new string.</param>
        public FieldEditorViewModel(IRandomNumberGenerator rng)
        {
            if (rng == null)
            {
                throw new ArgumentNullException("rng");
            }

            this.originalString = null;
            this.WorkingCopy = new KdbxString(String.Empty, String.Empty, rng, false);
            this.commitCommand = new TypedCommand<IKeePassEntry>(CanSave, DoSave);

            this.WorkingCopy.PropertyChanged +=
                new WeakEventHandler<PropertyChangedEventArgs>(OnWorkingCopyPropertyChanged).Handler;

            // Evaluate whether it's currently possible to save the string
            this.CanSave(null);
        }

        /// <summary>
        /// The copy of the string that is currently being manipulated.
        /// </summary>
        public IProtectedString WorkingCopy
        {
            get { return this._workingCopy; }
            private set { SetProperty(ref this._workingCopy, value); }
        }

        /// <summary>
        /// The command used for persisting changes to the original string.
        /// </summary>
        public ICommand CommitCommand
        {
            get { return this.commitCommand; }
        }

        /// <summary>
        /// The last validation error (if one exists) for the string.
        /// </summary>
        public string ValidationError
        {
            get { return this._validationError; }
            private set
            {
                TrySetProperty(ref this._validationError, value);
            }
        }

        /// <summary>
        /// Handles PropertyChanged events for the WorkingCopy string.
        /// </summary>
        /// <param name="sender">The IProtectedString that raised the event (WorkingCopy).</param>
        /// <param name="args">EventArgs for the PropertyChanged event.</param>
        private void OnWorkingCopyPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            Dbg.Assert(sender == this.WorkingCopy);
            if (args.PropertyName == "Key")
            {
                this.commitCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Determines whether this is in a currently valid save-able state.
        /// </summary>
        /// <param name="baseEntry">The entry we would like to save to.</param>
        /// <returns>Whether a save is possible.</returns>
        private bool CanSave(IKeePassEntry baseEntry)
        {
            this.ValidationError = String.Empty;

            if (string.IsNullOrEmpty(this.WorkingCopy.Key))
            {
                this.ValidationError = FieldEditorViewModel.Error_MissingKey;
            }

            if (FieldEditorViewModel.InvalidNames.Contains(this.WorkingCopy.Key))
            {
                this.ValidationError = FieldEditorViewModel.Error_ReservedKey;
            }

            if (baseEntry != null)
            {
                // If this is a new string, or if we've changed the key from the original,
                // validate that it doesn't clash with other strings in the entry's set.
                if ((this.originalString == null || this.originalString.Key != this.WorkingCopy.Key)
                    && baseEntry.Fields.Select(field => field.Key).Contains(this.WorkingCopy.Key))
                {
                    this.ValidationError = FieldEditorViewModel.Error_DuplicateKey;
                }
            }

            return String.IsNullOrEmpty(this.ValidationError);
        }

        /// <summary>
        /// Attempts to save this field to the specified entry.
        /// </summary>
        /// <param name="baseEntry">The entry to save to.</param>
        private void DoSave(IKeePassEntry baseEntry)
        {
            if (baseEntry == null)
            {
                throw new ArgumentNullException("baseEntry");
            }

            if (!CanSave(baseEntry))
            {
                throw new InvalidOperationException("Cannot save in the current state.");
            }

            if (this.originalString == null)
            {
                // New string...
                baseEntry.Fields.Add(this.WorkingCopy);
                this.originalString = this.WorkingCopy;
                this.WorkingCopy = this.originalString.Clone();
            }
            else
            {
                // Existing string...
                if (this.originalString.Key != this.WorkingCopy.Key)
                {
                    this.originalString.Key = this.WorkingCopy.Key;
                }

                if (this.originalString.Protected != this.WorkingCopy.Protected)
                {
                    this.originalString.Protected = this.WorkingCopy.Protected;
                }

                if (this.originalString.ClearValue != this.WorkingCopy.ClearValue)
                {
                    this.originalString.ClearValue = this.WorkingCopy.ClearValue;
                }
            }
        }
    }
}
