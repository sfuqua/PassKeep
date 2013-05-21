using System;
using System.Diagnostics;
using PassKeep.Common;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using PassKeep.Models;
using System.Windows.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Controls
{
    public sealed partial class BasicEntryControl : UserControl
    {
        public static readonly DependencyProperty UrlCommandProperty =
            DependencyProperty.Register("UrlCommand", typeof(ICommand), typeof(BasicEntryControl), PropertyMetadata.Create(DelegateCommand.NoOp));
        public ICommand UrlCommand
        {
            get { return (ICommand)GetValue(UrlCommandProperty); }
            set { SetValue(UrlCommandProperty, value); }
        }

        public static readonly DependencyProperty ShowNotesProperty =
            DependencyProperty.Register("ShowNotes", typeof(bool), typeof(BasicEntryControl), PropertyMetadata.Create(true));
        public bool ShowNotes
        {
            get { return (bool)GetValue(ShowNotesProperty); }
            set { SetValue(ShowNotesProperty, value); }
        }

        public static readonly DependencyProperty DetailsCommandProperty =
            DependencyProperty.Register("DetailsCommand", typeof(ICommand), typeof(BasicEntryControl), PropertyMetadata.Create(DelegateCommand.NoOp));
        public ICommand DetailsCommand
        {
            get { return (ICommand)GetValue(DetailsCommandProperty); }
            set { SetValue(DetailsCommandProperty, value); }
        }

        public static readonly DependencyProperty TimerEnabledProperty =
            DependencyProperty.Register("TimerEnabled", typeof(bool), typeof(BasicEntryControl), PropertyMetadata.Create(true, timerEnabledChanged));
        private static void timerEnabledChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BasicEntryControl control = (BasicEntryControl)o;
            bool newValue = (bool)e.NewValue;
            control.PART_UsernameProgress.Visibility = (newValue ? Visibility.Visible : Visibility.Collapsed);
            control.PART_PasswordProgress.Visibility = (newValue ? Visibility.Visible : Visibility.Collapsed);
        }
        public bool TimerEnabled
        {
            get { return (bool)GetValue(TimerEnabledProperty); }
            set { SetValue(TimerEnabledProperty, value); }
        }

        public static readonly DependencyProperty TimerDelayProperty =
            DependencyProperty.Register("TimerDelay", typeof(double), typeof(BasicEntryControl), PropertyMetadata.Create(1));
        public double TimerDelay
        {
            get { return (double)GetValue(TimerDelayProperty); }
            set { SetValue(TimerDelayProperty, value); }
        }

        public static readonly DependencyProperty CollapsibleProperty =
            DependencyProperty.Register("Collapsiple", typeof(bool), typeof(BasicEntryControl), PropertyMetadata.Create(false));
        public bool Collapsiple
        {
            get { return (bool)GetValue(CollapsibleProperty); }
            set { SetValue(CollapsibleProperty, value); }
        }

        public static readonly DependencyProperty ChildrenTransitionsProperty =
            DependencyProperty.Register("ChildrenTransitions", typeof(TransitionCollection), typeof(BasicEntryControl), PropertyMetadata.Create(transitionsDefault));
        public TransitionCollection ChildrenTransitions
        {
            get { return (TransitionCollection)GetValue(TransitionsProperty); }
            set { SetValue(TransitionsProperty, value); }
        }

        private static object transitionsDefault()
        {
            return new TransitionCollection 
            {
                new EntranceThemeTransition()
            };
        }

        private Storyboard activeStoryboard;

        public BasicEntryControl()
        {
            this.InitializeComponent();
        }

        private void initStoryboard(ProgressBar progressBar)
        {
            if (TimerEnabled)
            {
                if (activeStoryboard != null)
                {
                    activeStoryboard.Completed -= clearClipboard;
                    activeStoryboard.Stop();
                }

                activeStoryboard = new Storyboard();
                DoubleAnimation animation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(TimerDelay)),
                    EnableDependentAnimation = true,
                    FillBehavior = FillBehavior.Stop
                };
                Storyboard.SetTarget(animation, progressBar);
                Storyboard.SetTargetProperty(animation, "Value");
                activeStoryboard.Children.Add(animation);
                activeStoryboard.Begin();
                activeStoryboard.Completed += clearClipboard;
            }
        }

        private async void clearClipboard(object sender, object e)
        {
            await root.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => { Clipboard.Clear(); }
            );
        }

        private void UsernameCopied(object sender, EventArgs e)
        {
            initStoryboard(PART_UsernameProgress);
        }

        private void PasswordCopied(object sender, EventArgs e)
        {
            initStoryboard(PART_PasswordProgress);
        }
    }
}
