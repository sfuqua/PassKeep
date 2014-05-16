using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using PassKeep.Common;
using PassKeep.Controls;
using PassKeep.ViewModels;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Search;
using System.ComponentModel;
using PassKeep.Views.Bases;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using Microsoft.Practices.Unity;

namespace PassKeep.Views
{
    /// <summary>
    /// Serves as an anchor page in typical use of the application.
    /// The page contains a Frame which handles navigation for the rest of the
    /// app.
    /// </summary>
    public sealed partial class RootView : RootViewBase
    {
        private AppSettingsControl activeSettingsPanel = null;
        private ThreadPoolTimer lockTimer = null;
        private Action loadCancelAction;

        public RootView()
        {
            this.InitializeComponent();
            contentFrame.Navigated += contentFrame_Navigated;
        }

        private void contentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Debug.Assert(contentFrame.Content is PassKeepPage);
            ((PassKeepPage)contentFrame.Content).ContainerHelper = ContainerHelper;
        }

        void CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(
                new SettingsCommand("Configuration", "Configuration",
                    new UICommandInvokedHandler(
                        cmd =>
                        {
                            ConfigurationFlyout flyout = new ConfigurationFlyout();
                            flyout.Show();
                        }
                    )
            ));

            args.Request.ApplicationCommands.Add(
                new SettingsCommand("Help", "Help",
                    new UICommandInvokedHandler(
                        cmd =>
                        {
                            ContainerHelper.ResolveAndNavigate<HelpView, IHelpViewModel>(contentFrame);
                        }
                    )
            ));

