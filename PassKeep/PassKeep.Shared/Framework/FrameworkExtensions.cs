using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace PassKeep.Framework
{
    /// <summary>
    /// Handles helper extension functions for the XAML framework.
    /// </summary>
    public static class FrameworkExtensions
    {
        /// <summary>
        /// Binds a Control's Visibility property to its IsEnabled property from code-behind.
        /// </summary>
        /// <param name="control">The control to hide when disabled and show when enabled.</param>
        /// <remarks>Depends on BooleanToVisibilityConverter being defined as an app resource.</remarks>
        public static void HideWhenDisabled(this Control control)
        {
            control.ClearValue(Control.VisibilityProperty);
            control.SetBinding(
                Control.VisibilityProperty,
                new Binding
                {
                    Source = control,
                    Path = new PropertyPath("IsEnabled"),
                    Converter = (IValueConverter)App.Current.Resources["BooleanToVisibilityConverter"],
                    Mode = BindingMode.OneWay
                }
            );

            control.IsEnabledChanged += (s, e) =>
            {
                Debug.WriteLine("IsEnabledChanged");
            };
        }
    }
}
