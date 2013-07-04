using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System;
using PassKeep.Common;
using PassKeep.ViewModels;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using PassKeep.Models;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Search;

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

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            bool gotVm = false;
            if (pageState != null)
            {
                if (pageState.ContainsKey("ViewModel"))
                {
                    ViewModel = (TViewModel)pageState["ViewModel"];
                    gotVm = true;
                }
            }

            if (!gotVm)
            {
                ViewModel = navigationParameter as TViewModel;
                if (ViewModel == null)
                {
                    throw new ArgumentException();
                }
            }

            DataContext = ViewModel;
        }

        protected override void SaveState(Dictionary<string, object> pageState)
        {
            pageState["ViewModel"] = ViewModel;
            BottomAppBar.IsSticky = false;
            BottomAppBar.IsOpen = false;
        }
    }

    public abstract class PassKeepPage : LayoutAwarePage
    {
        public PassKeepPage()
        {
            SizeChanged += (s, e) =>
            {
                RefreshAppBarButtons();
            };
        }

        public NavigationService Navigator
        {
            get
            {
                if (Frame == null)
                {
                    return null;
                }
                return NavigationService.ForFrame(Frame);
            }
        }

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

        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            Navigator.GoBack();
        }

        protected override void GoForward(object sender, RoutedEventArgs e)
        {
            Navigator.GoForward();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Navigator.BackstackOverride = false;
            PreviousWasUnsafe = IsUnsafe;
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SearchPane.GetForCurrentView().ShowOnKeyboardInput = SearchOnType;
            if (!IsProtected || !PreviousWasUnsafe)
            {
                return;
            }

            EventHandler doLock = null;
            doLock = (s, e_i) =>
            {
                ((NavigationService)s).NavigationComplete -= doLock;
                Lock();
            };

            // Basically we want to find out if we're navigating
            // forward/back from a non-protected page, and then lock.
            switch(Navigator.LastNavigationMode)
            {
                case NavigationMode.Back:
                    Navigator.NavigationComplete += doLock;
                    break;
                case NavigationMode.Forward:
                    Navigator.NavigationComplete += doLock;
                    break;
                case NavigationMode.New:
                    break;
                case NavigationMode.Refresh:
                    break;
                default:
                    Debug.Assert(false);
                    break;
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
