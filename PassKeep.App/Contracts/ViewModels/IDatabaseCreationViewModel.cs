using System.Windows.Input;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// ViewModel for creating a new database.
    /// </summary>
    public interface IDatabaseCreationViewModel : IViewModel
    {
        /// <summary>
        /// The new file being used for the database.
        /// </summary>
        IStorageFile File
        {
            get;
        }

        /// <summary>
        /// The password to use.
        /// </summary>
        string MasterPassword
        {
            get;
            set;
        }

        /// <summary>
        /// Confirmation of password.
        /// </summary>
        string ConfirmedPassword
        {
            get;
            set;
        }

        /// <summary>
        /// The keyfile to use.
        /// </summary>
        StorageFile KeyFile
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to remember this database in the future.
        /// </summary>
        bool Remember
        {
            get;
            set;
        }

        /// <summary>
        /// Whether to use an empty database instead of using the sample as a basis.
        /// </summary>
        bool CreateEmpty
        {
            get;
        }

        /// <summary>
        /// Command used to lock in settings and create the database.
        /// </summary>
        ICommand CreateCommand
        {
            get;
        }
    }
}
