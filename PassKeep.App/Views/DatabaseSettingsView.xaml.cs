using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Providers;
using PassKeep.ViewBases;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Views
{
    public sealed partial class DatabaseSettingsView : DatabaseSettingsViewBase
    {
        private readonly InMemoryDatabaseSettingsProvider backupSettings;
        private bool changesSaved;

        public DatabaseSettingsView()
            : base()
        {
            InitializeComponent();
            this.changesSaved = false;
            this.backupSettings = new InMemoryDatabaseSettingsProvider();
        }

        /// <summary>
        /// Backs up the original database settings so that we restore them if not committed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.backupSettings.Cipher = ViewModel.Cipher;
            this.backupSettings.KdfParameters = ViewModel.GetKdfParameters().Reseed();
        }

        /// <summary>
        /// Restores original database settings if the user did not choose to commit them.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (!this.changesSaved)
            {
                // Restore original settings
                ViewModel.Cipher = this.backupSettings.Cipher;
                ViewModel.SetKdfParameters(this.backupSettings.KdfParameters);
            }
        }

        /// <summary>
        /// Provides a means of the parent page requesting a navigate from a clicked breadcrumb to the specified group.
        /// XXX: This is a duplicate of the behavior found in NodeDetailsView and should probably just be made default behavior.
        /// </summary>
        /// <remarks>
        /// This allows the page to preempt the navigate or do necessary cleanup.
        /// </remarks>
        /// <param name="dbViewModel">The DatabaseViewModel to use for the navigation.</param>
        /// <param name="navViewModel">The NavigationViewModel to update.</param>
        /// <param name="clickedGroup">The group to navigate to.</param>
        public override Task RequestBreadcrumbNavigation(IDatabaseViewModel dbViewModel, IDatabaseNavigationViewModel navViewModel, IKeePassGroup clickedGroup)
        {
            void navCallback()
            {
                navViewModel.SetGroup(clickedGroup);
            }

            Frame.Navigate(typeof(DatabaseView), new CancellableNavigationParameter(navCallback, dbViewModel));
            return Task.FromResult<object>(null);
        }
    }
}
