using PassKeep.Lib.Contracts.Models;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Views.TemplateSelectors
{
    public abstract class EntryDataTemplateSelector : DataTemplateSelector
    {
        protected abstract string suffix { get; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            if (element == null)
            {
                return null;
            }

            DataTemplate groupTemplate = (DataTemplate)Application.Current.Resources["GroupTemplate" + suffix];
            DataTemplate entryTemplate = (DataTemplate)Application.Current.Resources["EntryTemplate" + suffix];

            IKeePassNode node = item as IKeePassNode;
            if (node == null)
            {
                return null;
            }

            IKeePassEntry entry = node as IKeePassEntry;
            if (entry == null)
            {
                // Group only
                return groupTemplate;
            }
            else
            {
                // Also an newActiveEntry
                return entryTemplate;
            }
        }
    }

    public class FullEntryDataTemplateSelector : EntryDataTemplateSelector
    {
        protected override string suffix
        {
	        get { return string.Empty; }
        }
    }

    public class SnappedEntryDataTemplateSelector : EntryDataTemplateSelector
    {
        protected override string suffix
        {
            get { return "_Snapped"; }
        }
    }
}
