using PassKeep.Lib.Contracts.Models;
using PassKeep.Models;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PassKeep.Views.Controls
{
    /// <summary>
    /// Represents a horizontally scrolling list of Groups ("Breadcrumbs") leading up to the current
    /// group. An event is provided for clicked groups.
    /// </summary>
    public sealed partial class BreadcrumbNavigator : UserControl
    {
        public event EventHandler<GroupClickedEventArgs> GroupClicked;
        private void RaiseGroupClicked(IKeePassGroup group)
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
            Debug.Assert(e.ClickedItem is Breadcrumb);
            RaiseGroupClicked((e.ClickedItem as Breadcrumb).Group);
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
