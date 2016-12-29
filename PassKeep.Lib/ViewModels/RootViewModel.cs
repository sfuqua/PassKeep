using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Files;
using SariphLib.Infrastructure;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Serves as a ViewModel for the root container of the app.
    /// </summary>
    public class RootViewModel : AbstractViewModel, IRootViewModel
    {
        private readonly IAppSettingsService settingsService;
        private readonly ISensitiveClipboardService clipboardService;
        private readonly IFileExportService exportService;

        private ITestableFile _openedFile;
        private IDatabaseParentViewModel _decryptedDatabase;
        private IClipboardClearTimerViewModel _clipboardViewModel;
        private IPasswordGenViewModel _passwordGenViewModel;
        private IAppSettingsViewModel _appSettingsViewModel;

        /// <summary>
        /// Constructs the RootViewModel with the specified parameters.
        /// </summary>
        /// <param name="activationMode">How the app was launched.</param>
        /// <param name="openedFile">The file the app was opened to (or null).</param>
        /// <param name="passwordGenViewModel">The ViewModel for the password generation flyout.</param>
        /// <param name="appSettingsViewModel">The ViewModel for the settings flyout.</param>
        /// <param name="clipboardViewModel">a ViewModel over a clipboard clear timer.</param>
        /// <param name="taskNotificationService">A service used to control the UI for blocking operations.</param>
        /// <param name="clipboardService">A service for accessing the clipboard.</param>
        /// <param name="exportService">A service used for handling requests to export a file.</param>
        /// <param name="settingsService">A service for accessing app settings.</param>
        /// <param name="idleTimer">A timer used for computing idle timer.</param>
        public RootViewModel(
            ActivationMode activationMode,
            ITestableFile openedFile,
            IPasswordGenViewModel passwordGenViewModel,
            IAppSettingsViewModel appSettingsViewModel,
            IClipboardClearTimerViewModel clipboardViewModel,
            ITaskNotificationService taskNotificationService,
            ISensitiveClipboardService clipboardService,
            IFileExportService exportService,
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

            if (taskNotificationService == null)
            {
                throw new ArgumentNullException(nameof(taskNotificationService));
            }

            if (clipboardService == null)
            {
                throw new ArgumentNullException(nameof(clipboardService));
            }

            if (exportService == null)
            {
                throw new ArgumentNullException(nameof(exportService));
            }

            this.ActivationMode = activationMode;
            this.CandidateFile = openedFile;

            this.PasswordGenViewModel = passwordGenViewModel;
            this.AppSettingsViewModel = appSettingsViewModel;

            this.TaskNotificationService = taskNotificationService;

            this.ClipboardClearViewModel = clipboardViewModel;
            this.clipboardService = clipboardService;

            this.exportService = exportService;

            this.settingsService = settingsService;
        }

        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();
            
            await Task.WhenAll(
                this.AppSettingsViewModel.ActivateAsync(),
                this.ClipboardClearViewModel.ActivateAsync()
            );

            this.ClipboardClearViewModel.TimerComplete += ClipboardTimerComplete;
            this.clipboardService.CredentialCopied += ClipboardService_CredentialCopied;
            this.exportService.Exporting += ExportService_Exporting;
        }

        public override async Task SuspendAsync()
        {
            await base.SuspendAsync();

            this.ClipboardClearViewModel.TimerComplete -= ClipboardTimerComplete;
            this.clipboardService.CredentialCopied -= ClipboardService_CredentialCopied;
            this.exportService.Exporting -= ExportService_Exporting;

            await Task.WhenAll(
                this.AppSettingsViewModel.SuspendAsync(),
                this.ClipboardClearViewModel.SuspendAsync()
            );
        }

        public override void HandleAppSuspend()
        {
            base.HandleAppSuspend();
            this.ClipboardClearViewModel.HandleAppSuspend();
        }

        public override void HandleAppResume()
        {
            base.HandleAppResume();
            this.ClipboardClearViewModel.HandleAppResume();
        }

        /// <summary>
        /// Fired when the automated clipboard clear timer failed to clear the clipboard, in order to notify the view.
        /// </summary>
        public event EventHandler ClipboardClearFailed;

        /// <summary>
        /// Fired when the view should allow choosing a location to export a file.
        /// </summary>
        public event TypedEventHandler<IRootViewModel, FileRequestedEventArgs> ExportingCachedFile;

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
            this.ClipboardClearViewModel.StartTimer(args);
        }

        /// <summary>
        /// Handles picking a file for the export service.
        /// </summary>
        /// <param name="sender">The export service.</param>
        /// <param name="eventArgs">Args that will bubble to the view.</param>
        private void ExportService_Exporting(IFileExportService sender, FileRequestedEventArgs eventArgs)
        {
            ExportingCachedFile?.Invoke(this, eventArgs);
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
