using System;
using System.Collections.Generic;
using System.Diagnostics;
using PassKeep.Common;
using PassKeep.ViewModels;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using PassKeep.Controls;
using PassKeep.KeePassLib;
using Windows.ApplicationModel;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class DatabaseUnlockView : DatabaseUnlockViewBase
    {
        public override bool IsProtected
        {
            get { return false; }
        }

        public DatabaseUnlockView()
        {
            this.InitializeComponent();
        }

        private void StartedUnlockHandler(object sender, CancelableEventArgs e)
        {
            onStartedLoading("Decrypting...", e.Cancel);
        }

        private void DoneUnlockHandler(object sender, EventArgs e)
        {
            onDoneLoading();
        }

        private async void DocumentReadyHandler(object sender, DocumentReadyEventArgs e)
        {
            if (ViewModel.Settings.AutoLoadEnabled && !ViewModel.IsSample)
            {
                StorageApplicationPermissions.MostRecentlyUsedList.AddOrReplace(ConfigurationViewModel.DatabaseToken, ViewModel.File);
            }
            /*if (ViewModel.Settings.EnableBackup && !ViewModel.IsSample)
            {
                onStartedLoading("Creating backup...", () => { });
                await ViewModel.File.CopyAsync(Package.Current.InstalledLocation, string.Format("{0}-Backup.kdbx", ViewModel.File.DisplayName), NameCollisionOption.ReplaceExisting);
                onDoneLoading();
            }*/

            DatabaseViewModel newViewModel = new DatabaseViewModel(ViewModel.Settings, ViewModel.Reader.GetWriter(), ViewModel.File, e.Rng, ViewModel.IsSample);
            onStartedLoading("Loading database...", () => { });
            await newViewModel.BuildTree();

            if (string.IsNullOrWhiteSpace(newViewModel.Document.Metadata.HeaderHash) ||
                newViewModel.Document.Metadata.HeaderHash == ViewModel.Reader.HeaderHash)
            {
                Navigator.ReplacePage(typeof(DatabaseView), newViewModel);
            }
            else
            {
                ViewModel.Error = new KeePassError(KdbxParseError.BadHeaderHash);
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            base.LoadState(navigationParameter, pageState);

            ViewModel.StartedUnlock += StartedUnlockHandler;
            ViewModel.DoneUnlock += DoneUnlockHandler;
            ViewModel.DocumentReady += DocumentReadyHandler;

            onStartedLoading("Reading header...", () => { });
            ViewModel.ValidateFile();
            onDoneLoading();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.StartedUnlock -= StartedUnlockHandler;
            ViewModel.DoneUnlock -= DoneUnlockHandler;
            ViewModel.DocumentReady -= DocumentReadyHandler;

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            // Intentionally left blank
        }

        private void passwordInput_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Bug in FxCop with ButtonBase.Command getter (CA0001)
                dynamic unlocker = unlockButton;

                if (unlocker.Command.CanExecute(null))
                {
                    Debug.WriteLine("View got {ENTER}, attempting file unlock...");
                    unlocker.Command.Execute(null);
                }
                else
                {
                    Debug.WriteLine("View got {ENTER}, but file is mot unlockable.");
                }
            }
        }

        private async void database_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.Value == ApplicationViewState.Snapped && !ApplicationView.TryUnsnap())
            {
                return;
            }

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

            Navigator.Navigate(typeof(DatabaseUnlockView), new DatabaseUnlockViewModel(ViewModel.Settings, pickedKdbx));
        }

        private async void keyfile_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicationView.Value == ApplicationViewState.Snapped && !ApplicationView.TryUnsnap())
            {
                return;
            }

            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add("*");

            StorageFile pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile == null)
            {
                return;
            }

            tbKeyfile.Text = pickedFile.Name;
            ViewModel.Keyfile = pickedFile;
        }
    }
}
