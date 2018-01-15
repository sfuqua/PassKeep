// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Files;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using PassKeep.Lib.EventArgClasses;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that tracks database state and can generate ViewModels for child views.
    /// </summary>
    public sealed class DatabaseParentViewModel : DatabasePersistenceViewModel, IDatabaseParentViewModel
    {
        private ITestableFile file;
        private bool fileIsSample;
        private KdbxDocument document;
        private IResourceProvider resourceProvider;
        private IRandomNumberGenerator rng;
        private IDatabaseNavigationViewModel navigationViewModel;
        private IAppSettingsService settingsService;
        private ISensitiveClipboardService clipboardService;

        private DateTime? suspendTime;
        private ISyncContext syncContext;
        private ITimer idleTimer;
        private double idleSecondsUntilLock;

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="syncContext">Context to use for marshalling to the UI thread.</param>
        /// <param name="timerFactory">Used to create a timer.</param>
        /// <param name="file">The file on disk represented by this database.</param>
        /// <param name="fileIsSample">Whether this file is a sample file.</param>
        /// <param name="document">The decrypted database.</param>
        /// <param name="resourceProvider">A IResourceProvider for the View.</param>
        /// <param name="rng">A random number generator used to protect strings.</param>
        /// <param name="navigationViewModel">A ViewModel representing the navigation of the database.</param>
        /// <param name="persistenceService">A service used to save the database.</param>
        /// <param name="settingsService">A service used to access app settings.</param>
        /// <param name="clipboardService">A service used to access the clipboard for credentials.</param>
        public DatabaseParentViewModel(
            ISyncContext syncContext,
            ITimerFactory timerFactory,
            ITestableFile file,
            bool fileIsSample,
            KdbxDocument document,
            IResourceProvider resourceProvider,
            IRandomNumberGenerator rng,
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            IAppSettingsService settingsService,
            ISensitiveClipboardService clipboardService
            ) : base(document, persistenceService)
        {
            if (timerFactory == null)
            {
                throw new ArgumentNullException(nameof(timerFactory));
            }

            this.syncContext = syncContext ?? throw new ArgumentNullException(nameof(syncContext));
            this.idleTimer = timerFactory.Assemble(TimeSpan.FromSeconds(1));

            this.file = file ?? throw new ArgumentNullException(nameof(file));
            this.fileIsSample = fileIsSample;
            this.document = document ?? throw new ArgumentNullException(nameof(document));
            this.resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
            this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
            this.navigationViewModel = navigationViewModel ?? throw new ArgumentNullException(nameof(navigationViewModel));
            this.settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            this.clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        }

        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();
            this.idleTimer.Tick += IdleTimer_Tick;
            this.settingsService.PropertyChanged += SettingsServicePropertyChanged;
            ResetIdleTimer();
        }

        public override async Task SuspendAsync()
        {
            await base.SuspendAsync();
            this.idleTimer.Tick -= IdleTimer_Tick;
            this.settingsService.PropertyChanged -= SettingsServicePropertyChanged;
        }

        /// <summary>
        /// Records the suspension time to recalculate timeouts later.
        /// </summary>
        public override void HandleAppSuspend()
        {
            this.suspendTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Adjusts the idle period timeout based on how long we were suspended.
        /// </summary>
        public override void HandleAppResume()
        {
            if (this.suspendTime == null || !this.settingsService.EnableLockTimer)
            {
                return;
            }

            TimeSpan timeSuspended = DateTime.UtcNow.Subtract(this.suspendTime.Value);
            this.suspendTime = null;

            this.idleSecondsUntilLock -= timeSuspended.TotalSeconds;
            CheckIdleTimer();
        }

        /// <summary>
        /// Invoked when the View should lock the workspace.
        /// </summary>
        public event EventHandler LockRequested;

        /// <summary>
        /// Invoked when the View should show database settings.
        /// </summary>
        public event EventHandler<SettingsRequestedEventArgs> SettingsRequested;

        private void FireLockRequested()
        {
            LockRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The file on disk represented by this database.
        /// </summary>
        public ITestableFile File
        {
            get { return this.file; }
            private set { this.file = value; }
        }

        /// <summary>
        /// Whether <see cref="File"/> is a sample database.
        /// </summary>
        public bool FileIsSample
        {
            get { return this.fileIsSample; }
            private set { this.fileIsSample = value; }
        }

        /// <summary>
        /// The database that has been decrypted.
        /// </summary>
        public KdbxDocument Document
        {
            get { return this.document; }
            set { SetProperty(ref this.document, value); }
        }

        /// <summary>
        /// Gets the ViewModel representing the current nav state of the database.
        /// </summary>
        public IDatabaseNavigationViewModel NavigationViewModel
        {
            get { return this.navigationViewModel; }
            set { SetProperty(ref this.navigationViewModel, value); }
        }

        /// <summary>
        /// Generates an <see cref="IDatabaseViewModel"/> based on current state.
        /// </summary>
        /// <returns>A ViewModel over the database tree.</returns>
        public IDatabaseViewModel GetDatabaseViewModel()
        {
            return new DatabaseViewModel(
                this.document,
                this.resourceProvider,
                this.rng.Clone(),
                NavigationViewModel,
                PersistenceService,
                this.settingsService,
                this.clipboardService
                );
        }
        
        /// <summary>
        /// Called to manually lock the workspace.
        /// </summary>
        public void TryLock()
        {
            FireLockRequested();
        }

        /// <summary>
        /// Called to manually request database settings.
        /// </summary>
        public void RequestSettings()
        {
            IDatabaseSettingsViewModel settingsViewModel;
            throw new NotImplementedException();
            //settingsViewModel.
            //SettingsRequested?.Invoke(this, new SettingsRequestedEventArgs(settingsViewModel));
        }

        /// <summary>
        /// Resets the idle timer as appropriate.
        /// </summary>
        public void HandleInteractivity()
        {
            ResetIdleTimer();
        }

        /// <summary>
        /// Stops the idle timer if necessary, or resets the value to a new high.
        /// </summary>
        private void ResetIdleTimer()
        {
            if (!this.settingsService.EnableLockTimer)
            {
                this.idleTimer.Stop();
            }
            else
            {
                this.idleSecondsUntilLock = this.settingsService.LockTimer;
                this.idleTimer.Start();
            }
        }

        /// <summary>
        /// Handles settings changes to synchronize the lock timer with user preferences.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LockTimer")
            {
                // Clamp the timer to a lower value if necessary
                if (this.idleSecondsUntilLock > this.settingsService.LockTimer)
                {
                    this.idleSecondsUntilLock = (double)this.settingsService.LockTimer;
                }
            }
            else if (e.PropertyName == "EnableLockTimer")
            {
                ResetIdleTimer();
            }
        }

        /// <summary>
        /// Handles ticks of the idle timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IdleTimer_Tick(object sender, object e)
        {
            this.idleSecondsUntilLock -= this.idleTimer.Interval.TotalSeconds;
            CheckIdleTimer();

            DebugHelper.Trace($"Idle time remaining: {this.idleSecondsUntilLock}");
        }

        /// <summary>
        /// Handles checking expiration of the idle timer.
        /// </summary>
        private void CheckIdleTimer()
        {
            if (this.idleSecondsUntilLock <= 0)
            {
                this.idleSecondsUntilLock = 0;
                this.idleTimer.Stop();
                this.syncContext.Post(TryLock);
            }
        }
    }
}
