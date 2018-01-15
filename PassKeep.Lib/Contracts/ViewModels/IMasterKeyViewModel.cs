// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Files;
using System;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Provides view information for a database's composite master key.
    /// </summary>
    public interface IMasterKeyViewModel
    {
        /// <summary>
        /// Fired when <see cref="ConfirmCommand"/> is successfully invoked.
        /// </summary>
        event EventHandler Confirmed;

        /// <summary>
        /// The password to use for database encryption.
        /// </summary>
        string MasterPassword
        {
            get;
            set;
        }

        /// <summary>
        /// User-provided confirmation of <see cref="MasterPassword"/>.
        /// </summary>
        string ConfirmedPassword
        {
            get;
            set;
        }

        /// <summary>
        /// The keyfile to use for encrypting the database.
        /// </summary>
        ITestableFile KeyFile
        {
            get;
            set;
        }

        /// <summary>
        /// A command that confirms the specified master key settings (and can only execute if <see cref="ConfirmedPassword"/> is correct.
        /// </summary>
        ICommand ConfirmCommand
        {
            get;
        }
    }
}