            args.Request.ApplicationCommands.Add(
                new SettingsCommand("Feedback", "Ideas and bugs",
                    new UICommandInvokedHandler(
                        async cmd =>
                        {
                            await Launcher.LaunchUriAsync(new Uri("mailto:passkeep@outlook.com"));
                        }
                    )
            ));
        }

        private void StartLoadingHandler(object sender, LoadingStartedEventArgs e)
        {
            LoadingGrid.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            LoadingText.Text = e.Text;
            loadCancelAction = e.Cancel;

            if (e.Indeterminate)
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                LoadingRing.Visibility = Visibility.Visible;
                LoadingRing.IsActive = true;
            }
            else
            {
                LoadingRing.Visibility = Visibility.Collapsed;
                LoadingBar.Value = 0;
                LoadingBar.Visibility = Visibility.Visible;
            }

            contentFrame.IsEnabled = false;
        }

        private void DoneLoadingHandler(object sender, EventArgs e)
        {
            LoadingBar.Value = 1;
            LoadingBar.Visibility = Visibility.Collapsed;
            LoadingRing.Visibility = Visibility.Collapsed;
            LoadingGrid.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
            loadCancelAction = null;

            contentFrame.IsEnabled = true;
        }

        protected override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            base.navHelper_LoadState(sender, e);

            SettingsPane.GetForCurrentView().CommandsRequested += CommandsRequested;

            contentFrame.Navigating += (s, evt) =>
            {
                PassKeepPage previousPage = contentFrame.Content as PassKeepPage;
                if (previousPage == null)
                {
                    return;
                }

                previousPage.StartedLoading -= StartLoadingHandler;
                previousPage.DoneLoading -= DoneLoadingHandler;
            };

            contentFrame.Navigated += (s, evt) =>
            {
                DoneLoadingHandler(null, new EventArgs());

                PassKeepPage currentPage = contentFrame.Content as PassKeepPage;
                Debug.Assert(currentPage != null);

                currentPage.BottomAppBar = BottomAppBar;
                currentPage.LeftCommands = leftCommands;
                currentPage.RightCommands = rightCommands;

                currentPage.StartedLoading += StartLoadingHandler;
                currentPage.DoneLoading += DoneLoadingHandler;
                btnLock.Visibility = (currentPage.IsProtected ? Visibility.Visible : Visibility.Collapsed);
            };

            Window.Current.CoreWindow.KeyDown += KeyDownHandler;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            switch (ViewModel.ActivationMode)
            {
                case ActivationMode.Regular:
                    if (ViewModel.Settings.AutoLoadEnabled)
                    {
                        var list = StorageApplicationPermissions.MostRecentlyUsedList;
                        if (list.ContainsItem(ConfigurationViewModel.DatabaseToken))
                        {
                            try
                            {
                                var file = await list.GetFileAsync(ConfigurationViewModel.DatabaseToken);
                                contentFrame.Navigate(typeof(DatabaseUnlockView), new DatabaseUnlockViewModel(ViewModel.Settings, file));
                                return;
                            }
                            catch (FileNotFoundException)
                            { }
                        }
                    }

                    contentFrame.Navigate(typeof(WelcomeView), new BasicViewModel(ViewModel.Settings));
                    break;

                case ActivationMode.Search:
                    Debug.Assert(ViewModel.SearchViewModel != null);
                    Search(ViewModel.SearchViewModel);
                    break;

                case ActivationMode.File:
                    Debug.Assert(ViewModel.FileOpenViewModel != null);
                    OpenFile(ViewModel.FileOpenViewModel);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        public void Search(ISearchViewModel searchViewModel)
        {
            DatabaseView dbView = contentFrame.Content as DatabaseView;
            if (dbView != null)
            {
                searchViewModel.DatabaseViewModel = dbView.ViewModel;
                contentFrame.Navigate(typeof(EntrySearchView), searchViewModel);
                return;
            }

            EntryDetailsView entryView = contentFrame.Content as EntryDetailsView;
            GroupDetailsView groupView = contentFrame.Content as GroupDetailsView;
            if (entryView != null)
            {
                searchViewModel.DatabaseViewModel = entryView.ViewModel.DatabaseViewModel;
                contentFrame.Navigate(typeof(EntrySearchView), searchViewModel);
                return;
            }
            else if (groupView != null)
            {
                searchViewModel.DatabaseViewModel = groupView.ViewModel.DatabaseViewModel;
                contentFrame.Navigate(typeof(EntrySearchView), searchViewModel);
                return;
            }
            else
            {
                EntrySearchView searchView = contentFrame.Content as EntrySearchView;
                if (searchView != null)
                {
                    searchViewModel.DatabaseViewModel = searchView.ViewModel.DatabaseViewModel;
                    contentFrame.ReplacePage(typeof(EntrySearchView), searchViewModel);
                }
                else
                {
                    contentFrame.Navigate(typeof(EntrySearchView), searchViewModel);
                }
            }
        }

        public void OpenFile(StorageFile file, bool isSampleFile = false)
        {
            ContainerHelper.ResolveAndNavigate<DatabaseUnlockView, IDatabaseUnlockViewModel>(
                contentFrame,
                new ParameterOverride("file", file),
                new ParameterOverride("isSampleFile", isSampleFile)
            );
        }

        private async void OpenDatabase_Click(object sender, RoutedEventArgs e)
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

            OpenFile(pickedKdbx);
        }

        private async void OpenSample_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder installFolder = Package.Current.InstalledLocation;
            StorageFolder subFolder = await installFolder.GetFolderAsync("Assets");
            StorageFile sample = await subFolder.GetFileAsync("SampleDatabase.kdbx");

            OpenFile(sample, true);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (loadCancelAction != null)
            {
                loadCancelAction();
            }
        }

        private async void KeyDownHandler(object sender, KeyEventArgs e)
        {
            var ctrlState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if ((ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            {
                switch(e.VirtualKey)
                {
                    case VirtualKey.L:
                        //e.Handled = await doLock();
                        break;
                    case VirtualKey.O:
                        OpenDatabase_Click(sender, new RoutedEventArgs());
                        e.Handled = true;
                        break;
                    default:
                        e.Handled = await ((PassKeepPage)contentFrame.Content).HandleHotKey(e.VirtualKey);
                        break;
                }
            }
            else if (e.VirtualKey == VirtualKey.Escape)
            {
                Cancel_Click(null, new RoutedEventArgs());
            }
            else if (e.VirtualKey == VirtualKey.Delete)
            {
                e.Handled = await ((PassKeepPage)contentFrame.Content).HandleDelete();
            }
            else
            {
                ((PassKeepPage)contentFrame.Content).HandleGenericKey(e.VirtualKey);
            }
        }
    }
}
