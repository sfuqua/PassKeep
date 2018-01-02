// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents an interface for a ViewModel used to edit custom fields of an entry.
    /// </summary>
    public class FieldEditorViewModel : AbstractViewModel, IFieldEditorViewModel
    {
        public const string Error_MissingKeyLoc = "FieldValidationError_MissingKey";
        public const string Error_ReservedKeyLoc = "FieldValidationError_ReservedKey";
        public const string Error_DuplicateKeyLoc = "FieldValidationError_DuplicateKey";

        public readonly string LocalizedMissingKey, LocalizedReservedKey, LocalizedDuplicateKey;

        // The list of reserved string keys
        private static readonly List<string> InvalidNames = new List<string> { "UserName", "Password", "Title", "Notes", "URL" };

        private TypedCommand<IKeePassEntry> commitCommand;

        private IProtectedString _workingCopy;
        private string _validationError;

        /// <summary>
        /// Initializes the save command and localized strings.
        /// </summary>
        /// <param name="resourceProvider"></param>
        private FieldEditorViewModel(IResourceProvider resourceProvider)
        {
            if (resourceProvider == null)
            {
                throw new ArgumentNullException(nameof(resourceProvider));
            }

            this.LocalizedMissingKey = resourceProvider.GetString(Error_MissingKeyLoc) ?? $"MISSING_STRING_FVEMK";
            this.LocalizedReservedKey = resourceProvider.GetString(Error_ReservedKeyLoc) ?? $"MISSING_STRING_FVERK";
            this.LocalizedDuplicateKey = resourceProvider.GetString(Error_DuplicateKeyLoc) ?? $"MISSING_STRING_FVEDK";

            this.commitCommand = new TypedCommand<IKeePassEntry>(CanSave, DoSave);
        }

        /// <summary>
        /// Initializes this ViewModel as an edit view over the specified protected string.
        /// </summary>
        /// <param name="stringToEdit">The string that this ViewModel wraps.</param>
        /// <param name="resourceProvider">ResourceLoader used for localization.</param>
        public FieldEditorViewModel(IProtectedString stringToEdit, IResourceProvider resourceProvider)
            : this(resourceProvider)
        {
            Original = stringToEdit ?? throw new ArgumentNullException(nameof(stringToEdit));
            WorkingCopy = Original.Clone();

            // Evaluate whether it's currently possible to save the string
            CanSave(null);
        }

        /// <summary>
        /// Initializes this ViewModel as an edit view over a new protected string.
        /// </summary>
        /// <param name="rng">The random number generator to use for the new string.</param>
        /// <param name="resourceProvider">ResourceLoader used for localization.</param>
        public FieldEditorViewModel(IRandomNumberGenerator rng, IResourceProvider resourceProvider)
            : this(resourceProvider)
        {
            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            Original = null;
            WorkingCopy = new KdbxString(String.Empty, String.Empty, rng, false);

            // Evaluate whether it's currently possible to save the string
            CanSave(null);
        }

        public override async Task SuspendAsync()
        {
            await base.SuspendAsync();
            WorkingCopy.PropertyChanged -= OnWorkingCopyPropertyChanged;
        }

        /// <summary>
        /// The backing string for <see cref="WorkingCopy"/>, or null for a new field.
        /// </summary>
        public IProtectedString Original
        {
            private set;
            get;
        }

        /// <summary>
        /// The copy of the string that is currently being manipulated.
        /// </summary>
        public IProtectedString WorkingCopy
        {
            get { return this._workingCopy; }
            private set
            {
                IProtectedString original = this._workingCopy;
                if (value != original)
                { 
                    if (original != null)
                    {
                        original.PropertyChanged -= OnWorkingCopyPropertyChanged;
                    }

                    if (value != null)
                    {
                        value.PropertyChanged += OnWorkingCopyPropertyChanged;
                    }

                    SetProperty(ref this._workingCopy, value);
                }
            }
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
            DebugHelper.Assert(sender == WorkingCopy);
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
            ValidationError = String.Empty;

            if (String.IsNullOrEmpty(WorkingCopy.Key))
            {
                ValidationError = this.LocalizedMissingKey;
            }

            if (FieldEditorViewModel.InvalidNames.Contains(WorkingCopy.Key))
            {
                ValidationError = this.LocalizedReservedKey;
            }

            if (baseEntry != null)
            {
                // If this is a new string, or if we've changed the key from the original,
                // validate that it doesn't clash with other strings in the entry's set.
                if ((Original == null || Original.Key != WorkingCopy.Key)
                    && baseEntry.Fields.Select(field => field.Key).Contains(WorkingCopy.Key))
                {
                    ValidationError = this.LocalizedDuplicateKey;
                }
            }

            return String.IsNullOrEmpty(ValidationError);
        }

        /// <summary>
        /// Attempts to save this field to the specified entry.
        /// </summary>
        /// <param name="baseEntry">The entry to save to.</param>
        private void DoSave(IKeePassEntry baseEntry)
        {
            if (baseEntry == null)
            {
                throw new ArgumentNullException(nameof(baseEntry));
            }

            if (!CanSave(baseEntry))
            {
                throw new InvalidOperationException("Cannot save in the current state.");
            }

            if (Original == null)
            {
                // New string...
                baseEntry.Fields.Add(WorkingCopy);
                Original = WorkingCopy;
                WorkingCopy = Original.Clone();
            }
            else
            {
                // Existing string...
                if (Original.Key != WorkingCopy.Key)
                {
                    Original.Key = WorkingCopy.Key;
                }

                if (Original.Protected != WorkingCopy.Protected)
                {
                    Original.Protected = WorkingCopy.Protected;
                }

                if (Original.ClearValue != WorkingCopy.ClearValue)
                {
                    Original.ClearValue = WorkingCopy.ClearValue;
                }
            }
        }
    }
}
