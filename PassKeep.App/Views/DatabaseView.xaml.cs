﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Navigation;
using PassKeep.Lib.Contracts.Enums;

namespace PassKeep.Views
{
    /// <summary>
    /// The primary View over the contents of a KeePass database.
    /// </summary>
    public sealed partial class DatabaseView : DatabaseViewBase
    {
        private const string EditResourceKey = "Edit";
        private const string DeleteResourceKey = "Delete";
        private const string CreateResourceKey = "Create";

        private const string EditRenameResourceKey = "Rename";
        private const string EditDetailsResourceKey = "Details";

        private const string CreateEntryKey = "NewEntry";
        private const string CreateGroupKey = "NewGroup";

        private const string DeletePromptKey = "DeletePrompt";
        private const string DeletePromptTitleKey = "DeletePromptTitle";

        private IDatabaseNodeViewModel nodeBeingRenamed;

        public DatabaseView()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Used for CommandBar bindings.
        /// </summary>
        public GridView ChildGridView
        {
            get { return this.childGridView; }
        }

        private Flyout RenameFlyout
        {
            get
            {
                return Resources["nodeRenameFlyout"] as Flyout;
            }
        }

        #region Auto-event handlers

        /// <summary>
        /// Auto-event handler for when the user wants to rename a node.
        /// </summary>
        /// <param name="vm">The ViewModel.</param>
        /// <param name="node">The node being renamed.</param>
        [AutoWire(nameof(IDatabaseViewModel.RequestRenameNode))]
        public void RequestRenameNodeHandler(IDatabaseViewModel vm, IDatabaseNodeViewModel node)
        {
            DebugHelper.Trace($"Rename requested for node {node.Node.Title.ClearValue}");
            this.nodeBeingRenamed = node;

            TextBox inputBox = RenameFlyout.Content as TextBox;
            DebugHelper.Assert(inputBox != null);

            inputBox.Text = node.Node.Title.ClearValue;
            RenameFlyout.ShowAt(this.childGridView.ContainerFromItem(node) as FrameworkElement);

            inputBox.SelectAll();
        }

        /// <summary>
        /// Auto-event handler for when the user wants to delete a node.
        /// </summary>
        /// <param name="vm">The ViewModel.</param>
        /// <param name="node">The node being deleted.</param>
        [AutoWire(nameof(IDatabaseViewModel.RequestDeleteNode))]
        public async void RequestDeleteNodeHandler(IDatabaseViewModel vm, IDatabaseNodeViewModel node)
        {
            DebugHelper.Trace($"Delete requested for node {node.Node.Title.ClearValue}");

            MessageDialog dialog = new MessageDialog(
                GetString(DatabaseView.DeletePromptKey),
                GetString(DatabaseView.DeletePromptTitleKey)
            );

            IUICommand yesCommand = new UICommand(GetString("Yes"));
            IUICommand noCommand = new UICommand(GetString("No"));

            dialog.Commands.Add(yesCommand);
            dialog.Commands.Add(noCommand);

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            IUICommand chosenCommand = await dialog.ShowAsync();
            if (chosenCommand == noCommand)
            {
                // User chose not to delete after all, abort.
                return;
            }

            // Otherwise the user confirmed the delete, so do it.
            ViewModel.DeleteNodeAndSave(node.Node);
        }

        /// <summary>
        /// Auto-event handler for when the user wants to get details for a node.
        /// </summary>
        /// <param name="vm">The ViewModel.</param>
        /// <param name="node">The node for which to request details.</param>
        [AutoWire(nameof(IDatabaseViewModel.RequestDetails))]
        public void RequestDetailsHandler(IDatabaseViewModel vm, IDatabaseNodeViewModel node)
        {
            DebugHelper.Trace($"Details requested for node {node.Node.Title.ClearValue}");

            if (node.Node is IKeePassEntry entry)
            {
                Frame.Navigate(
                    typeof(EntryDetailsView),
                    ViewModel.GetEntryDetailsViewModel(entry, /* editing */ true)
                );
            }
            else
            {
                IKeePassGroup group = node.Node as IKeePassGroup;
                DebugHelper.Assert(group != null);
                Frame.Navigate(
                    typeof(GroupDetailsView),
                    ViewModel.GetGroupDetailsViewModel(group, /* editing */ true)
                );
            }
        }

        #endregion

        /// <summary>
        /// Handles setting up the sort mode MenuFlyout when this page is navigated to.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            foreach (DatabaseSortMode sortMode in ViewModel.AvailableSortModes)
            {
                ToggleMenuFlyoutItem menuItem = new ToggleMenuFlyoutItem
                {
                    Text = sortMode.ToString(),
                    IsChecked = sortMode == ViewModel.SortMode,
                    Tag = sortMode
                };

                menuItem.RegisterPropertyChangedCallback(ToggleMenuFlyoutItem.IsCheckedProperty, SortModeToggled);

                this.sortModeFlyout.Items.Add(menuItem);
            }
        }

