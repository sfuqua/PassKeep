using PassKeep.Common;
using PassKeep.Lib.EventArgClasses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Framework
{
    /// <summary>
    /// The base Page type used by PassKeep.
    /// </summary>
    public abstract class PassKeepPage : Page
    {
        protected readonly NavigationHelper navigationHelper;

        /// <summary>
        /// Bootstraps the NavigationHelper.
        /// </summary>
        protected PassKeepPage() : base()
        {
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
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
    }
}
