using Microsoft.Practices.Unity;
using PassKeep.Common;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.Services;
using PassKeep.ViewModels;
using PassKeep.Views.Bases;
using System;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace PassKeep.Views
{
    /// <summary>
    /// A View of an unlockable document defaultSaveLocation.
    /// </summary>
    public sealed partial class DatabaseUnlockView : DatabaseUnlockViewBase
    {
        public DatabaseUnlockView()
        {
            this.InitializeComponent();
            // TODO: Dynamic layout for this View
        }

        #region Navigation helpers

        /// <summary>
        /// Called when the NavigationHelper needs to load the state of the View.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            base.navHelper_LoadState(sender, e);
            // TODO: Currently, header validation is automatic. Is this some sort of DOS opportunity?
            // Should this be cancelable?
        }

        #endregion

        #region View event handlers

        /// <summary>
        /// Allows the user to change the document defaultSaveLocation they are interested in opening.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void database_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add("*");

            StorageFile pickedKdbx = await picker.PickSingleFileAsync();
            if (pickedKdbx == null)
            {
                return;
            }

            this.ViewModel.CandidateFile = pickedKdbx;
        }

        /// <summary>
        /// Allows the user to change the keyfile they are interested in using as a credential.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void keyfile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add("*");

            StorageFile pickedFile = await picker.PickSingleFileAsync();

            tbKeyfile.Text = (pickedFile != null ? pickedFile.Name : String.Empty);
            ViewModel.KeyFile = pickedFile;
        }

        #endregion

        #region ViewModel event handlers

        /// <summary>
        /// EventHandler for when the ViewModel indicates it has started a document decryption.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">CancellableEventArgs for the unlock event.</param>
        private void StartedUnlockingHandler(object sender, CancellableEventArgs e)
        {
            RaiseStartedLoading("Decrypting...", e.Cancel);
        }

        /// <summary>
        /// EventHandler for when the ViewModel indicates it has finished a decryption attempt.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">EventArgs for the event.</param>
        private void StoppedUnlockingHandler(object sender, EventArgs e)
        {
            RaiseDoneLoading();
        }

        /// <summary>
        /// EventHandler for when the ViewModel indicates it has successfully decrypted a document.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">DocumentReadyEventArgs for the successful decryption.</param>
        private void DocumentReadyHandler(object sender, DocumentReadyEventArgs e)
        {
            IDatabaseUnlockViewModel viewModel = (IDatabaseUnlockViewModel)sender;

            // On successful unlock, add this document to the MRU access list if the settings permit it.
            if (this.SettingsService.AutoLoadEnabled && !viewModel.IsSampleFile)
            {
                StorageApplicationPermissions.MostRecentlyUsedList.AddOrReplace(ConfigurationViewModel.DatabaseToken, viewModel.CandidateFile);
            }

            // We always want to override the "document" parameter.
            // If the ViewModel represents a sample file, we also want to override the persistence service dependency
            // to be a dummy persistence service that does nothing (to avoid stomping the sample file with different data).
            ResolverOverride databaseParameter = new ParameterOverride("database", e.Document);
            ResolverOverride[] overrides = (viewModel.IsSampleFile ?
                new ResolverOverride[] {
                    databaseParameter,
                    new DependencyOverride<IDatabasePersistenceService>(new DummyPersistenceService())
                } :
                new ResolverOverride[] {
                    databaseParameter
                }
            );

            // TODO: Evaluate whether this should be a standard navigate (allowing going back to the 'Locked' page),
            // or some sort of in-place replacemen.
            this.ContainerHelper.ResolveAndNavigate<DatabaseView, IDatabaseViewModel>(
                this.Frame,
                overrides
            );
        }

        #endregion
    }
}
