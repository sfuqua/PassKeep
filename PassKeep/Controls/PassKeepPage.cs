using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.EventArgClasses;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Search;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Controls
{
    /// <summary>
    /// An abstract base class for all PassKeep Views
    /// </summary>
    /// <remarks></remarks>
    public abstract class PassKeepPage : Page
    {
        protected NavigationHelper navHelper;

        public ContainerHelper ContainerHelper
        {
            get;
            set;
        }

        public PassKeepPage()
        {
            this.navHelper = new NavigationHelper(this);
            this.navHelper.LoadState += navHelper_LoadState;
            this.navHelper.SaveState += navHelper_SaveState;
            SizeChanged += handleSizeChange;
        }

        protected void handleSizeChange(object sender, SizeChangedEventArgs e)
        {
            // TODO: Need some virtual/abstract way to handle re-layout
            // Also, appbar buttons
        }

         protected virtual void TryGoBack()
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        protected virtual void TryGoForward()
        {
            if (Frame.CanGoForward)
            {
                Frame.GoForward();
            }
        }

        protected virtual void navHelper_LoadState(object sender, LoadStateEventArgs e)
        { }

        protected virtual void navHelper_SaveState(object sender, SaveStateEventArgs e)
        { }

        /// <summary>
        /// An Event for a parent View/control to be notified when this View
        /// has begun a loading event.
        /// </summary>
        public event EventHandler<LoadingStartedEventArgs> StartedLoading;

        /// <summary>
        /// Called by a View to switch into a "loading" state
        /// </summary>
        /// <param name="text"></param>
        /// <param name="cancel"></param>
        /// <param name="indeterminate"></param>
        protected void onStartedLoading(string text, Action cancel, bool indeterminate = true)
        {
            if (StartedLoading != null)
            {
                StartedLoading(this, new LoadingStartedEventArgs(text, cancel, indeterminate));
            }
        }

        /// <summary>
        /// An Event for a parent View/control to be notified when this View
        /// has completed a loading event.
        /// </summary>
        public event EventHandler DoneLoading;
        protected void onDoneLoading()
        {
            if (DoneLoading != null)
            {
                DoneLoading(this, new EventArgs());
            }
        }

        public virtual Task<bool> HandleHotKey(VirtualKey key)
        {
            return Task.Run(() => false);
        }

        public virtual Task<bool> HandleDelete()
        {
            return Task.Run(() => false);
        }

        public virtual void HandleGenericKey(VirtualKey key) { }

        /// <summary>
        /// Whether this View should open the Search charm on keydown.
        /// </summary>
        public virtual bool SearchOnKeypress
        {
            get { return false; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navHelper.OnNavigatedFrom(e);
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            navHelper.OnNavigatedTo(e);

            SearchPane.GetForCurrentView().ShowOnKeyboardInput = SearchOnKeypress;
        }
    }
}
