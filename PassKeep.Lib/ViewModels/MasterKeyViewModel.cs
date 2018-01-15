// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Provides information to a view that allows constructing a composite master key for a database.
    /// </summary>
    public sealed class MasterKeyViewModel : AbstractViewModel, IMasterKeyViewModel
    {
        private readonly ActionCommand confirmCommand;
        private string password;
        private string confirmedPassword;
        private ITestableFile keyFile;

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        public MasterKeyViewModel()
        {
            this.confirmCommand = new ActionCommand(
                () => this.password == this.confirmedPassword,
                () => Confirmed?.Invoke(this, EventArgs.Empty)
            );
        }

        /// <summary>
        /// Indicates that the user has confirmed the desired key settings.
        /// </summary>
        public event EventHandler Confirmed;

        /// <summary>
        /// The desired master password.
        /// </summary>
        public string MasterPassword
        {
            get => this.password;
            set
            {
                if (TrySetProperty(ref this.password, value))
                {
                    this.confirmCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// A confirmation of the value of <see cref="MasterPassword"/>.
        /// </summary>
        public string ConfirmedPassword
        {
            get => this.confirmedPassword;
            set
            {
                if (TrySetProperty(ref this.confirmedPassword, value))
                {
                    this.confirmCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Key file for the database - null if not used.
        /// </summary>
        public ITestableFile KeyFile
        {
            get => this.keyFile;
            set { this.keyFile = value; }
        }

        /// <summary>
        /// Command that is invoked when the user wishes to lock in the specified settings.
        /// </summary>
        public ICommand ConfirmCommand => this.confirmCommand;
    }
}
