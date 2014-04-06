using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassKeep.Common;
using SariphLib.Mvvm;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;

namespace PassKeep.ViewModels
{
    public class FieldEditViewModel : ViewModelBase
    {
        private IRandomNumberGenerator rng;
        public IKeePassEntry Entry { set; private get; }
        private IProtectedString backup;

        private IProtectedString workingCopy;
        public IProtectedString WorkingCopy
        {
            get { return workingCopy; }
            private set
            {
                SetProperty(ref workingCopy, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool editing;
        public bool Editing
        {
            get { return editing; }
            set
            {
                SetProperty(ref editing, value);
                if (!value)
                {
                    WorkingCopy = null;
                }
                else
                {
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private ActionCommand saveCommand;
        public ActionCommand SaveCommand
        {
            get { return saveCommand; }
            set { SetProperty(ref saveCommand, value); }
        }

        public event EventHandler<FieldChangedEventArgs> FieldChanged;
        private void onFieldChanged()
        {
            if (FieldChanged != null)
            {
                FieldChanged(this, new FieldChangedEventArgs(backup));
            }
        }

        public event EventHandler<ValidationErrorEventArgs> ValidationError;
        private void onValidationError(string err)
        {
            if (ValidationError != null)
            {
                ValidationError(this, new ValidationErrorEventArgs(err));
            }
        }

        private static List<string> invalidNames = new List<string> { "UserName", "Password", "Title", "Notes", "URL" };

        public FieldEditViewModel(IKeePassEntry entry, IRandomNumberGenerator rng, ConfigurationViewModel appSettings)
            : base(appSettings)
        {
            this.Entry = entry;
            this.rng = rng;
            SaveCommand = new ActionCommand(canSave, saveAction);
            Editing = false;
        }

        public void Edit(IProtectedString str = null)
        {
            backup = str;
            WorkingCopy = (str != null ? str.Clone() : new KdbxString(string.Empty, string.Empty, rng.Clone()));
            Editing = true;
        }

        private bool canSave()
        {
            if (!Editing || WorkingCopy == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(WorkingCopy.Key))
            {
                onValidationError("Please enter a field name.");
                return false;
            }

            if (invalidNames.Contains(WorkingCopy.Key))
            {
                onValidationError("That field name is reserved, please use a different one.");
                return false;
            }

            if ((backup == null || backup.Key != WorkingCopy.Key)
                && Entry.Fields.Select(f => f.Key).Contains(WorkingCopy.Key))
            {
                onValidationError("That field name is already being used, please use a different one.");
                return false;
            }

            return true;
        }

        private void saveAction()
        {
            Debug.Assert(Editing);
            Debug.Assert(WorkingCopy != null);
            if (!Editing || WorkingCopy == null)
            {
                throw new InvalidOperationException();
            }

            if (backup == null)
            {
                Entry.Fields.Add(WorkingCopy);
            }
            else
            {
                if (backup.Key != WorkingCopy.Key)
                {
                    backup.Key = WorkingCopy.Key;
                }
                if (backup.Protected != WorkingCopy.Protected)
                {
                    backup.Protected = WorkingCopy.Protected;
                }
                if (backup.ClearValue != WorkingCopy.ClearValue)
                {
                    backup.ClearValue = WorkingCopy.ClearValue;
                }

                onFieldChanged();
            }

            Editing = false;
        }
    }

    public class FieldChangedEventArgs : EventArgs
    {
        public IProtectedString Field { get; set; }
        public FieldChangedEventArgs(IProtectedString str)
        {
            Field = str;
        }
    }

    public class ValidationErrorEventArgs : EventArgs
    {
        public string Error { get; set; }
        public ValidationErrorEventArgs(string str)
        {
            Error = str;
        }
    }
}
