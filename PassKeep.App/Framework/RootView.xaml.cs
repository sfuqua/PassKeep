﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Framework.Messages;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.ViewBases;
using PassKeep.Views;
using PassKeep.Views.FlyoutPages;
using PassKeep.Views.Flyouts;
using SariphLib.Files;
using SariphLib.Diagnostics;
using SariphLib.Messaging;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Framework
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RootView : RootViewBase
    {
        public readonly ActionCommand ContentBackCommand;

        private const string FeedbackDescriptionResourceKey = "FeedbackDescription";
        private const string ContactEmailResourceKey = "ContactEmail";

        // The size of the SplitView pane when opened.
        private double splitViewPaneWidth = 0;

        // Whether the last navigation was caused by a SplitView nav button
        private bool splitViewNavigation = false;

        private readonly object splitViewSyncRoot = new object();

        public RootView()
        {
            InitializeComponent();

            this.ContentBackCommand = new ActionCommand(
                () => CanGoBack(),
                () => { GoBack(); }
            );

            MessageBus = new MessageBus();
            BootstrapMessageSubscriptions(
                typeof(DatabaseCandidateMessage),
                typeof(DatabaseOpenedMessage),
                typeof(DatabaseClosedMessage),
                typeof(SavingStateChangeMessage)
            );

            // Handle adjusting the size of the SplitView when IsPaneOpen changes, to cause Flyouts to position properly
            this.splitViewPaneWidth = this.mainSplitView.OpenPaneLength;
            this.mainSplitView.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, OnSplitViewIsPaneOpenChanged);
            OnSplitViewIsPaneOpenChanged(this.mainSplitView, SplitView.IsPaneOpenProperty);
        }

        public override Frame ContentFrame
        {
            get
            {
                return this.contentFrame;
            }
        }

        #region Message handlers

        public Task HandleDatabaseCandidateMessage(DatabaseCandidateMessage message)
        {
            ViewModel.CandidateFile = message.File;
            return Task.FromResult(0);
        }

        public async Task HandleDatabaseOpenedMessage(DatabaseOpenedMessage message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.DecryptedDatabase = message.ViewModel;
            });
        }

        public Task HandleDatabaseClosedMessage(DatabaseClosedMessage message)
        {
            ViewModel.DecryptedDatabase = null;
            return Task.FromResult(0);
        }

        public Task HandleSavingStateChangeMessage(SavingStateChangeMessage message)
        {
            DebugHelper.Trace($"New saving value: {message.IsNowSaving}");
            if (message.IsNowSaving)
            {
                this.savingIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                this.savingIndicator.Visibility = Visibility.Collapsed;
            }

            return Task.FromResult(0);
        }

        #endregion

        #region Auto-event Handlers

        /// <summary>
        /// Handles the case where the ViewModel failed to clear the clipboard (i.e., because it was out of  focus).
        /// </summary>
        /// <param name="sender">The RootViewModel.</param>
        /// <param name="e">Args for the failure event.</param>
        [AutoWire(nameof(IRootViewModel.ClipboardClearFailed))]
        public void ClipboardClearFailedHandler(object sender, EventArgs e)
        {
            // No need to await this call.
#pragma warning disable CS4014
            // In the event of a failure, prompt the user to clear the clipboard manually.
            Dispatcher.RunAsync(
#pragma warning restore CS4014
                CoreDispatcherPriority.Normal,
                async () =>
                {
                    MessageDialog dialog = new MessageDialog(GetString("ClipboardClearError"), GetString("ClearClipboardPrompt"))
                    {
                        Options = MessageDialogOptions.None
                    };

                    IUICommand clearCommand = new UICommand(GetString("ClearClipboardAction"));
                    IUICommand cancelCmd = new UICommand(GetString("Cancel"));

                    dialog.Commands.Add(clearCommand);
                    dialog.Commands.Add(cancelCmd);

                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;

                    IUICommand chosenCmd = await dialog.ShowAsync();
                    if (chosenCmd == clearCommand)
                    {
                        Clipboard.Clear();
                    }
                }
            );
        }

        #endregion

        /// <summary>
        /// Navigates to the proper view for opening a database file.
        /// </summary>
        /// <param name="file">The file being opened.</param>
        /// <param name="isSample">Whether we are unlocking a sample file.</param>
        public async void OpenFile(ITestableFile file, bool isSample = false)
        {
            DebugHelper.Trace("Navigating RootView to Database Unlocker...");
            this.contentFrame.Navigate(typeof(DatabaseUnlockView),
                new NavigationParameter(
                    new {
                        file = await DatabaseCandidateFactory.AssembleAsync(file),
                        isSampleFile = isSample
                    }
                )
            );
        }

        /// <summary>
        /// Handles initialization logic when the RootView is first navigated to.
        /// </summary>
        /// <param name="e">EventArgs for the navigation.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SystemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;

            // Use ActivationMode to decide how to navigate the ContentFrame
            switch(ViewModel.ActivationMode)
            {
                case ActivationMode.Regular:
                    // Load the welcome hub
                    DebugHelper.Trace("Navigating RootView to Dashboard...");
                    this.contentFrame.Navigate(typeof(DashboardView));
                    break;
                case ActivationMode.File:
                    // Load the DatabaseView
                    OpenFile(ViewModel.CandidateFile);
                    break;
                default:
                    throw new NotImplementedException();
            }

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;
        }

        /// <summary>
        /// Handler for SplitView.IsPaneOpen changing - handles resizing the SplitView's OpenPaneLength.
        /// </summary>
        /// <remarks>This size adjustment is necessary so that attached flyouts position properly relative to SplitView nav bar items.</remarks>
        /// <param name="sender">The SplitView.</param>
        /// <param name="prop">The IsPaneOpen property.</param>
        private void OnSplitViewIsPaneOpenChanged(DependencyObject sender, DependencyProperty prop)
        {
            DebugHelper.Assert(sender == this.mainSplitView);
            DebugHelper.Assert(prop == SplitView.IsPaneOpenProperty);

            SplitView splitView = (SplitView)sender;
            if (splitView.IsPaneOpen)
            {
                splitView.OpenPaneLength = this.splitViewPaneWidth;
            }
            else if (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay || splitView.DisplayMode == SplitViewDisplayMode.CompactInline)
            {
                splitView.OpenPaneLength = splitView.CompactPaneLength;
            }
            else if (splitView.DisplayMode == SplitViewDisplayMode.Overlay)
            {
                splitView.OpenPaneLength = 0;
            }
        }

        /// <summary>
        /// Handles navigating when the system (chrome back button, HW back button) requests a back.
        /// </summary>
        /// <param name="sender">Null.</param>
        /// <param name="e">EventArgs for the back request event.</param>
        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.contentFrame != null && CanGoBack())
            {
                GoBack();
                this.ContentBackCommand.RaiseCanExecuteChanged();
                SystemNavigationManager.AppViewBackButtonVisibility =
                    (this.ContentBackCommand.CanExecute(null) ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);

                e.Handled = true;
            }
        }


        /// <summary>
        /// Unhooks event handlers for the page.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            Window.Current.CoreWindow.PointerPressed -= CoreWindow_PointerPressed;
        }

        /// <summary>
        /// Handles KeyDown events for the current window.
        /// </summary>
        /// <param name="sender">The CoreWindow that dispatched the event.</param>
        /// <param name="args">KeyEventArgs for the event.</param>
        private async void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            CoreVirtualKeyStates ctrlState = sender.GetKeyState(VirtualKey.Control);
            if ((ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            {
                CoreVirtualKeyStates shiftState = sender.GetKeyState(VirtualKey.Shift);
                bool shiftDown = (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                // Handle accelerator (Ctrl) hotkeys
                switch (args.VirtualKey)
                {
                    case VirtualKey.O:
                        if (!shiftDown)
                        {
                            // Prompt to open a file
                            args.Handled = true;
                            await PickFileForOpenAsync(
                                file =>
                                {
                                    OpenFile(file);
                                }
                            );
                        }
                        break;
                    default:
                        args.Handled = ((PassKeepPage)this.contentFrame.Content).HandleAcceleratorKey(args.VirtualKey, shiftDown);
                        break;
                }
            }
            else
            {
                if (args.VirtualKey == VirtualKey.Escape)
                {
                    ViewModel.TaskNotificationService.RequestCancellation();
                }
            }
        }

        /// <summary>
        /// Handles pointer events on the window. This allows mouse navigation (forward/back).
        /// </summary>
        /// <param name="sender">The CoreWindow handling the event.</param>
        /// <param name="args">EventArgs for the pointer event.</param>
        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            PointerPointProperties props = args.CurrentPoint.Properties;

            // Ignore chords with standard mouse buttons
            if (props.IsLeftButtonPressed || props.IsRightButtonPressed || props.IsMiddleButtonPressed)
            {
                return;
            }

            // If either MB4 or MB5 is pressed (but not both), navigate as appropriate
            bool backPressed = props.IsXButton1Pressed;
            bool forwardPressed = props.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                // TODO: Forward navigation is not supported
                // Issue #124
                if (backPressed && CanGoBack())
                {
                    DebugHelper.Trace("Navigating back due to mouse button");
                    GoBack();
                }
                else if (forwardPressed) /* && CanGoForward */
                {
                    // Dbg.Trace("Navigating forward due to mouse button");
                    // GoForward();
                }
            }
        }

        #region Declaratively bound event handlers

        /// <summary>
        /// Invoked when the content Frame of the RootView is done navigating.
        /// </summary>
        /// <param name="sender">The content Frame.</param>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (this.splitViewNavigation)
            {
                // For top-level navigates we want to clear the backstack - this is inline with other apps like Music.
                this.contentFrame.BackStack.Clear();
                this.splitViewNavigation = false;
            }
            else
            { 
                SynchronizeNavigationListView();
            }
        }

        #endregion

        /// <summary>
        /// Updates the selected item of the navigation ListView without navigating.
        /// </summary>
        private void SynchronizeNavigationListView()
        {
            if (this.contentFrame.Content is DashboardView || this.contentFrame.Content is DatabaseParentView)
            {
                SetNavigationListViewSelection(this.dashItem);
            }
            else if (this.contentFrame.Content is DatabaseUnlockView || this.contentFrame.Content is DatabaseCreationView)
            {
                SetNavigationListViewSelection(this.openItem);
            }
            /* else if (this.contentFrame.Content is DatabaseParentView)
            {
                Dbg.Assert(this.dbHomeItem.Visibility == Visibility.Visible);
                SetNavigationListViewSelection(this.dbHomeItem);
            } */
            else if (this.contentFrame.Content is HelpView)
            {
                SetNavigationListViewSelection(this.helpItem);
            }
            else if (this.contentFrame.Content is AppSettingsView)
            {
                SetNavigationListViewSelection(this.settingsItem);
            }
            else
            {
                SetNavigationListViewSelection(null);
            }
        }

        /// <summary>
        /// Helper to set the selected value of the SplitView nav list without invoking the event handler.
        /// </summary>
        /// <param name="selection">The value to forcibly select.</param>
        private void SetNavigationListViewSelection(object selection)
        {
            lock(this.splitViewSyncRoot)
            { 
                this.splitViewList.SelectionChanged -= SplitViewList_SelectionChanged;
                this.splitViewList.SelectedItem = selection;
                this.splitViewList.SelectionChanged += SplitViewList_SelectionChanged;
            }
        }

        /// <summary>
        /// Invoked when the user manually opens or closed the SplitView panel.
        /// </summary>
        /// <param name="sender">The hamburger button.</param>
        /// <param name="e">RoutedEventArgs.</param>
        private void SplitViewToggle_Click(object sender, RoutedEventArgs e)
        {
            this.mainSplitView.IsPaneOpen = !this.mainSplitView.IsPaneOpen;
            DebugHelper.Trace($"SplitView.IsPaneOpen has been toggled to new state: {this.mainSplitView.IsPaneOpen}");
        }

        /// <summary>
        /// Invoked whenever the user selects a different SplitView option.
        /// </summary>
        /// <param name="sender">The ListView hosted in the SplitView panel.</param>
        /// <param name="e">EventARgs for the selection change.</param>
        private async void SplitViewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.contentFrame == null)
            {
                return;
            }

            DebugHelper.Assert(e.AddedItems.Count == 1);
            object selection = e.AddedItems[0];
            object deselection = e.RemovedItems.Count == 1 ? e.RemovedItems[0] : null;

            // Helper function for reverting the SelectedItem to the previous value,
            // for buttons that aren't "real" navigates.
            void abortSelection()
            {
                DebugHelper.Assert(deselection != null);
                SetNavigationListViewSelection(deselection);
            }

            if (selection == this.dashItem && deselection != this.dashItem)
            {
                DebugHelper.Trace("Dashboard selected in SplitView.");
                this.splitViewNavigation = true;
                this.contentFrame.Navigate(typeof(DashboardView));

                if (this.mainSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    this.mainSplitView.IsPaneOpen = false;
                }
            }
            else if (selection == this.openItem)
            {
                DebugHelper.Trace("Open selected in SplitView.");
                await PickFileForOpenAsync(
                    /* gotFile */ file =>
                    {
                        OpenFile(file);
                    },
                    /* cancelled */
                    abortSelection
                );

                if (this.mainSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    this.mainSplitView.IsPaneOpen = false;
                }
            }
            else if (selection == this.dbHomeItem)
            {
                DebugHelper.Trace("Database Home selected in SplitView.");
                this.splitViewNavigation = true;
                DebugHelper.Assert(ViewModel.DecryptedDatabase != null, "This button should not be accessible if there is not decrypted database");
                this.contentFrame.Navigate(typeof(DatabaseParentView), ViewModel.DecryptedDatabase);

                if (this.mainSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    this.mainSplitView.IsPaneOpen = false;
                }
            }
            else if (selection == this.passwordItem)
            {
                DebugHelper.Trace("Password Generator selected in SplitView.");
                abortSelection();

                // If we are in super compacted mode (nothing is visible), hide the pane when we open the password flyout.
                // This lets us fit into a phone's view.
                if (this.mainSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    this.mainSplitView.IsPaneOpen = false;
                }

                FlyoutBase.ShowAttachedFlyout(this.passwordItem);
            }
            else if (selection == this.helpItem)
            {
                DebugHelper.Trace("Help selected in SplitView.");
                if(!ShowHelp())
                {
                    abortSelection();
                }

                if (this.mainSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    this.mainSplitView.IsPaneOpen = false;
                }
            }
            else if (selection == this.settingsItem)
            {
                DebugHelper.Trace("Settings selected in SplitView.");
                if (!ShowAppSettings())
                {
                    abortSelection();
                }

                if (this.mainSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    this.mainSplitView.IsPaneOpen = false;
                }
            }
        }

        /// <summary>
        /// Detects whether <see cref="SettingsFlyout"/> is accessible on this platform.
        /// </summary>
        /// <remarks>This wonky detection is necessary because the type exists on phone but does nothing.</remarks>
        /// <returns>Whether <see cref="SettingsFlyout"/> is usable.</returns>
        private bool CanShowSettingsFlyouts()
        {
            return Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ApplicationSettings.SettingsPane");
        }

        /// <summary>
        /// Handles opening the help flyout if possible, else navigating to a help page.
        /// </summary>
        /// <returns>True if a navigation is occurring, else false.</returns>
        private bool ShowHelp()
        {
            if (CanShowSettingsFlyouts())
            {
                OpenFlyout(new HelpFlyout(ViewModel.HelpViewModel));
                return false;
            }
            else
            {
                ContentFrame.Navigate(typeof(HelpView));
                return true;
            }
        }

        /// <summary>
        /// Handles opening the app settings flyout if possible, else navigating to a settings page.
        /// </summary>
        /// <returns>True if a navigation is occurring, else false.</returns>
        private bool ShowAppSettings()
        {
            if (CanShowSettingsFlyouts())
            {
                AppSettingsFlyout flyout = new AppSettingsFlyout(
                    ViewModel.AppSettingsViewModel
                );
                OpenFlyout(flyout);
                return false;
            }
            else
            {
                ContentFrame.Navigate(typeof(AppSettingsView));
                return true;
            }
        }

        /// <summary>
        /// Helper for dealing with SettingsFlyouts.
        /// </summary>
        /// <param name="flyout"></param>
        private void OpenFlyout(SettingsFlyout flyout)
        {
            // Default BackClick behavior brings up the legacy Settings Pane, which is undesirable.
            flyout.BackClick += (s, e) =>
            {
                flyout.Hide();
                e.Handled = true;
            };

            flyout.Show();
        }
    }
}
