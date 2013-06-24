using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Search;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SariphLib.MVVM;

namespace PassKeep.Lib.Services
{
    public class NavigationService : BindableBase, INavigate
    {
        private Frame rootFrame;
        private Stack<Tuple<LayoutAwarePage, object>> backStack;
        private Stack<Tuple<LayoutAwarePage, object>> forwardStack;

        public LayoutAwarePage LastPage
        {
            get;
            private set;
        }

        private object currentParameter
        {
            get;
            set;
        }
        public LayoutAwarePage CurrentPage
        {
            get
            {
                return (LayoutAwarePage)rootFrame.Content;
            }
        }

        public NavigationMode LastNavigationMode
        {
            get;
            private set;
        }

        public bool CanGoBack
        {
            get
            {
                return backStack.Count > 0 || BackstackOverride;
            }
        }

        private bool _backstackOverride;
        public bool BackstackOverride
        {
            get { return _backstackOverride; }
            set
            {
                _backstackOverride = value;
                OnPropertyChanged("CanGoBack");
            }
        }

        public bool CanGoForward
        {
            get
            {
                return forwardStack.Count > 0;
            }
        }

        public event EventHandler NavigationComplete;
        private void onNavigationComplete()
        {
            if (NavigationComplete != null)
            {
                NavigationComplete(this, new EventArgs());
            }
        }

        #region Static magic

        private static Dictionary<Frame, NavigationService> lookup = new Dictionary<Frame,NavigationService>();
        public static NavigationService ForFrame(Frame frame)
        {
            if (lookup.ContainsKey(frame))
            {
                return lookup[frame];
            }

            lookup[frame] = new NavigationService(frame);
            return lookup[frame];
        }

        #endregion

        private NavigationService(Frame rootFrame)
        {
            this.rootFrame = rootFrame;
            backStack = new Stack<Tuple<LayoutAwarePage, object>>();
            forwardStack = new Stack<Tuple<LayoutAwarePage, object>>();
            LastPage = null;
        }

        public bool Navigate(Type targetPageType)
        {
            return Navigate(targetPageType, null);
        }

        public bool Navigate(Type targetPageType, object parameter, bool updateBackStack = true, bool nukeForwardStack = true, NavigationMode mode = NavigationMode.New)
        {
            SearchPane.GetForCurrentView().ShowOnKeyboardInput = false;

            LayoutAwarePage lastPage = CurrentPage;
            LastNavigationMode = mode;
            LastPage = CurrentPage;
            if (rootFrame.Navigate(targetPageType, parameter))
            {
                if (updateBackStack && lastPage != null)
                {
                    backStack.Push(new Tuple<LayoutAwarePage, object>(lastPage, currentParameter));
                    OnPropertyChanged("CanGoBack");
                }

                if (nukeForwardStack)
                {
                    forwardStack.Clear();
                    OnPropertyChanged("CanGoForward");
                }

                currentParameter = parameter;
                onNavigationComplete();
                return true;
            }

            onNavigationComplete();
            return false;
        }

        public bool ReplacePage(Type targetPageType, object parameter = null, NavigationMode mode = NavigationMode.New)
        {
            return Navigate(targetPageType, parameter, false, false, mode);
        }

        public bool GoBack()
        {
            if (!CanGoBack)
            {
                return false;
            }

            if (BackstackOverride)
            {
                return false;
            }

            Tuple<LayoutAwarePage, object> entry = backStack.Pop();
            OnPropertyChanged("CanGoBack");

            forwardStack.Push(new Tuple<LayoutAwarePage, object>(CurrentPage, currentParameter));
            OnPropertyChanged("CanGoForward");

            ReplacePage(entry.Item1.GetType(), entry.Item2, NavigationMode.Back);

            return true;
        }

        public bool GoForward()
        {
            if (!CanGoForward)
            {
                return false;
            }

            Tuple<LayoutAwarePage, object> entry = forwardStack.Pop();
            OnPropertyChanged("CanGoForward");

            backStack.Push(new Tuple<LayoutAwarePage, object>(CurrentPage, currentParameter));
            OnPropertyChanged("CanGoBack");

            ReplacePage(entry.Item1.GetType(), entry.Item2, NavigationMode.Forward);

            return true;
        }
    }
}
