using System;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using Windows.Storage;
using System.Windows.Input;
using SariphLib.Mvvm;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// ViewModel used to create a new database.
    /// </summary>
    public class DatabaseCreationViewModel : AbstractViewModel, IDatabaseCreationViewModel
    {
        private string _masterPassword, _confirmedPassword;
        private bool _rememberDatabase, _useEmpty;
        private StorageFile _keyFile;

        public DatabaseCreationViewModel(
            IStorageFile file
        )
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            this.File = file;
            this.CreateCommand = new ActionCommand(
                () => this.ConfirmedPassword == this.MasterPassword,
                () => { }
            );

            this.MasterPassword = String.Empty;
            this.ConfirmedPassword = String.Empty;
        }

        /// <summary>
        /// The new file being used for the database.
        /// </summary>
        public IStorageFile File
        {
            get;
            private set;
        }

        /// <summary>
        /// The password to use.
        /// </summary>
        public string MasterPassword
        {
            get { return this._masterPassword; }
            set
            {
                if (TrySetProperty(ref this._masterPassword, value))
                {
                    ((ActionCommand)this.CreateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Confirmation of password.
        /// </summary>
        public string ConfirmedPassword
        {
            get { return this._confirmedPassword; }
            set
            {
                if (TrySetProperty(ref this._confirmedPassword, value))
                {
                    ((ActionCommand)this.CreateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// The keyfile to use.
        /// </summary>
        public StorageFile KeyFile
        {
            get { return this._keyFile; }
            set
            {
                SetProperty(ref this._keyFile, value);
            }
        }

        /// <summary>
        /// Whether to remember this database in the future.
        /// </summary>
        public bool Remember
        {
            get { return this._rememberDatabase; }
            set { TrySetProperty(ref this._rememberDatabase, value); }
        }

        /// <summary>
        /// Whether to use an empty database instead of using the sample as a basis.
        /// </summary>
        public bool CreateEmpty
        {
            get { return this._useEmpty; }
            set { TrySetProperty(ref this._useEmpty, value); }
        }

        /// <summary>
        /// Command used to lock in settings and create the database.
        /// </summary>
        public ICommand CreateCommand
        {
            get;
            private set;
        }
    }
}
