using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

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

        private ActionCommand editDetailsCommand;

        public DatabaseView()
            : base()
        {
            this.InitializeComponent();

            this.editDetailsCommand = new ActionCommand(
                () => this.childGridView.SelectedItem is IKeePassEntry,
                EditSelection
            );
        }

        #region Auto-event handlers

        /// <summary>
        /// Auto-event handler for saving a database.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">CancellableEventArgs for the operation.</param>
        public void StartedSaveHandler(object sender, CancellableEventArgs e)
        {
            this.RaiseStartedLoading(
                new LoadingStartedEventArgs(
                    GetString(PassKeepPage.SavingResourceKey),
                    e.Cts
                )
            );
        }

        /// <summary>
        /// Auto-event handler for when a save operation has stopped.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">EventArgs for the notification.</param>
        public void StoppedSaveHandler(object sender, EventArgs e)
        {
            this.RaiseDoneLoading();
        }

        #endregion

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
            Dbg.Assert(selectedNode != null);

            TextBox inputBox = new TextBox
            {
                Text = selectedNode.Node.Title.ClearValue
            };

            Flyout renameFlyout = new Flyout
            {
                Placement = FlyoutPlacementMode.Bottom,
                Content = inputBox
            };

            inputBox.KeyUp += (s, e) =>
            {
                string input = ((TextBox)s).Text;

                if (e.Key == Windows.System.VirtualKey.Enter && input.Length > 0)
                {
                    renameFlyout.Hide();
                    this.ViewModel.RenameNodeAndSave(selectedNode.Node, input);
                }
            };

            renameFlyout.ShowAt(this.childGridView.ContainerFromItem(selectedNode) as FrameworkElement);

            inputBox.SelectAll();
        }

        /// <summary>
        /// Opens the selection for editing.
        /// </summary>
        private void EditSelection()
        {
            IDatabaseEntryViewModel selectedEntry = this.childGridView.SelectedItem as IDatabaseEntryViewModel;
            Dbg.Assert(selectedEntry != null);

            if (selectedEntry != null)
            {
                // We've selected an entry, edit it.
                Frame.Navigate(
                    typeof(EntryDetailsView),
                    this.ViewModel.GetEntryDetailsViewModel(
                        (IKeePassEntry)selectedEntry.Node, /* editing */ true
                    )
                );
            }
            else
            {
                // Assume it's a group if it's not an entry, so edit that instead.
                // TODO: Implement this View
            }
        }

        /// <summary>
        /// Confirms the user's choice and then deletes the currently selected node.
        /// </summary>
        /// <remarks>
        /// This function currently deletes ONE SelectedItem.
        /// </remarks>
        private async void PromptToDeleteSelection()
        {
            Dbg.Assert(this.childGridView.SelectedItem != null);

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
            IDatabaseNodeViewModel selectedNode = this.childGridView.SelectedItem as IDatabaseNodeViewModel;
            if (selectedNode != null)
            {
                this.ViewModel.DeleteNodeAndSave(selectedNode.Node);
                return;
            }

            // Should never happen...
            throw new InvalidOperationException(
                $"Unable to delete unknown selection: {this.childGridView.SelectedItem}"
            );
        }

        /// <summary>
        /// Handles the user opting to create a new entry in the current group.
        /// </summary>
        private void CreateEntry()
        {
            Frame.Navigate(
                typeof(EntryDetailsView),
                this.ViewModel.GetEntryDetailsViewModel(
                    this.ViewModel.NavigationViewModel.ActiveGroup
                )
            );
        }

        /// <summary>
        /// Handles the user opting to create a new group within the current group.
        /// </summary>
        private void CreateGroup()
        {
            /* TODO
            Frame.Navigate(
                typeof(GroupDetailsView),
                this.ViewModel.GetGroupDetailsViewModel(
                    this.ViewModel.NavigationViewModel.ActiveGroup
                )
            );
            */
        }

        /// <summary>
        /// EventHandler for user interaction with the BreadcrumbNavigator.
        /// </summary>
        /// <param name="sender">The BreadcrumbNavigator.</param>
        /// <param name="e">EventArgs provided the clicked group.</param>
        private void Breadcrumb_GroupClicked(object sender, GroupClickedEventArgs e)
        {
            IKeePassGroup clickedGroup = e.Group;
            Dbg.Assert(clickedGroup != null);

            Dbg.Trace($"Updating View to breadcrumb: {e.Group.Title.ClearValue}");
            this.ViewModel.NavigationViewModel.SetGroup(clickedGroup);
        }

        /// <summary>
        /// Handles updates to the "SortBy" ComboBox.
        /// </summary>
        /// <param name="sender">The sorting ComboBox.</param>
        /// <param name="e">EventArgs for the selection change.</param>
        private void Sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            Dbg.Trace($"Handling sort selection change. New value: {box.SelectedItem}");
        }

        /// <summary>
        /// Handles queries from the SearchBox.
        /// </summary>
        /// <param name="sender">The querying SearchBox.</param>
        /// <param name="args">Args for the query.</param>
        private void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            Dbg.Trace($"Handling SearchBox query: {args.QueryText}");
            this.ViewModel.Filter = args.QueryText;
        }

        /// <summary>
        /// Handles clicks on the Child GridView.
        /// </summary>
        /// <param name="sender">The Child GridView.</param>
        /// <param name="e">ClickEventArgs for the action.</param>
        private void ChildGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            bool wasFiltered = !String.IsNullOrEmpty(this.searchBox.QueryText);
            if (wasFiltered)
            {
                this.searchBox.QueryText = String.Empty;
            }

            // First check to see if it's an entry
            IDatabaseEntryViewModel clickedEntry = e.ClickedItem as IDatabaseEntryViewModel;
            if (clickedEntry != null)
            {
                IKeePassEntry entry = clickedEntry.Node as IKeePassEntry;
                Dbg.Assert(entry != null);

                if (wasFiltered)
                {
                    this.ViewModel.NavigationViewModel.SetGroup(entry.Parent);
                }

                // For now, on item click, navigate to the EntryDetailsView.
                Frame.Navigate(
                    typeof(EntryDetailsView),
                    this.ViewModel.GetEntryDetailsViewModel(entry, /* editing */ false)
                );
            }
            else
            {
                // We clicked a group, so drill into it...
                IDatabaseNodeViewModel clickedNode = e.ClickedItem as IDatabaseNodeViewModel;
                Dbg.Assert(clickedNode != null);

                IKeePassGroup clickedGroup = clickedNode.Node as IKeePassGroup;
                Dbg.Assert(clickedGroup != null);

                this.ViewModel.NavigationViewModel.SetGroup(clickedGroup);
            }
        }

        /// <summary>
        /// Handles SelectionChanged events from the GridView.
        /// </summary>
        /// <param name="sender">The child GridView.</param>
        /// <param name="e">EventArgs for the selection change event.</param>
        private void ChildGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.editDetailsCommand.RaiseCanExecuteChanged();
        }

        private void SearchBox_QueryChanged(SearchBox sender, SearchBoxQueryChangedEventArgs args)
        {
            Dbg.Trace($"New query: {args.QueryText}");
            if (String.IsNullOrEmpty(args.QueryText))
            {
                this.ViewModel.Filter = String.Empty;
            }
        }
    }
}
