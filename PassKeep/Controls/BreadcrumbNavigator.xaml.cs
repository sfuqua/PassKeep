using System;
using System.Diagnostics;
using PassKeep.Models.Abstraction;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Controls
{
    public sealed partial class BreadcrumbNavigator : UserControl
    {
        public event EventHandler<GroupClickedEventArgs> GroupClicked;
        private void onGroupClicked(IKeePassGroup group)
        {
            if (GroupClicked != null)
            {
                GroupClicked(this, new GroupClickedEventArgs(group));
            }
        }

        public BreadcrumbNavigator()
        {
            this.InitializeComponent();
        }

        private void breadcrumbs_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.Assert(e.ClickedItem is IKeePassGroup);
            onGroupClicked(e.ClickedItem as IKeePassGroup);
        }
    }

    public class GroupClickedEventArgs : EventArgs
    {
        public IKeePassGroup Group { get; set; }
        public GroupClickedEventArgs(IKeePassGroup group)
        {
            Group = group;
        }
    }
}
