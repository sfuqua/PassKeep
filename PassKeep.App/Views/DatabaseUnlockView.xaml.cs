using PassKeep.Contracts.Models;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.Services;
using PassKeep.Models;
using PassKeep.ViewBases;
using SariphLib.Files;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        }

        /// <summary>
        /// Works around an issue where the password box is not focused after navigate.
        /// </summary>
        private void FocusPasswordBoxIfAppropriate()
        {
            if (this.passwordBox.IsEnabled)
            {
                this.passwordBox.Focus(FocusState.Programmatic);
            }
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

            // XXX - this works around what seems to be a Windows bug where
            // when navigating from RootView bindings are not updating.
            // Remove when able.
            Bindings.Update();
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
                    this.capsLockPopup.ShowBelow(this.passwordBox, this.formPanel);
                }
                else if (!this.capsLockEnabled)
                {
                    this.capsLockPopup.IsOpen = false;
                }
            }
        }

        /// <summary>
        /// Focuses the password box after load if appropriate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FocusPasswordBoxIfAppropriate();
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
                async file =>
                {
                    this.ViewModel.CandidateFile = await DatabaseCandidateFactory.AssembleAsync(file);
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
            Dbg.Assert(sender is PasswordBox);
            if (this.capsLockEnabled)
            {
                this.capsLockPopup.ShowBelow(this.passwordBox, this.formPanel);
            }
        }

        /// <summary>
        /// Handles hiding the caps lock warning flyout.
        /// </summary>
        /// <param name="sender">The PaswordBox.</param>
        /// <param name="e">EventArgs for the notification.</param>
        private void passwordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Dbg.Assert(sender is PasswordBox);
            this.capsLockPopup.IsOpen = false;
        }

        /// <summary>
        /// Handles focusing the PasswordBox when it is enabled.
        /// </summary>
        /// <param name="sender">The PasswordBox.</param>
        /// <param name="e">EventArgs for the property change.</param>
        private void passwordBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FocusPasswordBoxIfAppropriate();
        }

        #region Auto-event handles

        /// <summary>
        /// Auto-event handler for the ViewModel's DocumentReady event.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">EventArgs with the new document.</param>
        public async void DocumentReadyHandler(object sender, DocumentReadyEventArgs e)
        {
            IDatabasePersistenceService persistenceService;
            if (this.ViewModel.IsSampleFile)
            {
                persistenceService = new DummyPersistenceService();
            }
            else
            {
                persistenceService = new DefaultFilePersistenceService(
                    e.Writer,
                    this.ViewModel.CandidateFile,
                    this.SyncContext,
                    await this.ViewModel.CandidateFile.StorageItem.CheckWritableAsync()
                );
            }

            Frame.Navigate(
                typeof(DatabaseParentView),
                new NavigationParameter(
                    new {
                        file = this.ViewModel.CandidateFile.StorageItem,
                        fileIsSample = this.ViewModel.IsSampleFile,
                        document = e.Document,
                        rng = e.Rng,
                        persistenceService = persistenceService
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
