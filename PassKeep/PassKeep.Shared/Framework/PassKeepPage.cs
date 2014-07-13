﻿using PassKeep.Common;
using PassKeep.Lib.EventArgClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Framework
{
    /// <summary>
    /// The base Page type used by PassKeep.
    /// </summary>
    public abstract class PassKeepPage : Page
    {
        /// <summary>
        /// The thinnest possible view of an app ("snap" width in Windows 8).
        /// </summary>
        public const int SnapWidth = 320;

        /// <summary>
        /// Typical minimum width for an app.
        /// </summary>
        public const int NarrowWidth = 500;

        protected readonly NavigationHelper navigationHelper;

        private static readonly Action NoOp = () => { };

        /// <summary>
        /// Bootstraps the NavigationHelper.
        /// </summary>
        protected PassKeepPage()
        {
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            this.Loaded += PassKeepPage_Loaded;
            this.Unloaded += PassKeepPage_Unloaded;
        }

        /// <summary>
        /// Invoked when the Page needs to communicate that it has started a load
        /// operation.
        /// </summary>
        public event EventHandler<LoadingStartedEventArgs> StartedLoading;

        protected void RaiseStartedLoading(LoadingStartedEventArgs args)
        {
            if (StartedLoading != null)
            {
                StartedLoading(this, args);
            }
        }

        /// <summary>
        /// Invoked when the Page needs to communicate that it has finished
        /// a load operation.
        /// </summary>
        public event EventHandler DoneLoading;

        protected void RaiseDoneLoading()
        {
            if (DoneLoading != null)
            {
                DoneLoading(this, new EventArgs());
            }
        }

        /// <summary>
        /// Accesses the ContainerHelper for this page.
        /// </summary>
        public ContainerHelper ContainerHelper
        {
            get;
            internal set;
        }

        /// <summary>
        /// Accesses the NavigationHelper for this page.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Handles the specified accelerator (Ctrl-modified) key.
        /// </summary>
        /// <param name="key">The hotkey to handle.</param>
        /// <returns>Whether this page can handle the provided hotkey.</returns>
        public abstract bool HandleAcceleratorKey(VirtualKey key);

        /// <summary>
        /// Callback for the NavigationHelper's LoadState event.
        /// </summary>
        /// <param name="sender">The NavigationHelper.</param>
        /// <param name="e">EventArgs for the event.</param>
        protected abstract void navigationHelper_LoadState(object sender, LoadStateEventArgs e);

        /// <summary>
        /// Callback for the NavigationHelper's SaveState event.
        /// </summary>
        /// <param name="sender">The NavigationHelper.</param>
        /// <param name="e">EventArgs for the event.</param>
        protected abstract void navigationHelper_SaveState(object sender, SaveStateEventArgs e);

        /// <summary>
        /// Alerts the NavigationHelper that this Page has been navigated to.
        /// </summary>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        protected sealed override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        /// <summary>
        /// Alerts the NavigationHelper that this Page has been navigated from.
        /// </summary>
        /// <param name="e">NavigationEventArgs for the navigation.</param>
        protected sealed override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Updates the VisualStateManager's state based on a new window size.
        /// </summary>
        /// <param name="newWindowSize">The size to base the state on.</param>
        protected virtual void SetVisualState(Size windowSize)
        {
            Debug.WriteLine("Performing no-op for SetVisualState...");
        }

        /// <summary>
        /// Displays a file picker in the Documents library with any extension.
        /// </summary>
        /// <param name="gotFileCallback">Callback to invoke with the picked file.</param>
        /// <param name="cancelledCallback">Callback to invoke if the user pressed 'cancel'.</param>
        protected async Task PickFile(Action<StorageFile> gotFileCallback, Action cancelledCallback)
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // Not all databases end in .kdbx
            picker.FileTypeFilter.Add("*");

            StorageFile pickedFile = await picker.PickSingleFileAsync();
            if (pickedFile == null)
            {
                Debug.WriteLine("User cancelled the file picker.");
                cancelledCallback();
            }
            else
            {
                Debug.WriteLine("User selected a file via the picker.");
                gotFileCallback(pickedFile);
            }
        }

        /// <summary>
        /// Displays a file picker in the Documents library with any extension.
        /// </summary>
        /// <param name="gotFileCallback">Callback to invoke with the picked file.</param>
        protected async Task PickFile(Action<StorageFile> gotFileCallback)
        {
            await PickFile(gotFileCallback, PassKeepPage.NoOp);
        }

        /// <summary>
        /// Called when the Page is loaded by the framework.
        /// </summary>
        /// <param name="sender">This page.</param>
        /// <param name="e">EventArgs for the load.</param>
        private void PassKeepPage_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += HandleSizeChange;
        }

        /// <summary>
        /// Called when the Page is unloaded by the framework.
        /// </summary>
        /// <param name="sender">This page.</param>
        /// <param name="e">EventArgs for the unload.</param>
        private void PassKeepPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= HandleSizeChange;
        }

        /// <summary>
        /// Handles window size changes.
        /// </summary>
        /// <param name="sender">The resizing Window.</param>
        /// <param name="e">EventArgs for the resize.</param>
        private void HandleSizeChange(object sender, WindowSizeChangedEventArgs e)
        {
            SetVisualState(e.Size);
        }
    }
}
