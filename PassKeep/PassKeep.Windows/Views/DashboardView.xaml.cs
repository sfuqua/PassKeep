﻿using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Models;
using PassKeep.ViewBases;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=321224

namespace PassKeep.Views
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class DashboardView : DashboardViewBase
    {
        private Grid welcomeBlurb;

        public DashboardView()
            : base()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Updates the VisualStateManager's state based on a new window size.
        /// </summary>
        /// <param name="newWindowSize">The size to base the state on.</param>
        protected override void SetVisualState(Size windowSize)
        {
            string newState = StandardState.Name;

            if (windowSize.Width < PassKeepPage.NarrowWidth)
            {
                newState = NarrowState.Name;
            }

            Debug.WriteLine("Going to new VisualState: {0}", newState);
            Debug.WriteLine(VisualStateManager.GoToState(this, newState, false));

            if (newState == StandardState.Name)
            {
                this.rootHub.Orientation = Orientation.Horizontal;
                this.welcomeBlurb.Visibility = Visibility.Visible;
                this.welcomeSection.ClearValue(HubSection.WidthProperty);
                this.recentSection.ClearValue(HubSection.WidthProperty);
                this.recentSection.ClearValue(HubSection.WidthProperty);
                this.pageTitle.ClearValue(TextBlock.FontSizeProperty);
            }
            else if (newState == NarrowState.Name)
            {
                this.rootHub.Orientation = Orientation.Vertical;
                this.welcomeBlurb.Visibility = Visibility.Collapsed;
                this.welcomeSection.Width = windowSize.Width;
                this.recentSection.Width = windowSize.Width;
                //this.pinnedSection.Width = windowSize.Width;
                this.pageTitle.FontSize = 30.0;
            }
        }

        /// <summary>
        /// Stores a reference to the welcome blurb Grid when it loads.
        /// </summary>
        /// <param name="sender">The Grid we're interested in.</param>
        /// <param name="e">EventArgs for the Loaded event.</param>
        private void welcomeBlurb_Loaded(object sender, RoutedEventArgs e)
        {
            this.welcomeBlurb = (Grid)sender;
        }

        /// <summary>
        /// Attempts to open a database of the user's choosing.
        /// </summary>
        /// <param name="sender">The "open database" button.</param>
        /// <param name="e">Args for the click.</param>
        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // Not all databases end in .kdbx
            picker.FileTypeFilter.Add("*");

            StorageFile pickedKdbx = await picker.PickSingleFileAsync();
            if (pickedKdbx == null)
            {
                // If nothing was picked, abort.
                return;
            }

            StorageApplicationPermissions.MostRecentlyUsedList.Add(pickedKdbx, pickedKdbx.Name);
            NavigateToOpenedFile(pickedKdbx);
        }

        /// <summary>
        /// Attempts to open the sample database.
        /// </summary>
        /// <param name="sender">The "open sample" button.</param>
        /// <param name="e">Args for the click.</param>
        private async void OpenSample_Click(object sender, RoutedEventArgs e)
        {
            // Locate the sample file
            StorageFolder installFolder = Package.Current.InstalledLocation;
            StorageFolder subFolder = await installFolder.GetFolderAsync("Assets");
            StorageFile sample = await subFolder.GetFileAsync("SampleDatabase.kdbx");

            NavigateToOpenedFile(sample, true);
        }

        /// <summary>
        /// Navigates the Frame to the DatabaseUnlockView for the specified database file.
        /// </summary>
        /// <param name="file">The file to begin unlocking.</param>
        /// <param name="isSample">Whether this is the sample database.</param>
        private void NavigateToOpenedFile(IStorageFile file, bool isSample = false)
        {
            Frame.Navigate(
                typeof(DatabaseUnlockView),
                new
                {
                    file = file,
                    isSampleFile = isSample
                }
            );
        }

        /// <summary>
        /// Handles right-tap/click on recent databases.
        /// Shows the context menu.
        /// </summary>
        /// <param name="sender">The database tile being interacted with.</param>
        /// <param name="e">Args for the tap.</param>
        private void GridViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement tappedElement = sender as FrameworkElement;
            Debug.Assert(tappedElement != null);

            FlyoutBase.ShowAttachedFlyout(tappedElement);
        }

        /// <summary>
        /// Handles primary taps/clicks on recent databases.
        /// Attempts to open the files.
        /// </summary>
        /// <param name="sender">The database tile being interacted with.</param>
        /// <param name="e">Args for the tap.</param>
        private async void GridViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FrameworkElement tappedElement = sender as FrameworkElement;
            Debug.Assert(tappedElement != null);

            StoredFileDescriptor tappedDescriptor = tappedElement.DataContext as StoredFileDescriptor;
            Debug.Assert(tappedDescriptor != null);

            IStorageFile tappedFile = await this.ViewModel.GetFileAsync(tappedDescriptor);
            if (tappedFile == null)
            {
                Debug.WriteLine("Warning: Could not fetch StorageFile. Forgetting descriptor.");
                this.ViewModel.ForgetCommand.Execute(tappedDescriptor);
            }
            else
            {
                Debug.WriteLine("Retrieved StorageFile from descriptor.");
                NavigateToOpenedFile(tappedFile);
            }
        }
    }
}