using System;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Infrastructure;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using PassKeep.Lib.Contracts.Models;

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

        private async void Group_Drop(object sender, DragEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            Dbg.Assert(senderElement != null);

            IDatabaseGroupViewModel groupVm = senderElement.DataContext as IDatabaseGroupViewModel;
            Dbg.Assert(groupVm != null);

            IKeePassGroup thisGroup = groupVm.Node as IKeePassGroup;
            Dbg.Assert(thisGroup != null);

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

        private async void Group_DragEnter(object sender, DragEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            Dbg.Assert(senderElement != null);

            IDatabaseGroupViewModel groupVm = senderElement.DataContext as IDatabaseGroupViewModel;
            Dbg.Assert(groupVm != null);

            IKeePassGroup thisGroup = groupVm.Node as IKeePassGroup;
            Dbg.Assert(thisGroup != null);

            DragOperationDeferral deferral = e.GetDeferral();

            string text = await e.DataView.GetTextAsync();
            if (!String.IsNullOrWhiteSpace(text) && thisGroup.CanAdopt(text))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
                e.Handled = true;
            }

            deferral.Complete();
        }
    }
}
