using System;
using System.Diagnostics;
using System.Windows.Input;
using PassKeep.Common;
using PassKeep.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

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

        public static readonly DependencyProperty ClipboardViewModelProperty =
            DependencyProperty.Register("ClipboardViewModel", typeof(EntryPreviewClipboardViewModel), typeof(BasicEntryControl), PropertyMetadata.Create(clipboardDefaultValue, clipboardVmChanged));
        public EntryPreviewClipboardViewModel ClipboardViewModel
        {
            get { return (EntryPreviewClipboardViewModel)GetValue(ClipboardViewModelProperty); }
            set { SetValue(ClipboardViewModelProperty, value); }
        }

        private static object clipboardDefaultValue()
        {
            return null;
        }

        private static void clipboardVmChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BasicEntryControl thisControl = (BasicEntryControl)o;
            if (e.OldValue != null)
            {
                ((EntryPreviewClipboardViewModel)e.OldValue).TimerComplete -= thisControl.onClipboardTimerFinished;
            }
            if (e.NewValue != null)
            {
                ((EntryPreviewClipboardViewModel)e.NewValue).TimerComplete += thisControl.onClipboardTimerFinished;
            }
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

        public BasicEntryControl()
        {
            this.InitializeComponent();
        }

        private void onClipboardTimerFinished(object sender, ClipboardTimerCompleteEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            e.Handled = true;

            // Clear the clipboard
            try
            {
                Clipboard.Clear();
            }
            catch (Exception)
            {
                setClipboardTimerError();
            }
        }

        private async void setClipboardTimerError()
        {
            var dialog = new MessageDialog("PassKeep was unable to automatically clear your clipboard because another app was in use.", "Clear Clipboard?")
            {
                Options = MessageDialogOptions.None
            };

            IUICommand clearCommand = new UICommand("Clear Now");
            IUICommand cancelCmd = new UICommand("Cancel");

            dialog.Commands.Add(clearCommand);
            dialog.Commands.Add(cancelCmd);

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            IUICommand chosenCmd = await dialog.ShowAsync();
            if (chosenCmd == clearCommand)
            {
                Clipboard.Clear();
            }
        }

        private void UsernameCopied(object sender, EventArgs e)
        {
            Debug.Assert(ClipboardViewModel != null);
            if (ClipboardViewModel == null)
            {
                throw new InvalidOperationException();
            }

            ClipboardViewModel.StartTimer(ClipboardTimerTypes.UserName);
        }

        private void PasswordCopied(object sender, EventArgs e)
        {
            Debug.Assert(ClipboardViewModel != null);
            if (ClipboardViewModel == null)
            {
                throw new InvalidOperationException();
            }

            ClipboardViewModel.StartTimer(ClipboardTimerTypes.Password);
        }
    }
}
