using System;
using PassKeep.Lib.Contracts.Models;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Infrastructure;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using PassKeep.Framework;
using PassKeep.Framework.Messages;
using Windows.System;
using PassKeep.Models;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace PassKeep.Views
{
    /// <summary>
    /// A view over the active database, with a frame for displaying various child views.
    /// </summary>
    public sealed partial class DatabaseParentView : DatabaseParentViewBase
    {
        private readonly string lockButtonLabel;

        public DatabaseParentView()
            : base()
        {
            InitializeComponent();
            this.lockButtonLabel = GetString("LockButton");
            this.ContentFrame.Navigated += ContentFrame_Navigated;
        }

        /// <summary>
        /// Handles adding the database lock button to all child appbars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            PassKeepPage newPage = this.ContentFrame.Content as PassKeepPage;
            Dbg.Assert(newPage != null);

            if (newPage.BottomAppBar == null)
            {
                newPage.BottomAppBar = new CommandBar();
            }

            CommandBar commandBar = newPage.BottomAppBar as CommandBar;
            Dbg.Assert(commandBar != null);

            AppBarButton lockButton = new AppBarButton
            {
                Label = this.lockButtonLabel,
                Icon = new FontIcon
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Glyph = "\uE1F6"
                }
            };
            lockButton.Click += LockAppBarButtonClick;

            commandBar.SecondaryCommands.Add(lockButton);
        }


        /// <summary>
        /// Provides access to the <see cref="Frame"/> that hosts database content.
        /// </summary>
        public override Frame ContentFrame
        {
            get { return this.databaseContentFrame; }
        }

        /// <summary>
        /// Handles database-related hotkeys.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public override bool HandleAcceleratorKey(VirtualKey key, bool shift)
        {
            if (!shift)
            {
                switch (key)
                {
                    case VirtualKey.L:
                        this.ViewModel.TryLock();
                        return true;
                }
            }

            return base.HandleAcceleratorKey(key, shift);
        }

        #region Auto-event handlers

        /// <summary>
        /// Locks the workspace when asked by the ViewModel.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e"></param>
        public void LockRequestedHandler(object sender, EventArgs e)
        {
            Frame.Navigated += FrameLockNavigation;
            Frame.Navigate(
                typeof(DatabaseUnlockView),
                new NavigationParameter(
                    new
                    {
                        file = new StorageFileDatabaseCandidate(this.ViewModel.File),
                        isSampleFile = this.ViewModel.FileIsSample
                    }
                )
            );
        }

        #endregion

        /// <summary>
        /// Handles lock events from child AppBars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TryLock();
        }

        /// <summary>
        /// Clears the frame's backstack after a lock-related navigation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrameLockNavigation(object sender, NavigationEventArgs e)
        {
            Frame.Navigated -= FrameLockNavigation;
            Frame.BackStack.Clear();

            this.MessageBus.Publish(new DatabaseClosedMessage());

            this.SystemNavigationManager.AppViewBackButtonVisibility =
                (CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
            Dbg.Assert(this.SystemNavigationManager.AppViewBackButtonVisibility == AppViewBackButtonVisibility.Collapsed);
        }

        /// <summary>
        /// Handles navigating the content frame to the DatabaseView on first launch.
        /// </summary>
        /// <param name="e">EventArgs for the navigation.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.MessageBus.Publish(new DatabaseOpenedMessage(this.ViewModel));
            this.databaseContentFrame.Navigate(
                typeof(DatabaseView),
                this.ViewModel.GetDatabaseViewModel()
            );

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;
        }

        /// <summary>
        /// Handles unregistering the event handler for user input.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            Window.Current.CoreWindow.PointerPressed -= CoreWindow_PointerPressed;
        }

        /// <summary>
        /// EventHandler for user interaction with the BreadcrumbNavigator.
        /// </summary>
        /// <param name="sender">The BreadcrumbNavigator.</param>
        /// <param name="e">EventArgs provided the clicked group.</param>
        private void Breadcrumb_GroupClicked(object sender, GroupClickedEventArgs e)
        {
            IKeePassGroup clickedGroup = e.Group;
            Dbg.Assert(clickedGroup != null);

            IDatabaseChildView childView = this.databaseContentFrame.Content as IDatabaseChildView;
            Dbg.Assert(childView != null);

            childView.RequestBreadcrumbNavigation(
                this.ViewModel.GetDatabaseViewModel(),
                this.ViewModel.NavigationViewModel,
                clickedGroup
            );
        }

        /// <summary>
        /// Notifies the ViewModel of pointer presses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs args)
        {
            this.ViewModel.HandleInteractivity();
        }

        /// <summary>
        /// Notifies the ViewModel of keypresses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            this.ViewModel.HandleInteractivity();
        }
    }
}
