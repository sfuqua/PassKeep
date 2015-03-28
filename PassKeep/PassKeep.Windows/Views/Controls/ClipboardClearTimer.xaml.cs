using PassKeep.Lib.Contracts.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Views.Controls
{
    /// <summary>
    /// A control that binds to a ClipboardClearTimer ViewModel and displays the amount of time
    /// remaining on the clear timer.
    /// </summary>
    public sealed partial class ClipboardClearTimer : UserControl
    {
        public ClipboardClearTimer()
        {
            this.InitializeComponent();
        }

        public void UserControl_DataContextChanged(FrameworkElement element, DataContextChangedEventArgs e)
        {

        }
    }

    /// <summary>
    /// Converts the clear timer's ViewModel into whether the control is visible or not.
    /// </summary>
    public sealed class ClipboardClearTimerEnabledToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IClipboardClearTimerViewModel viewModel = value as IClipboardClearTimerViewModel;
            if (viewModel == null)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a double from 0-1 
    /// </summary>
    /*public sealed class ClipboardClearTimerToLabelConverter : IValueConverter
    {

    }*/
}
