using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using PassKeep.ViewBases;
using SariphLib.Infrastructure;
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
        private const string DecryptingResourceKey = "Decrypting";

        private bool capsLockEnabled = false;

        public DatabaseUnlockView()
            : base()
        {
            this.InitializeComponent();

            // Whenever the password box is reenabled, focus it.
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

            Dbg.Trace($"Got initial caps lock state: {this.capsLockEnabled}");

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
        /// Handles KeyDown events on the window, for tracking caps lock state.
        /// </summary>
        /// <param name="sender">The CoreWindow.</param>
        /// <param name="e">EventArgs for the key event.</param>
        private void CoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            if (e.VirtualKey == VirtualKey.CapitalLock)
            {
                this.capsLockEnabled = !this.capsLockEnabled;
                Dbg.Trace($"Recorded change in caps lock state. New state: {this.capsLockEnabled}");

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
            if (e.Key == VirtualKey.Enter)
            {
                if (unlockButton.Command.CanExecute(null))
                {
                    Dbg.Trace($"{this.GetType()} got [ENTER], attempting to unlock database...");
                    unlockButton.Command.Execute(null);
                }
                else
                {
                    Dbg.Trace($"{this.GetType()} got [ENTER], but database is not currently unlockable.");
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
            Dbg.Trace("User clicked the 'open different database' button.");
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
            Dbg.Trace("User clicked the 'choose keyfile' button.");
            await PickFile(
                file =>
                {
                    this.ViewModel.KeyFile = file;
                },
                /* cancelled */ () =>
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
