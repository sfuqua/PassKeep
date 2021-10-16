// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Framework.Messages;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.ViewBases;
using PassKeep.Views;
using PassKeep.Views.FlyoutPages;
using SariphLib.Diagnostics;
using SariphLib.Files;
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
using MUXC = Microsoft.UI.Xaml.Controls;
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

        // The size of the NavigationView pane when opened.
        private double navViewPaneLength = 0;

        public RootView()
        {
            InitializeComponent();

            this.ContentBackCommand = new ActionCommand(CanGoBack, GoBack);
            this.ContentBackCommand.CanExecuteChanged += (s, e) =>
            {
                this.NavigationView.IsBackEnabled = this.ContentBackCommand.CanExecute(null);
            };

            // Raised by child frames
            CanGoBackChanged += (s, e) =>
            {
                this.ContentBackCommand.RaiseCanExecuteChanged();
            };

            MessageBus = new MessageBus();
            BootstrapMessageSubscriptions(
                typeof(DatabaseCandidateMessage),
                typeof(DatabaseOpenedMessage),
                typeof(DatabaseClosedMessage)
            );

            // Handle adjusting the size of the SplitView when IsPaneOpen changes, to cause Flyouts to position properly
            this.navViewPaneLength = this.NavigationView.OpenPaneLength;
            this.NavigationView.RegisterPropertyChangedCallback(MUXC.NavigationView.IsPaneOpenProperty, OnNavViewIsPaneOpenChanged);
            OnNavViewIsPaneOpenChanged(this.NavigationView, MUXC.NavigationView.IsPaneOpenProperty);
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
                    this.NavigationView.SelectedItem = this.dashNavItem;
                    // this.contentFrame.Navigate(typeof(DashboardView));
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
        /// Handler for NavigationVIew.IsPaneOpen changing - handles resizing the NavigationView's OpenPaneLength.
        /// </summary>
        /// <remarks>This size adjustment is necessary so that attached flyouts position properly relative to NavigationView nav bar items.</remarks>
        /// <param name="sender">The NavigationView.</param>
        /// <param name="prop">The IsPaneOpen property.</param>
        private void OnNavViewIsPaneOpenChanged(DependencyObject sender, DependencyProperty prop)
        {
            DebugHelper.Assert(prop == MUXC.NavigationView.IsPaneOpenProperty);

            MUXC.NavigationView navView = (MUXC.NavigationView)sender;
            if (navView.IsPaneOpen)
            {
                navView.OpenPaneLength = this.navViewPaneLength;
            }
            else if (navView.DisplayMode == MUXC.NavigationViewDisplayMode.Compact)
            {
                navView.OpenPaneLength = navView.CompactPaneLength;
            }
            else if (navView.DisplayMode == MUXC.NavigationViewDisplayMode.Minimal)
            {
                navView.OpenPaneLength = 0;
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
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            CoreVirtualKeyStates ctrlState = sender.GetKeyState(VirtualKey.Control);
            if ((ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
            {
                CoreVirtualKeyStates shiftState = sender.GetKeyState(VirtualKey.Shift);
                bool shiftDown = (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                // Handle accelerator (Ctrl) hotkeys
                switch (args.VirtualKey)
                {
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
            SynchronizeNavigationViewSelection();
            ContentFrame.Focus(FocusState.Programmatic);
        }

        #endregion

        /// <summary>
        /// Updates the selected item of the navigation ListView without navigating.
        /// </summary>
        private void SynchronizeNavigationViewSelection()
        {
            if (this.contentFrame.Content is DashboardView || this.contentFrame.Content is DatabaseParentView)
            {
                SetNavigationViewSelection(this.dashNavItem);
            }
            else if (this.contentFrame.Content is DatabaseUnlockView || this.contentFrame.Content is DatabaseCreationView)
            {
                SetNavigationViewSelection(this.openNavItem);
            }
            /* else if (this.contentFrame.Content is DatabaseParentView)
            {
                Dbg.Assert(this.dbHomeItem.Visibility == Visibility.Visible);
                SetNavigationListViewSelection(this.dbHomeItem);
            } */
            else if (this.contentFrame.Content is HelpView)
            {
                SetNavigationViewSelection(this.helpNavItem);
            }
            else if (this.contentFrame.Content is AppSettingsView)
            {
                SetNavigationViewSelection(this.NavigationView.SettingsItem);
            }
            else
            {
                SetNavigationViewSelection(null);
            }
        }

        /// <summary>
        /// Helper to set the selected value of the NavigationView list without invoking the event handler.
        /// </summary>
        /// <param name="selection">The value to forcibly select.</param>
        private void SetNavigationViewSelection(object selection)
        {
            this.NavigationView.SelectionChanged -= NavigationView_SelectionChanged;
            this.NavigationView.SelectedItem = selection;
            this.NavigationView.SelectionChanged += NavigationView_SelectionChanged;
        }

        private async void NavigationView_ItemInvoked(MUXC.NavigationView sender, MUXC.NavigationViewItemInvokedEventArgs args)
        {
            async Task Revert()
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, SynchronizeNavigationViewSelection);
            }

            if (args.InvokedItemContainer.Equals(this.passwordNavItem))
            {
                DebugHelper.Trace("Password Generator selected in NavigationView.");
                FlyoutBase.ShowAttachedFlyout(this.passwordNavItem);
            }
            else if (args.InvokedItemContainer.Equals(this.openNavItem))
            {
                DebugHelper.Trace("Open selected in NavigationView.");
                await PickFileForOpenAndContinueAsync(
                    /* gotFile */ file =>
                                  {
                                      OpenFile(file);
                                      return Task.CompletedTask;
                                  },
                    /* cancelled */
                    Revert
                );
            }
        }

        private void NavigationView_BackRequested(MUXC.NavigationView sender, MUXC.NavigationViewBackRequestedEventArgs args)
        {
            this.ContentBackCommand.Execute(null);
        }

        private void NavigationView_SelectionChanged(MUXC.NavigationView sender, MUXC.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                DebugHelper.Trace("Settings selected in NavigationView.");
                ContentFrame.Navigate(typeof(AppSettingsView));
            }
            else if (args.SelectedItemContainer.Equals(this.dashNavItem))
            {
                DebugHelper.Trace("Dashboard selected in NavigationView.");
                ContentFrame.Navigate(typeof(DashboardView));
            }
            else if (args.SelectedItemContainer.Equals(this.homeNavItem))
            {
                DebugHelper.Trace("Database Home selected in NavigationView.");
                DebugHelper.Assert(ViewModel.DecryptedDatabase != null, "This button should not be accessible if there is not decrypted database");
                ContentFrame.Navigate(typeof(DatabaseParentView), ViewModel.DecryptedDatabase);
            }
            else if (args.SelectedItemContainer.Equals(this.helpNavItem))
            {
                DebugHelper.Trace("Help selected in NavigationView.");
                ContentFrame.Navigate(typeof(HelpView));
            }
        }

        private void Flyout_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
        {
            SynchronizeNavigationViewSelection();
        }
    }
}
