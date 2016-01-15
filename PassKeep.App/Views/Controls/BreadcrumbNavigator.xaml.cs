using PassKeep.Lib.Contracts.Models;
using PassKeep.Models;
using SariphLib.Infrastructure;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
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

        private async void Breadcrumb_Drop(object sender, DragEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            Dbg.Assert(senderElement != null);

            Breadcrumb thisBreadcrumb = senderElement.DataContext as Breadcrumb;
            Dbg.Assert(thisBreadcrumb != null);

            IKeePassGroup thisGroup = thisBreadcrumb.Group;
            DragOperationDeferral deferral = e.GetDeferral();

            string encodedUuid = await e.DataView.GetTextAsync();
            if (thisGroup.TryAdopt(encodedUuid))
            {
                Dbg.Trace($"Successfully moved node {encodedUuid} to new parent {thisGroup.Uuid.EncodedValue}");
                e.AcceptedOperation = DataPackageOperation.Move;
            }
            else
            {
                Dbg.Trace($"WARNING: Unable to locate dropped node {encodedUuid}");
                e.AcceptedOperation = DataPackageOperation.None;
            }

            e.Handled = true;
            deferral.Complete();
        }

        private async void Breadcrumb_DragEnter(object sender, DragEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            Dbg.Assert(senderElement != null);

            Breadcrumb thisBreadcrumb = senderElement.DataContext as Breadcrumb;
            Dbg.Assert(thisBreadcrumb != null);

            IKeePassGroup thisGroup = thisBreadcrumb.Group;
            DragOperationDeferral deferral = e.GetDeferral();

            string text = await e.DataView.GetTextAsync();
            if (!String.IsNullOrWhiteSpace(text))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
                e.Handled = true;
            }

            deferral.Complete();
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
