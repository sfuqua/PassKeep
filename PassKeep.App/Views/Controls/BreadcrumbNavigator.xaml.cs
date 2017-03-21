using PassKeep.Lib.Contracts.Models;
using PassKeep.Models;
using SariphLib.Infrastructure;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Views.Controls
{
    /// <summary>
    /// Represents a horizontally scrolling list of Groups ("Breadcrumbs") leading up to the current
    /// group. An event is provided for clicked groups.
    /// </summary>
    public sealed partial class BreadcrumbNavigator : UserControl
    {
        /// <summary>
        /// Basic constructor.
        /// </summary>
        public BreadcrumbNavigator()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Dispatched when one of the breadcrumbs is clicked by the user.
        /// </summary>
        public event EventHandler<GroupClickedEventArgs> GroupClicked;

        /// <summary>
        /// Handler for click events on breadcrumbs - invokes <see cref="GroupClicked"/>.
        /// </summary>
        /// <param name="sender">The clicked breadcrumb.</param>
        /// <param name="e">EventArgs for the click event.</param>
        private void breadcrumbs_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.Assert(e.ClickedItem is Breadcrumb);
            RaiseGroupClicked((e.ClickedItem as Breadcrumb).Group);
        }

        /// <summary>
        /// Handler for drop events on breadcrumbs. Moves the dropped node into the target group,
        /// if appropriate.
        /// </summary>
        /// <param name="sender">The element receiving the drop event.</param>
        /// <param name="e">DragEventArgs for the drop.</param>
        private async void Breadcrumb_Drop(object sender, DragEventArgs e)
        {
            // First get the group we are dropping onto...
            FrameworkElement senderElement = sender as FrameworkElement;
            Dbg.Assert(senderElement != null);

            Breadcrumb thisBreadcrumb = senderElement.DataContext as Breadcrumb;
            Dbg.Assert(thisBreadcrumb != null);

            IKeePassGroup thisGroup = thisBreadcrumb.Group;
            Dbg.Assert(thisGroup != null);

            DragOperationDeferral deferral = e.GetDeferral();

            // Get the UUID of the dropped node - if possible, move it into this group.
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

        /// <summary>
        /// Handler for DragEnter events on breadcrumbs. Updates the AcceptedOperation
        /// property of <paramref name="e"/> based on the drop target.
        /// </summary>
        /// <param name="sender">The element receiving the DragEnter event.</param>
        /// <param name="e">DragEventArgs for the drag.</param>
        private async void Breadcrumb_DragEnter(object sender, DragEventArgs e)
        {
            // Get the group we are currently over...
            FrameworkElement senderElement = sender as FrameworkElement;
            Dbg.Assert(senderElement != null);

            Breadcrumb thisBreadcrumb = senderElement.DataContext as Breadcrumb;
            Dbg.Assert(thisBreadcrumb != null);

            IKeePassGroup thisGroup = thisBreadcrumb.Group;
            Dbg.Assert(thisGroup != null);

            // Update the DataPackageOperation of the drag event based on whether
            // we are dragging a node over a group (generally, yes).
            DragOperationDeferral deferral = e.GetDeferral();

            string text = await e.DataView.GetTextAsync();
            if (!String.IsNullOrWhiteSpace(text) && thisGroup.CanAdopt(text))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
                e.Handled = true;
            }

            deferral.Complete();
        }

        /// <summary>
        /// Invokes the <see cref="GroupClicked"/> event.
        /// </summary>
        /// <param name="group">The group that was clicked.</param>
        private void RaiseGroupClicked(IKeePassGroup group)
        {
            GroupClicked?.Invoke(this, new GroupClickedEventArgs(group));
        }
    }

    /// <summary>
    /// Lightweight helped EventArgs class for clicked breadcrumb items.
    /// </summary>
    public sealed class GroupClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs an instance around <paramref name="group"/>.
        /// </summary>
        /// <param name="group">The clicked group.</param>
        public GroupClickedEventArgs(IKeePassGroup group)
        {
            Group = group;
        }

        /// <summary>
        /// The clicked group.
        /// </summary>
        public IKeePassGroup Group { get; set; }
    }
}
