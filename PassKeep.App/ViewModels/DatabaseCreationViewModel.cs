using System;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using Windows.Storage;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// ViewModel used to create a new database.
    /// </summary>
    public class DatabaseCreationViewModel : AbstractViewModel, IDatabaseCreationViewModel
    {
        public DatabaseCreationViewModel(
            IStorageFile file
        )
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            this.File = file;
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
            get;
            set;
        }

        /// <summary>
        /// Confirmation of password.
        /// </summary>
        public string ConfirmedPassword
        {
            get;
            set;
        }

        /// <summary>
        /// The keyfile to use.
        /// </summary>
        public StorageFile KeyFile
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to remember this database in the future.
        /// </summary>
        public bool Remember
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to use an empty database instead of using the sample as a basis.
        /// </summary>
        public bool CreateEmpty
        {
            get;
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