        /// <summary>
        /// Handles updating the ViewModel when the user opts to change the sort mode.
        /// </summary>
        /// <param name="sender">The ToggleMenuFlyoutItem being updated.</param>
        /// <param name="dp">The IsChecked property.</param>
        private void SortModeToggled(DependencyObject sender, DependencyProperty dp)
        {
            ToggleMenuFlyoutItem menuItem = sender as ToggleMenuFlyoutItem;
            DebugHelper.Assert(menuItem != null);

            DatabaseSortMode sortMode = menuItem.Tag as DatabaseSortMode;
            DebugHelper.Assert(sortMode != null);

            if (menuItem.IsChecked && ViewModel.SortMode != sortMode)
            {
                // Update ViewModel and uncheck all other buttons
                ViewModel.SortMode = sortMode;
                foreach(MenuFlyoutItemBase sortModeChild in this.sortModeFlyout.Items)
                {
                    if (sortModeChild is ToggleMenuFlyoutItem item && item != menuItem)
                    {
                        item.IsChecked = false;
                    }
                }
            }
            else if (!menuItem.IsChecked && ViewModel.SortMode == sortMode)
            {
                // If we are unchecking the current sort mode, abort - user can't do this
                menuItem.IsChecked = true;
            }
        }

        public override bool HandleAcceleratorKey(VirtualKey key, bool shift)
        {
            IDatabaseEntryViewModel selectedEntry = this.childGridView.SelectedItem as IDatabaseEntryViewModel;

            if (!shift)
            {
                switch (key)
                {
                    case VirtualKey.S:
                        this.searchBox.Focus(FocusState.Programmatic);
                        this.searchBox.Text = String.Empty;
                        return true;
                    case VirtualKey.I:
                        CreateEntry();
                        return true;
                    case VirtualKey.G:
                        CreateGroup();
                        return true;
                    case VirtualKey.B:
                        if (selectedEntry != null)
                        {
                            selectedEntry.RequestCopyUsernameCommand.Execute(null);
                            return true;
                        }
                        break;
                    case VirtualKey.C:
                        if (selectedEntry != null)
                        {
                            selectedEntry.RequestCopyPasswordCommand.Execute(null);
                            return true;
                        }
                        break;
                    case VirtualKey.U:
                        if (selectedEntry != null)
                        {
                            selectedEntry.RequestLaunchUrlCommand.Execute(null);
                            return true;
                        }
                        break;
                }
            }
            else
            {
                switch (key)
                {
                    case VirtualKey.U:
                        if (selectedEntry != null)
                        {
                            selectedEntry.RequestCopyUrlCommand.Execute(null);
                            return true;
                        }
                        break;
                }
            }

            return base.HandleAcceleratorKey(key, shift);
        }

        /// <summary>
        /// Provides a means of the parent page requesting a navigate from a clicked breadcrumb to the specified group.
        /// </summary>
        /// <remarks>
        /// This doesn't actually navigate, just updates the navViewModel.
        /// </remarks>
        /// <param name="dbViewModel">The DatabaseViewModel to use for the navigation.</param>
        /// <param name="navViewModel">The NavigationViewModel to update.</param>
        /// <param name="clickedGroup">The group to navigate to.</param>
        public override Task RequestBreadcrumbNavigation(IDatabaseViewModel dbViewModel, IDatabaseNavigationViewModel navViewModel, IKeePassGroup clickedGroup)
        {
            navViewModel.SetGroup(clickedGroup);

            // Task.CompletedTask is better here but not available presently.
            return Task.FromResult(0);
        }

        /// <summary>
        /// Creates a Popup that will allow renaming of the currently selected node.
        /// </summary>
        private void PromptToRenameSelection()
        {
            IDatabaseNodeViewModel selectedNode = this.childGridView.SelectedItem as IDatabaseNodeViewModel;
            DebugHelper.Assert(selectedNode != null);

            selectedNode.RequestRenameCommand.Execute(null);
        }

        /// <summary>
        /// Opens the selection for editing.
        /// </summary>
        private void EditSelection()
        {
            IDatabaseNodeViewModel selectedNode = this.childGridView.SelectedItem as IDatabaseNodeViewModel;
            DebugHelper.Assert(selectedNode != null);

            selectedNode.RequestEditDetailsCommand.Execute(null);
        }

        /// <summary>
        /// Confirms the user's choice and then deletes the currently selected node.
        /// </summary>
        /// <remarks>
        /// This function currently deletes ONE SelectedItem.
        /// </remarks>
        private void PromptToDeleteSelection()
        {
            IDatabaseNodeViewModel selectedNode = this.childGridView.SelectedItem as IDatabaseNodeViewModel;
            DebugHelper.Assert(selectedNode != null);

            DebugHelper.Assert(ViewModel.PersistenceService.CanSave);
            selectedNode.RequestDeleteCommand.Execute(null);
        }

        /// <summary>
        /// Handles the user opting to create a new entry in the current group.
        /// </summary>
        private void CreateEntry()
        {
            if (ViewModel.PersistenceService.CanSave)
            {
                Frame.Navigate(
                    typeof(EntryDetailsView),
                    ViewModel.GetEntryDetailsViewModel(
                        ViewModel.NavigationViewModel.ActiveGroup
                    )
                );
            }
        }

