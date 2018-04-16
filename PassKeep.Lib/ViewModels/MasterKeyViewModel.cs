// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Provides information to a view that allows constructing a composite master key for a database.
    /// </summary>
    public abstract class MasterKeyViewModel : AbstractViewModel, IMasterKeyViewModel
    {
        private readonly IFileAccessService fileService;
        private readonly AsyncActionCommand keyfileCommand;
        private readonly AsyncActionCommand confirmCommand;
        private string password;
        private string confirmedPassword;
        private ITestableFile keyFile;

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        public MasterKeyViewModel(IFileAccessService fileService)
        {
            this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));

            this.keyfileCommand = new AsyncActionCommand(
                async () =>
                {
                    KeyFile = await fileService.PickFileForOpenAsync();
                }
            );

            this.confirmCommand = new AsyncActionCommand(
                () => (!String.IsNullOrEmpty(this.password) || this.keyFile != null) && this.password == this.confirmedPassword,
                () => HandleCredentialsAsync(this.password, this.keyFile)
            );
        }

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
            set
            {
                if (TrySetProperty(ref this.keyFile, value))
                {
                    this.confirmCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// A command that allows specifying the value of <see cref="KeyFile"/>, e.g. through a chooser dialog.
        /// </summary>
        public IAsyncCommand ChooseKeyFileCommand => this.keyfileCommand;

        /// <summary>
        /// Command that is invoked when the user wishes to lock in the specified settings.
        /// </summary>
        public ICommand ConfirmCommand => this.confirmCommand;

        /// <summary>
        /// Called when <see cref="ConfirmCommand"/> is invoked.
        /// </summary>
        protected abstract Task HandleCredentialsAsync(string confirmedPassword, ITestableFile chosenKeyFile);
    }
}
