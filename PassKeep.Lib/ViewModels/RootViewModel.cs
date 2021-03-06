﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Files;
using SariphLib.Diagnostics;
using System;
using System.Threading.Tasks;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Serves as a ViewModel for the root container of the app.
    /// </summary>
    public class RootViewModel : AbstractViewModel, IRootViewModel
    {
        private readonly IAppSettingsService settingsService;
        private readonly ISensitiveClipboardService clipboardService;

        private ITestableFile _openedFile;
        private IDatabaseParentViewModel _decryptedDatabase;
        private IClipboardClearTimerViewModel _clipboardViewModel;
        private IPasswordGenViewModel _passwordGenViewModel;
        private IHelpViewModel _helpViewModel;
        private IAppSettingsViewModel _appSettingsViewModel;

        /// <summary>
        /// Constructs the RootViewModel with the specified parameters.
        /// </summary>
        /// <param name="activationMode">How the app was launched.</param>
        /// <param name="openedFile">The file the app was opened to (or null).</param>
        /// <param name="passwordGenViewModel">The ViewModel for the password generation flyout.</param>
        /// <param name="helpViewModel">The ViewModel for the help flyout.</param>
        /// <param name="appSettingsViewModel">The ViewModel for the settings flyout.</param>
        /// <param name="clipboardViewModel">a ViewModel over a clipboard clear timer.</param>
        /// <param name="taskNotificationService">A service used to control the UI for blocking operations.</param>
        /// <param name="clipboardService">A service for accessing the clipboard.</param>
        /// <param name="settingsService">A service for accessing app settings.</param>
        /// <param name="idleTimer">A timer used for computing idle timer.</param>
        public RootViewModel(
            ActivationMode activationMode,
            ITestableFile openedFile,
            IPasswordGenViewModel passwordGenViewModel,
            IHelpViewModel helpViewModel,
            IAppSettingsViewModel appSettingsViewModel,
            IClipboardClearTimerViewModel clipboardViewModel,
            ITaskNotificationService taskNotificationService,
            ISensitiveClipboardService clipboardService,
            IAppSettingsService settingsService
        )
        {
            ActivationMode = activationMode;
            CandidateFile = openedFile;

            PasswordGenViewModel = passwordGenViewModel ?? throw new ArgumentNullException(nameof(passwordGenViewModel));
            HelpViewModel = helpViewModel ?? throw new ArgumentNullException(nameof(helpViewModel));
            AppSettingsViewModel = appSettingsViewModel ?? throw new ArgumentNullException(nameof(appSettingsViewModel));

            TaskNotificationService = taskNotificationService ?? throw new ArgumentNullException(nameof(taskNotificationService));

            ClipboardClearViewModel = clipboardViewModel ?? throw new ArgumentNullException(nameof(clipboardViewModel));
            this.clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));

            this.settingsService = settingsService;
        }

        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();
            
            await Task.WhenAll(
                AppSettingsViewModel.ActivateAsync(),
                ClipboardClearViewModel.ActivateAsync()
            );

            ClipboardClearViewModel.TimerComplete += ClipboardTimerComplete;
            this.clipboardService.CredentialCopied += ClipboardService_CredentialCopied;
        }

        public override async Task SuspendAsync()
        {
            await base.SuspendAsync();

            ClipboardClearViewModel.TimerComplete -= ClipboardTimerComplete;
            this.clipboardService.CredentialCopied -= ClipboardService_CredentialCopied;

            await Task.WhenAll(
                AppSettingsViewModel.SuspendAsync(),
                ClipboardClearViewModel.SuspendAsync()
            );
        }

        public override void HandleAppSuspend()
        {
            base.HandleAppSuspend();
            ClipboardClearViewModel.HandleAppSuspend();
        }

        public override void HandleAppResume()
        {
            base.HandleAppResume();
            ClipboardClearViewModel.HandleAppResume();
        }

        /// <summary>
        /// Fired when the automated clipboard clear timer failed to clear the clipboard, in order to notify the view.
        /// </summary>
        public event EventHandler ClipboardClearFailed;

        /// <summary>
        /// Invokes <see cref="ClipboardClearFailed"/>.
        /// </summary>
        private void FireClipboardClearFailed()
        {
            ClipboardClearFailed?.Invoke(this, EventArgs.Empty);
        }

        public ActivationMode ActivationMode
        {
            get;
            private set;
        }

        public ITestableFile CandidateFile
        {
            get { return this._openedFile;  }
            set { SetProperty(ref this._openedFile, value); }
        }

        public IClipboardClearTimerViewModel ClipboardClearViewModel
        {
            get { return this._clipboardViewModel; }
            set { SetProperty(ref this._clipboardViewModel, value); }
        }

        public IDatabaseParentViewModel DecryptedDatabase
        {
            get { return this._decryptedDatabase; }
            set { SetProperty(ref this._decryptedDatabase, value); }
        }

        public IPasswordGenViewModel PasswordGenViewModel
        {
            get { return this._passwordGenViewModel; }
            set { SetProperty(ref this._passwordGenViewModel, value); }
        }

        public IHelpViewModel HelpViewModel
        {
            get { return this._helpViewModel; }
            set { SetProperty(ref this._helpViewModel, value); }
        }

        public IAppSettingsViewModel AppSettingsViewModel
        {
            get { return this._appSettingsViewModel; }
            set { SetProperty(ref this._appSettingsViewModel, value); }
        }

        /// <summary>
        /// Notifies the view of UI-blocking operations.
        /// </summary>
        public ITaskNotificationService TaskNotificationService
        {
            get;
            private set;
        }

        /// <summary>
        /// Starts the clipboard timer.
        /// </summary>
        /// <param name="sender">The ClipboardService.</param>
        /// <param name="args">The type of copy operation.</param>
        private void ClipboardService_CredentialCopied(ISensitiveClipboardService sender, ClipboardOperationType args)
        {
            ClipboardClearViewModel.StartTimer(args);
        }

        /// <summary>
        /// Attempts to automatically clear the clipboard after a set time period.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ClipboardTimerComplete(object sender, ClipboardTimerCompleteEventArgs args)
        {
            IClipboardClearTimerViewModel vm = sender as IClipboardClearTimerViewModel;
            DebugHelper.Assert(vm != null);

            // First validate that we should still be clearing the clipboard.
            // For example, a user may have disabled the option while the timer was in-progress.
            if (args.TimerType == ClipboardOperationType.UserName && !vm.UserNameClearEnabled)
            {
                return;
            }
            else if (args.TimerType == ClipboardOperationType.Password && !vm.PasswordClearEnabled)
            {
                return;
            }

            // Attempt the clear...
            if (!this.clipboardService.TryClear())
            {
                FireClipboardClearFailed();
            }
        }
    }
}
