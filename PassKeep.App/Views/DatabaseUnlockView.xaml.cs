using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using PassKeep.ViewBases;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// A View used to unlock/decrypt a candidate database file.
    /// </summary>
    public sealed partial class DatabaseUnlockView : DatabaseUnlockViewBase
    {
        private const int WideWidth = 1024;
        private const string DecryptingResourceKey = "Decrypting";

        private bool capsLockEnabled = false;

        public DatabaseUnlockView()
            : base()
        {
            this.InitializeComponent();
            this.passwordBox.IsEnabledChanged += (s, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    this.passwordBox.Focus(FocusState.Programmatic);
                }
            };
        }

        /// <summary>
        /// Handles setting up the caps lock key handler.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            CoreVirtualKeyStates capsState = Window.Current.CoreWindow.GetKeyState(VirtualKey.CapitalLock);
            this.capsLockEnabled = (capsState == CoreVirtualKeyStates.Locked);

            Debug.WriteLine("Got initial caps lock state: {0}", this.capsLockEnabled);

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        /// <summary>
        /// Handles tearing down the caps lock key handler.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
        }

        /// <summary>
        /// Updates the VisualStateManager's state based on a new window size.
        /// </summary>
        /// <param name="newWindowSize">The size to base the state on.</param>
        protected override void SetVisualState(Size windowSize)
        {
            string newState = WideState.Name;

            if (windowSize.Width < DatabaseUnlockView.WideWidth)
            {
                if (windowSize.Width >= PassKeepPage.NarrowWidth)
                {
                    newState = StackedState.Name;
                }
                else
                {
                    newState = NarrowState.Name;
                }
            }

            this.innerPanel.Width = windowSize.Width;

            Debug.WriteLine("Going to new VisualState: {0}", newState);
            Debug.WriteLine(VisualStateManager.GoToState(this, newState, false));

            if (newState == NarrowState.Name)
            {
                // Take up the entire snapped column
                this.formGrid.Width = windowSize.Width - 20;
                
                // Layout metadata table vertically
            }
            else
            {
                this.formGrid.ClearValue(Grid.WidthProperty);

                // Layout metadata table horizontally
            }

            // If we don't have a wide space, we need to stack the sections vertically
            // Please kill me now and give me flexbox :|
            if (newState == NarrowState.Name || newState == StackedState.Name)
            {
                Grid.SetRow(this.formGrid, 1);
                Grid.SetColumn(this.formGrid, 0);

                // Vertical layout (spanning both columns);
                Grid.SetRowSpan(this.metadataPanelWide, 1);
                Grid.SetRowSpan(this.formGrid, 1);
                Grid.SetColumnSpan(this.metadataPanelWide, 2);
                Grid.SetColumnSpan(this.formGrid, 2);
            }
            else
            {
                Grid.SetRowSpan(this.metadataPanelWide, 2);
                Grid.SetRowSpan(this.formGrid, 2);
                Grid.SetColumnSpan(this.metadataPanelWide, 1);
                Grid.SetColumnSpan(this.formGrid, 1);

                Grid.SetRow(this.formGrid, 0);
                Grid.SetColumn(this.formGrid, 1);
            }

            // We re-layout the metadata panel based on width
            if (newState == NarrowState.Name)
            {
                this.metadataPanelWide.Visibility = Visibility.Collapsed;
                this.metadataPanelNarrow.Visibility = Visibility.Visible;
            }
            else
            {
                this.metadataPanelWide.Visibility = Visibility.Visible;
                this.metadataPanelNarrow.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles KeyDown events on the window, for tracking caps lock state.
        /// </summary>
        /// <param name="sender">The CoreWindow.</param>
        /// <param name="e">EventArgs for the key event.</param>
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            if (e.VirtualKey == VirtualKey.CapitalLock)
            {
                this.capsLockEnabled = !this.capsLockEnabled;
                Debug.WriteLine("Recorded change in caps lock state. New state: {0}", this.capsLockEnabled);

                if (this.passwordBox.FocusState != FocusState.Unfocused && this.capsLockEnabled)
                {
                    this.capsLockPopup.IsOpen = true;
                }
                else if (!this.capsLockEnabled)
                {
                    this.capsLockPopup.IsOpen = false;
                }
            }
        }

        /// <summary>
        /// Handles the ENTER key for the password input box.
        /// </summary>
        /// <param name="sender">The password box.</param>
        /// <param name="e">EventArgs for the KeyUp event.</param>
        private void PasswordBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (unlockButton.Command.CanExecute(null))
                {
                    Debug.WriteLine("{0} got [ENTER], attempting to unlock database...", this.GetType());
                    unlockButton.Command.Execute(null);
                }
                else
                {
                    Debug.WriteLine("{0} got [ENTER], but database is not currently unlockable.", this.GetType());
                }
            }
        }

        /// <summary>
        /// Handles letting the user select a different database from this View, without navigating.
        /// </summary>
        /// <param name="sender">The button that invokes this command.</param>
        /// <param name="e">EventArgs for the click.</param>
        private async void DifferentDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("User clicked the 'open different database' button.");
            await PickFile(
                file =>
                {
                    this.ViewModel.CandidateFile = new StorageFileDatabaseCandidate(file);
                }
            );
        }

        /// <summary>
        /// Handles letting the user select a keyfile.
        /// </summary>
        /// <param name="sender">The button that invokes this command.</param>
        /// <param name="e">EventArgs for the click.</param>
        private async void ChooseKeyfileButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("User clicked the 'choose keyfile' button.");
            await PickFile(
                file =>
                {
                    this.ViewModel.KeyFile = file;
                },
                () =>
                {
                    this.ViewModel.KeyFile = null;
                }
            );
        }

        /// <summary>
        /// Handles showing the caps lock warning flyout.
        /// </summary>
        /// <param name="sender">The PasswordBox.</param>
        /// <param name="e">EventArgs for the notification.</param>
        private void passwordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.capsLockEnabled)
            {
                this.capsLockPopup.IsOpen = true;
            }
        }

        /// <summary>
        /// Handles hiding the caps lock warning flyout.
        /// </summary>
        /// <param name="sender">The PaswordBox.</param>
        /// <param name="e">EventArgs for the notification.</param>
        private void passwordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.capsLockPopup.IsOpen = false;
        }

        #region Auto-event handles

        /// <summary>
        /// Auto-event handler for the ViewModel's DocumentReady event.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">EventArgs with the new document.</param>
        public void DocumentReadyHandler(object sender, DocumentReadyEventArgs e)
        {
            Frame.Navigate(
                typeof(DatabaseView),
                new NavigationParameter(
                    new {
                        document = e.Document,
                        writer = e.Writer,
                        candidate = this.ViewModel.CandidateFile
                    }
                )
            );
        }

        /// <summary>
        /// Auto-event handler for the ViewModel's StartedUnlocking event.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">CancellableEventArgs for the operation.</param>
        public void StartedUnlockingHandler(object sender, CancellableEventArgs e)
        {
            this.RaiseStartedLoading(
                new LoadingStartedEventArgs(
                    ResourceLoader.GetForCurrentView().GetString(
                        DatabaseUnlockView.DecryptingResourceKey
                    ),
                    e.Cts
                )
            );
        }

        /// <summary>
        /// Auto-event handler for the ViewModel's StoppedUnlocking event.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">EventArgs for the notification.</param>
        public void StoppedUnlockingHandler(object sender, EventArgs e)
        {
            this.RaiseDoneLoading();
        }

        #endregion
    }
}
