using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Eventing;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using Windows.Storage;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Serves as a ViewModel for the root container of the app.
    /// </summary>
    public class RootViewModel : BindableBase, IRootViewModel
    {
        private IStorageFile _openedFile;
        private IClipboardClearTimerViewModel _clipboardViewModel;
        private IDatabaseParentViewModel _decryptedDatabase;
        private IPasswordGenViewModel _passwordGenViewModel;
        private ISensitiveClipboardService clipboardService;

        /// <summary>
        /// Constructs the RootViewModel with the specified parameters.
        /// </summary>
        /// <param name="activationMode">How the app was launched.</param>
        /// <param name="openedFile">The file the app was opened to (or null).</param>
        /// <param name="clipboardViewModel">a ViewModel over a clipboard clear timer.</param>
        /// <param name="clipboardService">A service for accessing the clipboard.</param>
        public RootViewModel(
            ActivationMode activationMode,
            IStorageFile openedFile,
            IClipboardClearTimerViewModel clipboardViewModel,
            ISensitiveClipboardService clipboardService
        )
        {
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

            this.ClipboardClearViewModel = clipboardViewModel;
            this.ClipboardClearViewModel.TimerComplete += new WeakEventHandler<ClipboardTimerCompleteEventArgs>(this.ClipboardTimerComplete).Handler;

            this.clipboardService = clipboardService;
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

        public IStorageFile CandidateFile
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
    }
}
