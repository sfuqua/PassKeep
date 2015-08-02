using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace PassKeep.Views.Controls
{
    /// <summary>
    /// A simple help icon that shows text when clicked.
    /// </summary>
    public sealed partial class HelpIcon : UserControl
    {
        /// <summary>
        /// The text shown in a flyout when this control is clicked.
        /// </summary>
        public static DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(HelpIcon),
            PropertyMetadata.Create(String.Empty)
        );

        private CoreCursor defaultCursor;

        public HelpIcon()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Helper property for <see cref="TextProperty"/>.
        /// </summary>
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Updates the cursor when over the help icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            defaultCursor = Window.Current.CoreWindow.PointerCursor;
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Help, 1);
        }

        /// <summary>
        /// Updates the cursor when not over the help icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = defaultCursor;
        }

        /// <summary>
        /// Shows the help flyout.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
