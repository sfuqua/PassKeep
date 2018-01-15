// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
        /// Describes the composite master key for the new database.
        /// </summary>
        IMasterKeyViewModel MasterKeyViewModel { get; }

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
