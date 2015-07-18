using PassKeep.Framework;
using SariphLib.Infrastructure;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace PassKeep.ResourceDictionaries
{
    public sealed partial class DataTemplateDictionary : ResourceDictionary
    {
        public DataTemplateDictionary()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handler for right-tap events on the entry/group containers. Handles displaying the context menu.
        /// </summary>
        /// <param name="sender">The container being tapped.</param>
        /// <param name="e">EventArgs for the tap.</param>
        private void NodeRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Dbg.Assert(element != null);

            element.ShowAttachedMenuAsContextMenu(e);
        }
    }
}
