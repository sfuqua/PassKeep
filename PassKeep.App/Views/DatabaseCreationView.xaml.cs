using System;
using PassKeep.ViewBases;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using SariphLib.Mvvm;
using SariphLib.Infrastructure;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.System;
using Windows.UI.Xaml.Input;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Services;
using PassKeep.Models;
using PassKeep.Framework;
using System.Threading.Tasks;
using PassKeep.Lib.KeePass.SecurityTokens;
using Windows.UI.Xaml.Controls.Primitives;
using SariphLib.Files;
using PassKeep.Lib.Contracts.ViewModels;

namespace PassKeep.Views
{
    /// <summary>
    /// View used to create a new database file.
    /// </summary>
    public sealed partial class DatabaseCreationView : DatabaseCreationViewBase
    {
        private bool capsLockEnabled = false;

        public DatabaseCreationView()
        {
            InitializeComponent();
        }

        #region Auto-event handlers

        /// <summary>
        /// The ViewModel has indicated the document is ready for viewing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [AutoWire(nameof(IDatabaseCreationViewModel.DocumentReady))]
        public async void DocumentReadyHandler(object sender, DocumentReadyEventArgs e)
        {
            IDatabasePersistenceService persistenceService = new DefaultFilePersistenceService(
                e.Writer,
                await DatabaseCandidateFactory.AssembleAsync(this.ViewModel.File),
                this.SyncContext,
                await this.ViewModel.File.CheckWritableAsync());

            Frame.Navigated -= FrameNavigated;
            Frame.Navigate(
                typeof(DatabaseParentView),
                new NavigationParameter(
                    new
                    {
                        file = this.ViewModel.File,
                        fileIsSample = false,
                        document = e.Document,
                        rng = e.Rng,
                        persistenceService = persistenceService
                    }
                )
            );
        }

        #endregion

        /// <summary>
        /// Handles adding an event handler for Frame navigation to delete orphan StorageFiles.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame.Navigated += FrameNavigated;

            CoreVirtualKeyStates capsState = Window.Current.CoreWindow.GetKeyState(VirtualKey.CapitalLock);
            this.capsLockEnabled = (capsState == CoreVirtualKeyStates.Locked);

            Dbg.Trace($"Got initial caps lock state: {this.capsLockEnabled}");

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }

        /// <summary>
        /// Handles unregistering the keydown handler.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (!e.Cancel)
            {
                Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            }
        }

        /// <summary>
        /// Deletes orphaned StorageFiles if the ViewModel did not save.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FrameNavigated(object sender, NavigationEventArgs e)
        {
            Frame.Navigated -= FrameNavigated;
            await this.ViewModel.File.DeleteAsync(StorageDeleteOption.PermanentDelete);
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

                if (!this.capsLockEnabled)
                {
                    this.capsLockPopup.IsOpen = false;
                }
                else
                {
                    if (this.passwordBox.FocusState != FocusState.Unfocused)
                    {
                        this.capsLockPopup.ShowBelow(this.passwordBox, this.layoutRoot);
                    }
                    else if (this.passwordConfirmBox.FocusState != FocusState.Unfocused)
                    {
                        this.capsLockPopup.ShowBelow(this.passwordConfirmBox, this.layoutRoot);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ENTER key for the password input boxes.
        /// </summary>
        /// <param name="sender">The password box.</param>
        /// <param name="e">EventArgs for the KeyUp event.</param>
        private void PasswordBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (createButton.Command.CanExecute(null))
                {
                    Dbg.Trace($"{this.GetType()} got [ENTER], attempting to unlock database...");
                    createButton.Command.Execute(null);
                }
                else
                {
                    Dbg.Trace($"{this.GetType()} got [ENTER], but database is not currently unlockable.");
                }
            }
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
                this.capsLockPopup.ShowBelow((PasswordBox)sender, this.layoutRoot);
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
        /// Updates the bound confirmed password value for every keystroke.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void passwordConfirmBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Dbg.Assert(sender == this.passwordConfirmBox);
            this.ViewModel.ConfirmedPassword = ((PasswordBox)sender).Password;
        }

        /// <summary>
        /// Updates the bound master password value for every keystroke.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Dbg.Assert(sender == this.passwordBox);
            this.ViewModel.MasterPassword = ((PasswordBox)sender).Password;
        }

        /// <summary>
        /// Enables nulling out of the keyfile by deleting all text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyFileBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(((TextBox)sender).Text))
            {
                this.ViewModel.KeyFile = null;
            }
        }

        /// <summary>
        /// Computes the number of encryption rounds needed for a one second delay.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void encryptionRoundsOneSecond_Click(object sender, RoutedEventArgs e)
        {
            this.encryptionRoundsOneSecond.IsEnabled = false;
            this.encryptionRounds.IsEnabled = false;


            ulong rounds = await KeyHelper.ComputeOneSecondDelay();
            int value = (int)Math.Min(rounds, (ulong)int.MaxValue);
            this.encryptionRounds.Value = value;

            this.encryptionRounds.IsEnabled = true;
            this.encryptionRoundsOneSecond.IsEnabled = true;
        }
    }
}
