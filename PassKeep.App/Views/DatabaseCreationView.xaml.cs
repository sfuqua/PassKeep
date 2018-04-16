// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Framework;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.Services;
using PassKeep.ViewBases;
using SariphLib.Diagnostics;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Views
{
    /// <summary>
    /// View used to create a new database file.
    /// </summary>
    public sealed partial class DatabaseCreationView : DatabaseCreationViewBase
    {
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
                e.Writer,
                await DatabaseCandidateFactory.AssembleAsync(ViewModel.File),
                SyncContext,
                await ViewModel.File.CheckWritableAsync());

            Frame.Navigated -= FrameNavigated;
            Frame.Navigate(
                typeof(DatabaseParentView),
                new NavigationParameter(
                    new
                    {
                        file = ViewModel.File,
                        fileIsSample = false,
                        document = e.Document,
                        rng = e.Rng,
                        persistenceService
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

            bool initialCapsLock = IsCapsLockLocked;
            DebugHelper.Trace($"Got initial caps lock state: {initialCapsLock}");
            this.MasterKeyControl.NotifyCapsLockState(initialCapsLock);

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
            await ViewModel.File.AsIStorageItem.DeleteAsync(StorageDeleteOption.PermanentDelete);
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
                this.MasterKeyControl.NotifyCapsLockState(IsCapsLockLocked);
            }
        }
    }
}
