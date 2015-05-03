using PassKeep.Common;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
        /// A command that maps to Frame.GoBack and Frame.CanGoBack.
        /// </summary>
        public readonly ICommand GoBackCommand;

        /// <summary>
        /// Bootstraps the NavigationHelper.
        /// </summary>
        /// <param name="primaryAvailable">Whether the primary commands are available immediately.</param>
        /// <param name="secondaryAvailable">Whether the secondary commands are available immediately.</param>
        protected PassKeepPage(bool primaryAvailable = true, bool secondaryAvailable = true)
            : base()
        {
            this.GoBackCommand = new RelayCommand(
                () =>
                {
                    this.Frame.GoBack();
                },
                () => this.Frame.CanGoBack
            );

            this.PrimaryCommandsImmediatelyAvailable = primaryAvailable;
            this.SecondaryCommandsImmediatelyAvailable = secondaryAvailable;

            this.Loaded += PassKeepPage_Loaded;
            this.Unloaded += PassKeepPage_Unloaded;
        }

        /// <summary>
        /// Whether the primary commands of this page are immediate without further delay.
        /// </summary>
        public bool PrimaryCommandsImmediatelyAvailable
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the secondary commands of this page are immediate without further delay.
        /// </summary>
        public bool SecondaryCommandsImmediatelyAvailable
        {
            get;
            private set;
        }

        /// <summary>
        /// The ViewModel used to track interaction time with the clipboard.
        /// </summary>
        public IClipboardClearTimerViewModel ClipboardClearViewModel
        {
            get { return (IClipboardClearTimerViewModel)GetValue(ClipboardClearViewModelProperty); }
            set { SetValue(ClipboardClearViewModelProperty, value); }
        }

        /// <summary>
        /// Raised by a subclass when it wishes to notify the root Frame that
        /// it has primary AppBar commands available.
        /// </summary>
        public event TypedEventHandler<PassKeepPage, EventArgs> PrimaryCommandsAvailable;

        /// <summary>
        /// Raises the PrimaryCommandsAvailable event.
        /// </summary>
        protected void RaisePrimaryCommandsAvailable()
        {
            Dbg.Assert(!this.PrimaryCommandsImmediatelyAvailable);
            if (PrimaryCommandsAvailable != null)
            {
                PrimaryCommandsAvailable(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raised by a subclass when it wishes to notify the root Frame that
        /// it has secondary AppBar commands available.
        /// </summary>
        public event TypedEventHandler<PassKeepPage, EventArgs> SecondaryCommandsAvailable;

        /// <summary>
        /// Raises the SecondaryCommandsAvailable event.
        /// </summary>
        protected void RaiseSecondaryCommandsAvailable()
        {
            Dbg.Assert(!this.SecondaryCommandsImmediatelyAvailable);
            if (SecondaryCommandsAvailable != null)
            {
                SecondaryCommandsAvailable(this, new EventArgs());
            }
        }

        /// <summary>
        /// Generates a list of ICommandBarElements to use for the AppBar's primary commands.
        /// </summary>
        /// <returns>The aformentioned list.</returns>
        public virtual IList<ICommandBarElement> GetPrimaryCommandBarElements()
        {
            return null;
        }

        /// <summary>
        /// Generates a list of ICommandBarElements to use for the AppBar's secondary commands.
        /// </summary>
        /// <returns>The aformentioned list.</returns>
        public virtual IList<ICommandBarElement> GetSecondaryCommandBarElements()
        {
            return null;
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
        /// Handles the specified accelerator (Ctrl-modified) key.
        /// </summary>
        /// <param name="key">The hotkey to handle.</param>
        /// <returns>Whether this page can handle the provided hotkey.</returns>
        public abstract bool HandleAcceleratorKey(VirtualKey key);

        /// <summary>
        /// Updates the VisualStateManager's state based on a new window size.
        /// </summary>
        /// <param name="newWindowSize">The size to base the state on.</param>
        protected virtual void SetVisualState(Size windowSize)
        {
            Dbg.Trace("Performing no-op for SetVisualState...");
        }

        /// <summary>
        /// Use the Window's current size to set the VisualState.
        /// </summary>
        protected void DetermineVisualState()
        {
            Rect windowBounds = Window.Current.CoreWindow.Bounds;
            SetVisualState(new Size(windowBounds.Width, windowBounds.Height));
        }

        /// <summary>
        /// Called when the Page is loaded by the framework.
        /// </summary>
        /// <param name="sender">This page.</param>
        /// <param name="e">EventArgs for the load.</param>
        private void PassKeepPage_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += HandleSizeChange;
            DetermineVisualState();
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
