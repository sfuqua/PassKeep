using PassKeep.Framework;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Eventing;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
using Windows.Storage;
using System.Threading;
using System.Collections.Generic;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Serves as a ViewModel for the root container of the app.
    /// </summary>
    public class RootViewModel : AbstractViewModel, IRootViewModel
    {
        private Stack<Tuple<string, CancellationTokenSource>> activeLoads;

        private IStorageFile _openedFile;
        private IClipboardClearTimerViewModel _clipboardViewModel;
        private IDatabaseParentViewModel _decryptedDatabase;
        private IPasswordGenViewModel _passwordGenViewModel;
        private IAppSettingsViewModel _appSettingsViewModel;
        private ISensitiveClipboardService clipboardService;
        private IAppSettingsService settingsService;

        /// <summary>
        /// Constructs the RootViewModel with the specified parameters.
        /// </summary>
        /// <param name="activationMode">How the app was launched.</param>
        /// <param name="openedFile">The file the app was opened to (or null).</param>
        /// <param name="passwordGenViewModel">The ViewModel for the password generation flyout.</param>
        /// <param name="appSettingsViewModel">The ViewModel for the settings flyout.</param>
        /// <param name="clipboardViewModel">a ViewModel over a clipboard clear timer.</param>
        /// <param name="clipboardService">A service for accessing the clipboard.</param>
        /// <param name="settingsService">A service for accessing app settings.</param>
        /// <param name="idleTimer">A timer used for computing idle timer.</param>
        public RootViewModel(
            ActivationMode activationMode,
            IStorageFile openedFile,
            IPasswordGenViewModel passwordGenViewModel,
            IAppSettingsViewModel appSettingsViewModel,
            IClipboardClearTimerViewModel clipboardViewModel,
            ISensitiveClipboardService clipboardService,
            IAppSettingsService settingsService
        )
        {
            if (passwordGenViewModel == null)
            {
                throw new ArgumentNullException(nameof(passwordGenViewModel));
            }

            if (appSettingsViewModel == null)
            {
                throw new ArgumentNullException(nameof(appSettingsViewModel));
            }

            if (clipboardViewModel == null)
            {
                throw new ArgumentNullException(nameof(clipboardViewModel));
            }

            if (clipboardService == null)
            {
                throw new ArgumentNullException(nameof(clipboardService));
            }

            this.ActivationMode = activationMode;
            this.CandidateFile = openedFile;

            this.PasswordGenViewModel = passwordGenViewModel;
            this.AppSettingsViewModel = appSettingsViewModel;

            this.ClipboardClearViewModel = clipboardViewModel;
            this.clipboardService = clipboardService;

            this.settingsService = settingsService;

            this.activeLoads = new Stack<Tuple<string, CancellationTokenSource>>();
        }

        public override void Activate()
        {
            base.Activate();

            this.AppSettingsViewModel.Activate();
            this.ClipboardClearViewModel.Activate();

            this.ClipboardClearViewModel.TimerComplete += this.ClipboardTimerComplete;
            this.clipboardService.CredentialCopied += this.ClipboardService_CredentialCopied;
        }

        public override void Suspend()
        {
            base.Suspend();

            this.ClipboardClearViewModel.TimerComplete -= this.ClipboardTimerComplete;
            this.clipboardService.CredentialCopied -= this.ClipboardService_CredentialCopied;

            this.AppSettingsViewModel.Suspend();
            this.ClipboardClearViewModel.Suspend();
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
        
        /// <summary>
        /// Text to display on a loading overlay.
        /// </summary>
        public string LoadingText
        {
            get
            {
                if (this.activeLoads.Count == 0)
                {
                    return String.Empty;
                }
                else
                {
                    return this.activeLoads.Peek().Item1;
                }
            }
        }

        /// <summary>
        /// Whether a load is in progress.
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return this.activeLoads.Count > 0;
            }
        }


        public ActivationMode ActivationMode
        {
            get;
            private set;
        }

        public IStorageFile CandidateFile
        {
            get { return this._openedFile;  }
            set   { SetProperty(ref this._openedFile, value); }
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

        public IAppSettingsViewModel AppSettingsViewModel
        {
            get { return this._appSettingsViewModel; }
            set { SetProperty(ref this._appSettingsViewModel, value); }
        }

        /// <summary>
        /// Starts the clipboard timer.
        /// </summary>
        /// <param name="sender">The ClipboardService.</param>
        /// <param name="args">The type of copy operation.</param>
        private void ClipboardService_CredentialCopied(ISensitiveClipboardService sender, ClipboardOperationType args)
        {
            this.ClipboardClearViewModel.StartTimer<ConcreteDispatcherTimer>(args);
        }

        /// <summary>
        /// Attempts to automatically clear the clipboard after a set time period.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ClipboardTimerComplete(object sender, ClipboardTimerCompleteEventArgs args)
        {
            IClipboardClearTimerViewModel vm = sender as IClipboardClearTimerViewModel;
            Dbg.Assert(vm != null);

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

        public void StartLoad(string loadingText, CancellationTokenSource cts)
        {
            this.activeLoads.Push(new Tuple<string, CancellationTokenSource>(loadingText, cts));
            OnPropertyChanged(nameof(LoadingText));
            OnPropertyChanged(nameof(IsLoading));
        }

        public void FinishLoad()
        {
            Dbg.Assert(this.activeLoads.Count > 0);
            this.activeLoads.Pop();
            OnPropertyChanged(nameof(LoadingText));
            OnPropertyChanged(nameof(IsLoading));
        }

        public void CancelCurrentLoad()
        {
            if (this.activeLoads.Count > 0)
            {
                this.activeLoads.Peek().Item2.Cancel();
            }
        }
    }
}
