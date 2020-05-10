// Copyright 2017 Steven Fuqua
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
using MUXC = Microsoft.UI.Xaml.Controls;

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

            GroupTreeViewNode groupNode = senderElement.DataContext as GroupTreeViewNode;
            IDatabaseGroupViewModel groupVm = groupNode?.GroupViewModel ?? senderElement.DataContext as IDatabaseGroupViewModel;
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

            GroupTreeViewNode groupNode = senderElement.DataContext as GroupTreeViewNode;
            IDatabaseGroupViewModel groupVm = groupNode?.GroupViewModel ?? senderElement.DataContext as IDatabaseGroupViewModel;
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

        private void RelativePanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {

        }
    }

    /// <summary>
    /// Slim wrapper that allows using <see cref="IDatabaseGroupViewModel"/> in a TreeView.
    /// </summary>
    public sealed class GroupTreeViewNode: MUXC.TreeViewNode
    {
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            "IsLoading", typeof(bool), typeof(GroupTreeViewNode),
            PropertyMetadata.Create(false)
        );

        public GroupTreeViewNode(IDatabaseGroupViewModel group) : base()
        {
            Content = group;
            GroupViewModel = group;
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public IDatabaseGroupViewModel GroupViewModel { get; }

        public IKeePassGroup Group => (IKeePassGroup)GroupViewModel.Node;
    }

    /// <summary>
    /// Slim wrapper that allows using <see cref="IDatabaseEntryViewModel"/> in a TreeView.
    /// </summary>
    public sealed class EntryTreeViewNode : MUXC.TreeViewNode
    {
        public EntryTreeViewNode(IDatabaseEntryViewModel entry) : base()
        {
            Content = entry;
            EntryViewModel = entry;
        }

        public IDatabaseEntryViewModel EntryViewModel { get; }

        public IKeePassEntry Entry => (IKeePassEntry)EntryViewModel.Node;
    }
}
