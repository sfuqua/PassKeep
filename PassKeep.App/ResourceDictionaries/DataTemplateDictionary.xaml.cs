﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Diagnostics;
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
            DebugHelper.Assert(element != null);

            element.ShowAttachedMenuAsContextMenu(e);
        }

        private async void Group_Drop(object sender, DragEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            DebugHelper.Assert(senderElement != null);

            IDatabaseGroupViewModel groupVm = senderElement.DataContext as IDatabaseGroupViewModel;
            DebugHelper.Assert(groupVm != null);

            IKeePassGroup thisGroup = groupVm.Node as IKeePassGroup;
            DebugHelper.Assert(thisGroup != null);

            DragOperationDeferral deferral = e.GetDeferral();

            string encodedUuid = await e.DataView.GetTextAsync();
            if (thisGroup.TryAdopt(encodedUuid))
            {
                DebugHelper.Trace($"Successfully moved node {encodedUuid} to new parent {thisGroup.Uuid.EncodedValue}");
                e.AcceptedOperation = DataPackageOperation.Move;
            }
            else
            {
                DebugHelper.Trace($"WARNING: Unable to locate dropped node {encodedUuid}");
                e.AcceptedOperation = DataPackageOperation.None;
            }

            e.Handled = true;
            deferral.Complete();
        }

        private async void Group_DragEnter(object sender, DragEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            DebugHelper.Assert(senderElement != null);

            IDatabaseGroupViewModel groupVm = senderElement.DataContext as IDatabaseGroupViewModel;
            DebugHelper.Assert(groupVm != null);

            IKeePassGroup thisGroup = groupVm.Node as IKeePassGroup;
            DebugHelper.Assert(thisGroup != null);

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
