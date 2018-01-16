﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Framework;
using PassKeep.Framework.Messages;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Views
{
    /// <summary>
    /// A view over the active database, with a frame for displaying various child views.
    /// </summary>
    public sealed partial class DatabaseParentView : DatabaseParentViewBase
    {
        private readonly string lockButtonLabel;
        private readonly string settingsLabel;

        public DatabaseParentView()
            : base()
        {
            InitializeComponent();
            this.lockButtonLabel = GetString("LockButton");
            this.settingsLabel = GetString("DbSettingsButton");
            ContentFrame.Navigated += ContentFrame_Navigated;
        }
        
        /// <summary>
        /// Handles adding the database lock button to all child appbars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            PassKeepPage newPage = ContentFrame.Content as PassKeepPage;
            DebugHelper.Assert(newPage != null);

            if (newPage.BottomAppBar == null)
            {
                newPage.BottomAppBar = new CommandBar();
            }

            CommandBar commandBar = newPage.BottomAppBar as CommandBar;
            DebugHelper.Assert(commandBar != null);

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

            AppBarButton settingsButton = new AppBarButton
            {
                Label = this.settingsLabel,
                Icon = new FontIcon
                {
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Glyph = "\uE115"
                }
            };
            settingsButton.Click += SettingsButtonClick;

            commandBar.SecondaryCommands.Add(lockButton);
            commandBar.SecondaryCommands.Add(settingsButton);
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
                        ViewModel.TryLock();
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
        [AutoWire(nameof(IDatabaseParentViewModel.LockRequested))]
        public async void LockRequestedHandler(object sender, EventArgs e)
        {
            Frame.Navigated += FrameLockNavigation;
            Frame.Navigate(
                typeof(DatabaseUnlockView),
                new NavigationParameter(
                    new
                    {
                        file = await DatabaseCandidateFactory.AssembleAsync(ViewModel.File),
                        isSampleFile = ViewModel.FileIsSample
                    }
                )
            );
        }

        /// <summary>
        /// Shows database settings when requested by the ViewModel.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e"></param>
        [AutoWire(nameof(IDatabaseParentViewModel.SettingsRequested))]
        public void SettingsRequestedHandler(object sender, SettingsRequestedEventArgs e)
        {
            this.DatabaseSettingsPopup.IsLightDismissEnabled = false;
            this.DatabaseSettingsPopup.IsOpen = true;
            return;
            this.databaseContentFrame.Navigate(
                typeof(DatabaseSettingsView),
                e.SettingsViewModel
            );
        }

        #endregion

        /// <summary>
        /// Handles PropertyChanged events from the persistence service.
        /// </summary>
        /// <param name="sender">The persistence service.</param>
        /// <param name="e">EventArgs for the property change.</param>
        private void PersistenceServicePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            IDatabasePersistenceService service = sender as IDatabasePersistenceService;
            DebugHelper.Assert(service != null);

            if (e.PropertyName == nameof(service.IsSaving))
            {
                MessageBus.Publish(new SavingStateChangeMessage(service.IsSaving));
            }
        }

        /// <summary>
        /// Handles lock events from child AppBars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockAppBarButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.TryLock();
        }

        /// <summary>
        /// Handles settings events from child AppBars.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.RequestSettings();
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

            MessageBus.Publish(new DatabaseClosedMessage());

            SystemNavigationManager.AppViewBackButtonVisibility =
                (CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
            DebugHelper.Assert(SystemNavigationManager.AppViewBackButtonVisibility == AppViewBackButtonVisibility.Collapsed);
        }

        /// <summary>
        /// Handles navigating the content frame to the DatabaseView on first launch.
        /// </summary>
        /// <param name="e">EventArgs for the navigation.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                // When we arrive here from unlocking a database or from creating a new database,
                // we want to obliterate that page from the stack. Going back to that page is not 
                // useful to users and can actually have negative consequences.
                if (Frame.CanGoBack)
                {
                    Type previousPage = Frame.BackStack[Frame.BackStack.Count - 1].SourcePageType;
                    if (previousPage == typeof(DatabaseCreationView) || previousPage == typeof(DatabaseUnlockView))
                    {
                        Frame.BackStack.RemoveAt(Frame.BackStack.Count - 1);
                    }
                }
            }

            //this.MessageBus.Publish(new DatabaseOpenedMessage(this.ViewModel));
            this.databaseContentFrame.Navigate(
                typeof(DatabaseView),
                ViewModel.GetDatabaseViewModel()
            );

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

            ViewModel.PersistenceService.PropertyChanged += PersistenceServicePropertyChangedHandler;
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

            ViewModel.PersistenceService.PropertyChanged -= PersistenceServicePropertyChangedHandler;
        }

        /// <summary>
        /// EventHandler for user interaction with the BreadcrumbNavigator.
        /// </summary>
        /// <param name="sender">The BreadcrumbNavigator.</param>
        /// <param name="e">EventArgs provided the clicked group.</param>
        private void Breadcrumb_GroupClicked(object sender, GroupClickedEventArgs e)
        {
            IKeePassGroup clickedGroup = e.Group;
            DebugHelper.Assert(clickedGroup != null);

            IDatabaseChildView childView = this.databaseContentFrame.Content as IDatabaseChildView;
            DebugHelper.Assert(childView != null);

            childView.RequestBreadcrumbNavigation(
                ViewModel.GetDatabaseViewModel(),
                ViewModel.NavigationViewModel,
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
            ViewModel.HandleInteractivity();
        }

        /// <summary>
        /// Notifies the ViewModel of keypresses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            ViewModel.HandleInteractivity();
        }
    }
}
