using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that tracks database state and can generate ViewModels for child views.
    /// </summary>
    public sealed class DatabaseParentViewModel : DatabasePersistenceViewModel, IDatabaseParentViewModel
    {
        private IStorageFile file;
        private bool fileIsSample;
        private KdbxDocument document;
        private ResourceLoader resourceLoader;
        private IRandomNumberGenerator rng;
        private IDatabaseNavigationViewModel navigationViewModel;
        private IAppSettingsService settingsService;
        private ISensitiveClipboardService clipboardService;

        private ITimer idleTimer;
        private double idleSecondsUntilLock;

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="idleTimer">Timer used to track user activity to lock the database.</param>
        /// <param name="file">The file on disk represented by this database.</param>
        /// <param name="fileIsSample">Whether this file is a sample file.</param>
        /// <param name="document">The decrypted database.</param>
        /// <param name="resourceLoader">A ResourceLoader for the View.</param>
        /// <param name="rng">A random number generator used to protect strings.</param>
        /// <param name="navigationViewModel">A ViewModel representing the navigation of the database.</param>
        /// <param name="persistenceService">A service used to save the database.</param>
        /// <param name="settingsService">A service used to access app settings.</param>
        /// <param name="clipboardService">A service used to access the clipboard for credentials.</param>
        public DatabaseParentViewModel(
            ITimer idleTimer,
            IStorageFile file,
            bool fileIsSample,
            KdbxDocument document,
            ResourceLoader resourceLoader,
            IRandomNumberGenerator rng,
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            IAppSettingsService settingsService,
            ISensitiveClipboardService clipboardService
            ) : base(document, persistenceService)
        {
            if (idleTimer == null)
            {
                throw new ArgumentNullException(nameof(idleTimer));
            }

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (resourceLoader == null)
            {
                throw new ArgumentNullException(nameof(resourceLoader));
            }

            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            if (navigationViewModel == null)
            {
                throw new ArgumentNullException(nameof(navigationViewModel));
            }

            if (settingsService == null)
            {
                throw new ArgumentNullException(nameof(settingsService));
            }

            if (clipboardService == null)
            {
                throw new ArgumentNullException(nameof(clipboardService));
            }

            this.idleTimer = idleTimer;
            this.file = file;
            this.fileIsSample = fileIsSample;
            this.document = document;
            this.resourceLoader = resourceLoader;
            this.rng = rng;
            this.navigationViewModel = navigationViewModel;
            this.settingsService = settingsService;
            this.clipboardService = clipboardService;
        }

        public override void Activate()
        {
            base.Activate();
            this.idleTimer.Tick += IdleTimer_Tick;
            this.settingsService.PropertyChanged += SettingsServicePropertyChanged;
            ResetIdleTimer();
        }

        public override void Suspend()
        {
            base.Suspend();
            this.idleTimer.Tick -= IdleTimer_Tick;
            this.settingsService.PropertyChanged -= SettingsServicePropertyChanged;
        }

        /// <summary>
        /// Invoked when the View should lock the workspace.
        /// </summary>
        public event EventHandler LockRequested;

        private void FireLockRequested()
        {
            LockRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The file on disk represented by this database.
        /// </summary>
        public IStorageFile File
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
                this.resourceLoader,
                this.rng.Clone(),
                this.NavigationViewModel,
                this.PersistenceService,
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

            this.idleSecondsUntilLock = this.settingsService.LockTimer;
            this.idleTimer.Start();
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
            if (this.idleSecondsUntilLock <= 0)
            {
                this.idleSecondsUntilLock = 0;
                TryLock();
            }

            Dbg.Trace($"Idle time remaining: {this.idleSecondsUntilLock}");
        }
    }
}
