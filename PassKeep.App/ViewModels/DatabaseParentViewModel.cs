using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using System;
using Windows.ApplicationModel.Resources;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel that tracks database state and can generate ViewModels for child views.
    /// </summary>
    public sealed class DatabaseParentViewModel : DatabasePersistenceViewModel, IDatabaseParentViewModel
    {
        private KdbxDocument document;
        private ResourceLoader resourceLoader;
        private IRandomNumberGenerator rng;
        private IDatabaseNavigationViewModel navigationViewModel;
        private IAppSettingsService settingsService;

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="document">The decrypted database.</param>
        /// <param name="resourceLoader">A ResourceLoader for the View.</param>
        /// <param name="rng">A random number generator used to protect strings.</param>
        /// <param name="navigationViewModel">A ViewModel representing the navigation of the database.</param>
        /// <param name="persistenceService">A service used to save the database.</param>
        /// <param name="settingsService">A service used to access app settings.</param>
        public DatabaseParentViewModel(
            KdbxDocument document,
            ResourceLoader resourceLoader,
            IRandomNumberGenerator rng,
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService persistenceService,
            IAppSettingsService settingsService
            ) : base(document, persistenceService)
        {
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

            this.document = document;
            this.resourceLoader = resourceLoader;
            this.rng = rng;
            this.navigationViewModel = navigationViewModel;
            this.settingsService = settingsService;
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
                this.settingsService
                );
        }
    }
}
