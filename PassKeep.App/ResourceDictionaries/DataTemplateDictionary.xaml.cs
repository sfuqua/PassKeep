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

        private void Border_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Dbg.Assert(element != null);

            element.ShowAttachedMenuAsContextMenu(e);
        }
    }
}
