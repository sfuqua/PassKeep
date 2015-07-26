using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using System;
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

        /// <summary>
        /// Initializes the instance.
        /// </summary>
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

            this.file = file;
            this.fileIsSample = fileIsSample;
            this.document = document;
            this.resourceLoader = resourceLoader;
            this.rng = rng;
            this.navigationViewModel = navigationViewModel;
            this.settingsService = settingsService;
            this.clipboardService = clipboardService;
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
    }
}
