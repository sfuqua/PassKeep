using Microsoft.Practices.Unity;
using PassKeep.Common;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
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
    /// A View of an unlockable database file.
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
        /// Allows the user to change the database file they are interested in opening.
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
        /// EventHandler for when the ViewModel indicates it has started a database decryption.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">CancelableEventArgs for the unlock event.</param>
        private void StartedUnlockingHandler(object sender, CancelableEventArgs e)
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

            // On successful unlock, add this database to the MRU access list if the settings permit it.
            if (this.SettingsService.AutoLoadEnabled && !viewModel.IsSampleFile)
            {
                StorageApplicationPermissions.MostRecentlyUsedList.AddOrReplace(ConfigurationViewModel.DatabaseToken, viewModel.CandidateFile);
            }

            // TODO: Evaluate whether this should be a standard navigate (allowing going back to the 'Locked' page),
            // or some sort of in-place replacemen.
            this.ContainerHelper.ResolveAndNavigate<DatabaseView, IDatabaseViewModel>(
                this.Frame,
                new ParameterOverride("database", e.Document)
            );
        }

        #endregion
    }
}
