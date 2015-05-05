﻿using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using System;
using Windows.System;
using Windows.UI.Xaml;

namespace PassKeep.Framework
{
    /// <summary>
    /// The base Page type used by PassKeep for contents of the root frame.
    /// </summary>
    public abstract class PassKeepPage : RootPassKeepPage
    {
        protected const string SavingResourceKey = "Saving";

        /// <summary>
        /// The thinnest possible view of an app ("snap" width in Windows 8).
        /// </summary>
        public const int SnapWidth = 320;

        /// <summary>
        /// Typical minimum width for an app.
        /// </summary>
        public const int NarrowWidth = 500;

        /// <summary>
        /// Dependency property for the ClipboardClearViewModel property.
        /// </summary>
        public static readonly DependencyProperty ClipboardClearViewModelProperty
            = DependencyProperty.Register(
                "ClipboardClearViewModel",
                typeof(IClipboardClearTimerViewModel),
                typeof(PassKeepPage),
                new PropertyMetadata(null)
                );

        /// <summary>
        /// Bootstraps the NavigationHelper.
        /// </summary>
        protected PassKeepPage()
            : base()
        { }

        /// <summary>
        /// The ViewModel used to track interaction time with the clipboard.
        /// </summary>
        public IClipboardClearTimerViewModel ClipboardClearViewModel
        {
            get { return (IClipboardClearTimerViewModel)GetValue(ClipboardClearViewModelProperty); }
            set { SetValue(ClipboardClearViewModelProperty, value); }
        }

        /// <summary>
        /// Invoked when the Page needs to communicate that it has started a load
        /// operation.
        /// </summary>
        public event EventHandler<LoadingStartedEventArgs> StartedLoading;

        protected void RaiseStartedLoading(LoadingStartedEventArgs args)
        {
            StartedLoading?.Invoke(this, args);
        }

        /// <summary>
        /// Invoked when the Page needs to communicate that it has finished
        /// a load operation.
        /// </summary>
        public event EventHandler DoneLoading;

        protected void RaiseDoneLoading()
        {
            DoneLoading?.Invoke(this, EventArgs.Empty);
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
        /// Handles the specified accelerator (Ctrl-modified) key.
        /// </summary>
        /// <param name="key">The hotkey to handle.</param>
        /// <returns>Whether this page can handle the provided hotkey.</returns>
        public virtual bool HandleAcceleratorKey(VirtualKey key)
        {
            return false;
        }
    }
}
