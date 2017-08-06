// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Infrastructure;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

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
                    Converter = (IValueConverter)Application.Current.Resources["BooleanToVisibilityConverter"],
                    Mode = BindingMode.OneWay
                }
            );

            control.IsEnabledChanged += (s, e) =>
            {
                Dbg.Trace("IsEnabledChanged");
            };
        }

        /// <summary>
        /// Given event args for a RightTapped event on a FrameworkElement, shows an attached MenuFlyout at the tap point.
        /// </summary>
        /// <param name="sender">The tapped element.</param>
        /// <param name="tapEventArgs">Args for the right-tap event.</param>
        public static void ShowAttachedMenuAsContextMenu(this FrameworkElement sender, RightTappedRoutedEventArgs tapEventArgs)
        {
            Point tapOffset = tapEventArgs.GetPosition(sender);

            MenuFlyout flyout = MenuFlyout.GetAttachedFlyout(sender) as MenuFlyout;
            Dbg.Assert(flyout != null);

            flyout.ShowAt(sender, tapOffset);
        }

    }
}
