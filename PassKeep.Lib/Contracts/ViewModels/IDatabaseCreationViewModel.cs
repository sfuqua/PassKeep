using PassKeep.Lib.EventArgClasses;
using System;
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
        /// Invoked when the ViewModel begins generating a database file.
        /// </summary>
        event EventHandler<CancellableEventArgs> StartedGeneration;

        /// <summary>
        /// Invoked when the document has been successfully created.
        /// </summary>
        event EventHandler<DocumentReadyEventArgs> DocumentReady;

        /// <summary>
        /// Invoked when the ViewModel stops generating a database file.
        /// </summary>
        event EventHandler StoppedGeneration;

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
        /// The number of encryption rounds to use for the database.
        /// </summary>
        int EncryptionRounds
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
