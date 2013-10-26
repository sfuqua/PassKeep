using PassKeep.Common;
using PassKeep.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Search;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Controls
{
    public abstract class PassKeepPage<TViewModel> : PassKeepPage
        where TViewModel : ViewModelBase
    {
        public TViewModel ViewModel
        {
            get;
            protected set;
        }

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

        protected override void navHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["ViewModel"] = ViewModel;
            BottomAppBar.IsSticky = false;
            BottomAppBar.IsOpen = false;
        }
    }

    public abstract class PassKeepPage : Page
    {
        protected NavigationHelper navHelper;

        public PassKeepPage()
        {
            this.navHelper = new NavigationHelper(this);
            this.navHelper.LoadState += navHelper_LoadState;
            this.navHelper.SaveState += navHelper_SaveState;

            SizeChanged += (s, e) =>
            {
                RefreshAppBarButtons();
            };
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

        public event EventHandler<LoadingStartedEventArgs> StartedLoading;
        protected void onStartedLoading(string text, Action cancel, bool indeterminate = true)
        {
            if (StartedLoading != null)
            {
                StartedLoading(this, new LoadingStartedEventArgs(text, cancel, indeterminate));
            }
        }

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

        public void RefreshAppBarButtons()
        {
            CustomAppBarButtons.Clear();
            LeftCommands.Visibility = Visibility.Visible;

            switch (ApplicationView.Value)
            {
                case ApplicationViewState.Filled:
                    goto default;
                case ApplicationViewState.FullScreenLandscape:
                    goto default;
                case ApplicationViewState.FullScreenPortrait:
                    SetupPortraitAppBarButtons();
                    break;
                case ApplicationViewState.Snapped:
                    SetupSnappedAppBarButtons();
                    break;
                default:
                    SetupDefaultAppBarButtons();
                    break;
            }
        }

        /// <summary>
        /// Called whenever fullscreen view is entered.
        /// Serves as a baseline for other views.
        /// </summary>
        public virtual void SetupDefaultAppBarButtons()
        {

        }

        /// <summary>
        /// Called whenever portrait view is entered.
        /// Defaults to regular configuration.
        /// </summary>
        public virtual void SetupPortraitAppBarButtons()
        {
            SetupDefaultAppBarButtons();
        }

        /// <summary>
        /// Called whenever snapped view is entered.
        /// Defaults to portrait configuration.
        /// </summary>
        public virtual void SetupSnappedAppBarButtons()
        {
            SetupPortraitAppBarButtons();
        }

        public abstract bool IsProtected
        {
            get;
        }
        public virtual bool IsUnsafe
        {
            get { return !IsProtected; }
        }
        protected static bool PreviousWasUnsafe = false;

        public virtual Task<bool> Lock()
        {
            return Task.Run(() => false);
        }

        public virtual bool SearchOnType
        {
            get { return false; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navHelper.OnNavigatedFrom(e);
            PreviousWasUnsafe = IsUnsafe;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            navHelper.OnNavigatedTo(e);

            SearchPane.GetForCurrentView().ShowOnKeyboardInput = SearchOnType;
            if (!IsProtected || !PreviousWasUnsafe)
            {
                return;
            }
        }

        private ItemsControl _rightCommands;
        public ItemsControl RightCommands
        {
            set
            {
                _rightCommands = value;
                _rightCommands.ItemsSource = CustomAppBarButtons;
            }
        }

        private StackPanel dummyPanel = new StackPanel();
        private StackPanel actualPanel;
        public StackPanel LeftCommands
        {
            protected get
            {
                return (actualPanel == null ? dummyPanel : actualPanel);
            }
            set
            {
                actualPanel = value;
            }
        }

        protected ObservableCollection<Button> CustomAppBarButtons
            = new ObservableCollection<Button>();
    }

    public class CancelableEventArgs : EventArgs
    {
        public Action Cancel
        {
            get;
            set;
        }

        public CancelableEventArgs(Action cancel)
        {
            Cancel = cancel;
        }
    }

    public class LoadingStartedEventArgs : CancelableEventArgs
    {
        public bool Indeterminate
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }


        public LoadingStartedEventArgs(string text, Action cancel, bool indeterminate = true)
            : base(cancel)
        {
            Indeterminate = indeterminate;
            Text = text;
        }
    }
}
