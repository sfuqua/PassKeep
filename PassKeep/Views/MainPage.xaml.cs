﻿using System;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PassKeep.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : MainViewBase
    {
        private AppSettingsControl activeSettingsPanel = null;
        private ThreadPoolTimer lockTimer = null;
        private Action loadingCancel;

        public override bool IsProtected
        {
            get { return false; }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void wireActivityEvents(UIElement element)
        {
            element.AddHandler(PointerPressedEvent, new PointerEventHandler(pointerEventHandlerDummy), true);
            element.AddHandler(PointerMovedEvent, new PointerEventHandler(pointerEventHandlerDummy), true);
            element.AddHandler(KeyDownEvent, new KeyEventHandler(keyEventHandlerDummy), true);
        }

        private void pointerEventHandlerDummy(object sender, PointerRoutedEventArgs e)
        {
            resetLockTimer();
        }

        private void keyEventHandlerDummy(object sender, KeyRoutedEventArgs e)
        {
            resetLockTimer();
        }

        void CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(
                new SettingsCommand("Configuration", "Configuration",
                    new UICommandInvokedHandler(
                        cmd => { initAppSettingsControl(SettingsPanel.Configuration); }
                    )
            ));

            args.Request.ApplicationCommands.Add(
                new SettingsCommand("Help", "Help",
                    new UICommandInvokedHandler(
                        cmd => { contentFrame.Navigate(typeof(WelcomeView), new BasicViewModel(ViewModel.Settings)); }
                    )
            ));

            args.Request.ApplicationCommands.Add(
                new SettingsCommand("Feedback", "Ideas and bugs",
                    new UICommandInvokedHandler(
                        async cmd => { await Launcher.LaunchUriAsync(new Uri("mailto:passkeep@outlook.com")); }
                    )
            ));
        }

        private enum SettingsPanel
        {
            Configuration
        }

        private void initAppSettingsControl(SettingsPanel control)
        {
            killAppSettingsControl();

            switch(control)
            {
                case SettingsPanel.Configuration:
                    activeSettingsPanel = new ConfigurationPanel(ViewModel.Settings);
                    break;
                default:
                    Debug.Assert(false);
                    throw new ArgumentException("Invalid enum value", "control");
            }

            activeSettingsPanel.BackPressed += (s, e) =>
                {
                    SettingsPane.Show();
                    killAppSettingsControl();
                };

            activeSettingsPanel.Dismissed += (s, e) =>
                {
                    killAppSettingsControl();
                    SearchPane.GetForCurrentView().ShowOnKeyboardInput = ((PassKeepPage)contentFrame.Content).SearchOnType;
                };

            wireActivityEvents(activeSettingsPanel);
            layoutRoot.Children.Add(activeSettingsPanel);
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = false;
        }

        private void killAppSettingsControl()
        {
            if (activeSettingsPanel != null)
            {
                layoutRoot.Children.Remove(activeSettingsPanel);
                activeSettingsPanel = null;
            }
        }

        private void StartLoadingHandler(object sender, LoadingStartedEventArgs e)
        {
            LoadingGrid.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            LoadingText.Text = e.Text;
            loadingCancel = e.Cancel;

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
            loadingCancel = null;

            contentFrame.IsEnabled = true;
        }

        protected override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            base.navHelper_LoadState(sender, e);

            SettingsPane.GetForCurrentView().CommandsRequested += CommandsRequested;
            btnOpenSample.DataContext = ViewModel.Settings;

            List<UIElement> elements = new List<UIElement> { this, BottomAppBar };
            foreach (UIElement element in elements)
            {
                wireActivityEvents(element);
            }

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

        protected void handleSettingChange(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LockTimer")
            {
                resetLockTimer();
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.Settings.PropertyChanged += handleSettingChange;

            switch (ViewModel.Mode)
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

        public void Search(EntrySearchViewModel viewModel)
        {
            DatabaseView dbView = contentFrame.Content as DatabaseView;
            if (dbView != null)
            {
                viewModel.DatabaseViewModel = dbView.ViewModel;
                contentFrame.Navigate(typeof(EntrySearchView), viewModel);
                return;
            }

            EntryDetailsView entryView = contentFrame.Content as EntryDetailsView;
            GroupDetailsView groupView = contentFrame.Content as GroupDetailsView;
            if (entryView != null)
            {
                viewModel.DatabaseViewModel = entryView.ViewModel.DatabaseViewModel;
                contentFrame.Navigate(typeof(EntrySearchView), viewModel);
                return;
            }
            else if (groupView != null)
            {
                viewModel.DatabaseViewModel = groupView.ViewModel.DatabaseViewModel;
                contentFrame.Navigate(typeof(EntrySearchView), viewModel);
                return;
            }
            else
            {
                EntrySearchView esView = contentFrame.Content as EntrySearchView;
                if (esView != null)
                {
                    viewModel.DatabaseViewModel = esView.ViewModel.DatabaseViewModel;
                    contentFrame.ReplacePage(typeof(EntrySearchView), viewModel);
                }
                else
                {
                    contentFrame.Navigate(typeof(EntrySearchView), viewModel);
                }
            }
        }

        public void OpenFile(FileOpenViewModel viewModel)
        {
            contentFrame.Navigate(typeof(DatabaseUnlockView), new DatabaseUnlockViewModel(ViewModel.Settings, viewModel.File));
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

            contentFrame.Navigate(typeof(DatabaseUnlockView), new DatabaseUnlockViewModel(ViewModel.Settings, pickedKdbx));
        }

        private async void OpenSample_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder installFolder = Package.Current.InstalledLocation;
            StorageFolder subFolder = await installFolder.GetFolderAsync("Assets");
            StorageFile sample = await subFolder.GetFileAsync("SampleDatabase.kdbx");

            contentFrame.Navigate(typeof(DatabaseUnlockView), new DatabaseUnlockViewModel(ViewModel.Settings, sample, true));
        }

        private async void Lock_Click(object sender, RoutedEventArgs e)
        {
            await doLock();
        }

        private async Task<bool> doLock()
        {
            PassKeepPage currentPage = contentFrame.Content as PassKeepPage;
            Debug.Assert(currentPage != null);
            return await currentPage.Lock();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (loadingCancel != null)
            {
                loadingCancel();
            }
        }

        private void resetLockTimer()
        {
            if (lockTimer != null)
            {
                lockTimer.Cancel();
            }

            if (ViewModel.Settings.EnableLockTimer)
            {
                lockTimer = ThreadPoolTimer.CreateTimer(lockTimerExpired, TimeSpan.FromSeconds(ViewModel.Settings.LockTimer));
            }
        }

        private async void lockTimerExpired(ThreadPoolTimer timer)
        {
            if (!ViewModel.Settings.EnableLockTimer)
            {
                return;
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
            {
                PassKeepPage currentPage = contentFrame.Content as PassKeepPage;
                Debug.Assert(currentPage != null);
                Debug.WriteLine("Lock timer expired. Current page is protected: {0}", currentPage.IsProtected);
                if (currentPage.IsProtected)
                {
                    currentPage.Lock();
                }
            }));
        }

        private async void KeyDownHandler(object sender, KeyEventArgs e)
        {
            var ctrlState = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if ((ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            {
                switch(e.VirtualKey)
                {
                    case VirtualKey.L:
                        e.Handled = await doLock();
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
