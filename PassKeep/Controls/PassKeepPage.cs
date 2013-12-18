using PassKeep.Common;
using PassKeep.Lib.Contracts.ViewModels;
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
    /// Represents a page of the app that is responsible for its own ViewModel,
    /// of a known type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel for this View</typeparam>
    public abstract class PassKeepPage<TViewModel> : PassKeepPage
        where TViewModel : class, IViewModel
    {
        /// <summary>
        /// Provides access to the ViewModel for this View
        /// </summary>
        public TViewModel ViewModel
        {
            get;
            protected set;
        }

        /// <summary>
        /// Loads the ViewModel from page state if it exists, otherwise tries to 
        /// cast one from the NavigationParameter.
        /// </summary>
        /// <remarks>Called by the View's NavigationHelper</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            bool gotVm = false;
            if (e.PageState != null)
            {
                if (e.PageState.ContainsKey("ViewModel"))
                {
                    ViewModel = (TViewModel)e.PageState["ViewModel"];
                    gotVm = true;
                }
            }

            if (!gotVm)
            {
                ViewModel = e.NavigationParameter as TViewModel;
                if (ViewModel == null)
                {
                    throw new ArgumentException();
                }
            }

            DataContext = ViewModel;
        }

        /// <summary>
        /// Persists the ViewModel to the page state and closes appbars.
        /// </summary>
        /// <remarks>Called by the View's NavigationHelper</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void navHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["ViewModel"] = ViewModel;
            BottomAppBar.IsSticky = false;
            BottomAppBar.IsOpen = false;
        }
    }

    /// <summary>
    /// An abstract base class for all PassKeep Views
    /// </summary>
    /// <remarks></remarks>
    public abstract class PassKeepPage : Page
    {
        protected NavigationHelper navHelper;

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
