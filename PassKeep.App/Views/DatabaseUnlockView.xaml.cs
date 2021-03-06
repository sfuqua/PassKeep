﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.Services;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Files;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
using System.Text;
using Windows.ApplicationModel;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Views
{
    /// <summary>
    /// A View used to unlock/decrypt a candidate database file.
    /// </summary>
    public sealed partial class DatabaseUnlockView : DatabaseUnlockViewBase
    {
        private const string DecryptingResourceKey = "Decrypting";
        private const string CredentialStorageFullResourceKey = "CredentialsFullPrompt";
        private const string CredentialsStorageFailedTitleResourceKey = "CredentialsFullFailureTitle";
        private const string CredentialsStorageFailedResourceKey = "CredentialsFullFailure";

        private bool capsLockEnabled = false;

        public DatabaseUnlockView()
            : base()
        {
            InitializeComponent();
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

            DebugHelper.Trace($"Got initial caps lock state: {this.capsLockEnabled}");
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            // XXX - this works around what seems to be a Windows bug where
            // when navigating from RootView bindings are not updating.
            // Remove when able.
            this.Bindings.Update();
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
                DebugHelper.Trace($"Recorded change in caps lock state. New state: {this.capsLockEnabled}");

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
                if (this.unlockButton.Command.CanExecute(null))
                {
                    DebugHelper.Trace($"{GetType()} got [ENTER], attempting to unlock database...");
                    this.unlockButton.Command.Execute(null);
                }
                else
                {
                    DebugHelper.Trace($"{GetType()} got [ENTER], but database is not currently unlockable.");
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
            DebugHelper.Trace("User clicked the 'open different database' button.");
            await PickFileForOpenAndContinueAsync(
                async file =>
                {
                    await ViewModel.UpdateCandidateFileAsync(await DatabaseCandidateFactory.AssembleAsync(file));
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
            DebugHelper.Trace("User clicked the 'choose keyfile' button.");
            await PickFileForOpenAsync(
                file =>
                {
                    ViewModel.KeyFile = file;
                },
                /* cancelled */ () =>
                {
                    ViewModel.KeyFile = null;
                }
            );
        }

        /// <summary>
        /// Handles showing the caps lock warning flyout.
        /// </summary>
        /// <param name="sender">The PasswordBox.</param>
        /// <param name="e">EventArgs for the notification.</param>
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            DebugHelper.Assert(sender is PasswordBox);
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
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DebugHelper.Assert(sender is PasswordBox);
            this.capsLockPopup.IsOpen = false;
        }

        /// <summary>
        /// Handles focusing the PasswordBox when it is enabled.
        /// </summary>
        /// <param name="sender">The PasswordBox.</param>
        /// <param name="e">EventArgs for the property change.</param>
        private void PasswordBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FocusPasswordBoxIfAppropriate();
        }


        /// <summary>
        /// Generates an error report based on the ViewModel's parse result.
        /// </summary>
        /// <param name="sender">The button that was clicked to fire this event.</param>
        /// <param name="e"></param>
        private async void ReportErrorButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder emailBuilder = new StringBuilder();
            emailBuilder.Append("mailto:");
            emailBuilder.Append(GetString("ContactEmail.Text"));
            emailBuilder.Append("?subject=");
            emailBuilder.Append("PassKeep: Trouble opening database");
            emailBuilder.Append("&body=");

            StringBuilder bodyBuilder = new StringBuilder();
            PackageVersion pkgVersion = Package.Current.Id.Version;
            string appVersion = $"{pkgVersion.Major}.{pkgVersion.Minor}.{pkgVersion.Revision}";
            bodyBuilder.AppendLine($"PassKeep version {appVersion} could not open my database.");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine($"Failure: {ViewModel.ParseResult.Code}");
            bodyBuilder.AppendLine($"Details: {ViewModel.ParseResult.Details}");
            bodyBuilder.AppendLine();

            // Get the device family information
            bodyBuilder.AppendLine($"Device family: {AnalyticsInfo.VersionInfo.DeviceFamily}");

            // Get the OS version number
            string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong version = UInt64.Parse(deviceFamilyVersion);
            ulong majorVersion = (version & 0xFFFF000000000000L) >> 48;
            ulong minorVersion = (version & 0x0000FFFF00000000L) >> 32;
            ulong buildVersion = (version & 0x00000000FFFF0000L) >> 16;
            ulong revisionVersion = (version & 0x000000000000FFFFL);
            bodyBuilder.AppendLine($"System version: {majorVersion}.{minorVersion}.{buildVersion}.{revisionVersion}");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine("Additional details - what other KeePass apps have you used with this database?");

            emailBuilder.Append(Uri.EscapeUriString(bodyBuilder.ToString()));

            Uri mailUri = new Uri(emailBuilder.ToString());
            await Launcher.LaunchUriAsync(mailUri);
        }

        #region Auto-event handles

        /// <summary>
        /// Auto-event handler for the ViewModel's PropertyChanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [AutoWire(nameof(IDatabaseUnlockViewModel.PropertyChanged))]
        public async void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.HasSavedCredentials))
            {
                // If we newly determine that the ViewModel has saved credentials, ask
                // the user if they want to authenticate with them.
                if (ViewModel.HasSavedCredentials)
                {
                    MessageDialog promptDialog = new MessageDialog(
                        GetString("UseSavedCredentialsContent"),
                        GetString("UseSavedCredentialsTitle")
                    )
                    {
                        Options = MessageDialogOptions.None
                    };

                    IUICommand yesCommand = new UICommand(GetString("Yes"));
                    promptDialog.Commands.Add(yesCommand);

                    IUICommand noCommand = new UICommand(GetString("No"));
                    promptDialog.Commands.Add(noCommand);

                    promptDialog.DefaultCommandIndex = 0;
                    promptDialog.CancelCommandIndex = 1;
                    IUICommand chosenCommand = await promptDialog.ShowAsync();

                    if (chosenCommand == yesCommand)
                    {
                        DebugHelper.Trace("User opted to use saved credentials");
                        await ViewModel.UseSavedCredentialsCommand.ExecuteAsync(null);
                    }
                    else
                    {
                        DebugHelper.Trace("User opted not to use saved credentials");
                    }
                }
            }
        }

        /// <summary>
        /// Auto-event handler for the ViewModel's DocumentReady event.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">EventArgs with the new document.</param>
        [AutoWire(nameof(IDatabaseUnlockViewModel.DocumentReady))]
        public void DocumentReadyHandler(object sender, DocumentReadyEventArgs e)
        {
            Frame.Navigate(
                typeof(DatabaseParentView),
                new NavigationParameter(
                    new {
                        file = e.Candidate.File,
                        fileIsSample = ViewModel.IsSampleFile,
                        document = e.Document,
                        rng = e.Rng,
                        persistenceService = e.PersistenceService,
                        masterKeyViewModel = e.KeyChangeViewModel
                    }
                )
            );
        }

        /// <summary>
        /// Auto-event handler for the ViewModel's CredentialStorageFailed event.
        /// Attempts to re-store credentials after prompting the user to clear some.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">Deferrable EventArgs that allow for the retry.</param>
        [AutoWire(nameof(IDatabaseUnlockViewModel.CredentialStorageFailed))]
        public async void CredentialStorageFailedHandler(object sender, CredentialStorageFailureEventArgs e)
        {
            using (e.GetDeferral())
            {
                // Prompt the user to delete a credential, then try again
                await new PasswordManagementDialog(
                    await e.GetSavedCredentialsViewModelAsync(),
                    GetString(CredentialStorageFullResourceKey)
                ).ShowAsync();

                if (!await e.RetryStorage())
                {
                    // Show a dialog explaining that we still can't store the credential
                    MessageDialog failureDialog = new MessageDialog(
                        GetString(CredentialsStorageFailedResourceKey),
                        GetString(CredentialsStorageFailedTitleResourceKey)
                    )
                    {
                        Options = MessageDialogOptions.None
                    };

                    failureDialog.Commands.Add(
                        new UICommand(GetString("OK"))
                    );

                    await failureDialog.ShowAsync();
                }
            } 
        }

        #endregion
    }
}
