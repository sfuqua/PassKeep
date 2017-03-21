using PassKeep.Lib.EventArgClasses;
using SariphLib.Files;
using System;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// ViewModel for creating a new database.
    /// </summary>
    public interface IDatabaseCreationViewModel : IViewModel
    {
        /// <summary>
        /// Invoked when the document has been successfully created.
        /// </summary>
        event EventHandler<DocumentReadyEventArgs> DocumentReady;

        /// <summary>
        /// The new file being used for the database.
        /// </summary>
        ITestableFile File
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
        ITestableFile KeyFile
        {
            get;
            set;
        }

        /// <summary>
        /// Configures key derivation and encryption.
        /// </summary>
        IDatabaseSettingsViewModel Settings
        {
            get;
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