        /// <summary>
        /// Handles the user opting to create a new group within the current group.
        /// </summary>
        private void CreateGroup()
        {
            if (ViewModel.PersistenceService.CanSave)
            {
                Frame.Navigate(
                    typeof(GroupDetailsView),
                    ViewModel.GetGroupDetailsViewModel(
                        ViewModel.NavigationViewModel.ActiveGroup
                    )
                );
            }
        }

        /// <summary>
        /// EventHandler for user interaction with the BreadcrumbNavigator.
        /// </summary>
        /// <param name="sender">The BreadcrumbNavigator.</param>
        /// <param name="e">EventArgs provided the clicked group.</param>
        private void Breadcrumb_GroupClicked(object sender, GroupClickedEventArgs e)
        {
            IKeePassGroup clickedGroup = e.Group;
            DebugHelper.Assert(clickedGroup != null);

            DebugHelper.Trace($"Updating View to breadcrumb: {e.Group.Title.ClearValue}");
            ViewModel.NavigationViewModel.SetGroup(clickedGroup);
        }

        /// <summary>
        /// Handles changes to the search text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SearchBox_QueryChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            DebugHelper.Trace($"New query: {sender.Text}");
            if (String.IsNullOrEmpty(sender.Text))
            {
                ViewModel.Filter = String.Empty;
            }
            else if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // TODO: If the user has entered some sort of text, populate sender.ItemsSource with suggestions.
            }
        }

        /// <summary>
        /// Handles queries from the SearchBox.
        /// </summary>
        /// <param name="sender">The querying SearchBox.</param>
        /// <param name="args">Args for the query.</param>
        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            DebugHelper.Trace($"Handling SearchBox query: {args.QueryText}");
            ViewModel.Filter = args.QueryText;
        }

        /// <summary>
        /// Handler for a suggestion being chosen for the search box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            // TODO: Set sender.Text based on args
        }

        /// <summary>
        /// Handles clicks on the Child GridView.
        /// </summary>
        /// <param name="sender">The Child GridView.</param>
        /// <param name="e">ClickEventArgs for the action.</param>
        private void ChildGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            bool wasFiltered = !String.IsNullOrEmpty(this.searchBox.Text);
            if (wasFiltered)
            {
                this.searchBox.Text = String.Empty;
            }

            // First check to see if it's an entry
            if (e.ClickedItem is IDatabaseEntryViewModel clickedEntry)
            {
                IKeePassEntry entry = clickedEntry.Node as IKeePassEntry;
                DebugHelper.Assert(entry != null);

                if (wasFiltered)
                {
                    ViewModel.NavigationViewModel.SetGroup(entry.Parent);
                }

                // For now, on item click, navigate to the EntryDetailsView.
                Frame.Navigate(
                    typeof(EntryDetailsView),
                    ViewModel.GetEntryDetailsViewModel(entry, /* editing */ false)
                );
            }
            else
            {
                // We clicked a group, so drill into it...
                IDatabaseGroupViewModel clickedGroup = e.ClickedItem as IDatabaseGroupViewModel;
                DebugHelper.Assert(clickedGroup != null);

                clickedGroup.RequestOpenCommand.Execute(null);
            }
        }

        /// <summary>
        /// Handles KeyUp events from the TextBox inside the node rename flyout.
        /// </summary>
        /// <param name="sender">The node rename input box.</param>
        /// <param name="e">EventArgs for the keyup event.</param>
        private void NodeRenameBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            DebugHelper.Assert(this.nodeBeingRenamed != null);
            DebugHelper.Assert(sender is TextBox);

            string input = ((TextBox)sender).Text;

            if (e.Key == VirtualKey.Enter && input.Length > 0)
            {
                ViewModel.RenameNodeAndSave(this.nodeBeingRenamed.Node, input);
                RenameFlyout.Hide();
            }
        }

        /// <summary>
        /// Invoked when the node rename flyout is closed.
        /// </summary>
        /// <param name="sender">The flyout.</param>
        /// <param name="e">N/A</param>
        private void RenameFlyout_Closed(object sender, object e)
        {
            this.nodeBeingRenamed = null;
        }

        /// <summary>
        /// Invoked when the user begins to drag an item in the node GridView.
        /// </summary>
        /// <param name="sender">The node GridView.</param>
        /// <param name="e">EventArgs for the drag operation.</param>
        private void childGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            DebugHelper.Assert(e.Items.Count == 1);

            IDatabaseNodeViewModel viewModel = e.Items[0] as IDatabaseNodeViewModel;
            DebugHelper.Assert(viewModel != null);

            e.Data.SetText(viewModel.Node.Uuid.EncodedValue);
        }

        /// <summary>
        /// Invoked when the user is done dragging an item in the node GridView. The DropResult on
        /// <paramref name="args"/> will give information on what happened.
        /// </summary>
        /// <param name="sender">The node GridView.</param>
        /// <param name="args">EventArgs for the completed drag operation.</param>
        private void childGridView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (args.DropResult == DataPackageOperation.Move)
            {
                ViewModel.Save();
            }
        }
    }
}
