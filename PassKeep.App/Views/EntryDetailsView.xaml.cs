using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// A view for details on a specific <see cref="IEntryDetailsViewModel"/>.
    /// </summary>
    public sealed partial class EntryDetailsView : EntryDetailsViewBase
    {
        private MessageDialog confirmationDialog;
        private IUICommand confirmationYesCommand;
        private IUICommand confirmationNoCommand;
        private IUICommand confirmationCancelCommand;

        public EntryDetailsView()
            : base()
        {
            this.InitializeComponent();

            // If we're eligible to show a prompt, do it!
            this.confirmationDialog = new MessageDialog(
                GetString("UnsavedPrompt"),
                GetString("UnsavedPromptTitle")
            )
            {
                Options = MessageDialogOptions.None,
            };

            this.confirmationYesCommand = new UICommand(GetString("Yes"));
            this.confirmationNoCommand = new UICommand(GetString("No"));
            this.confirmationCancelCommand = new UICommand(GetString("Cancel"));

            this.confirmationDialog.Commands.Add(this.confirmationYesCommand);
            this.confirmationDialog.Commands.Add(this.confirmationNoCommand);
            this.confirmationDialog.Commands.Add(this.confirmationCancelCommand);

            this.confirmationDialog.DefaultCommandIndex = 0;
            this.confirmationDialog.CancelCommandIndex = 2;
        }

        /// <summary>
        /// Handles making sure the breadcrumbs are in sync after a navigate/tinkering with the backstack.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ViewModel.NavigationViewModel.SetGroup(this.ViewModel.WorkingCopy.Parent);
        }

        public override bool HandleAcceleratorKey(Windows.System.VirtualKey key, bool shift)
        {
            // No accelerator keys to handle
            return false;
        }

        /// <summary>
        /// Provides a means of the parent page requesting a navigate from a clicked breadcrumb to the specified group.
        /// </summary>
        /// <remarks>
        /// This allows the page to preempt the navigate or do necessary cleanup.
        /// </remarks>
        /// <param name="dbViewModel">The DatabaseViewModel to use for the navigation.</param>
        /// <param name="navViewModel">The NavigationViewModel to update.</param>
        /// <param name="clickedGroup">The group to navigate to.</param>
        public override async Task RequestBreadcrumbNavigation(IDatabaseViewModel dbViewModel, IDatabaseNavigationViewModel navViewModel, IKeePassGroup clickedGroup)
        {
            Action doNavigate = () =>
            {
                navViewModel.SetGroup(clickedGroup);
                Frame.Navigate(typeof(DatabaseView), dbViewModel);
            };

            await PromptSaveAndThen(doNavigate);
        }

        /// <summary>
        /// If the entry is dirty, prompts a user for a save before taking action.
        /// </summary>
        /// <param name="callback">The action to take if confirmed.</param>
        private async Task PromptSaveAndThen(Action callback)
        {
            bool confirmed = this.ViewModel.IsReadOnly || !this.ViewModel.IsDirty();
            if (!confirmed)
            {
                IUICommand chosenCmd = await this.confirmationDialog.ShowAsync();
                if (chosenCmd == this.confirmationYesCommand)
                {
                    // User chose to save - continue only if it succeeds (is not cancelled)
                    confirmed = await this.ViewModel.TrySave();
                }
                else if (chosenCmd == this.confirmationNoCommand)
                {
                    // User chose not to save - continue
                    confirmed = true;
                }
                else
                {
                    // User chose to cancel, abort!
                    Debug.Assert(chosenCmd == this.confirmationCancelCommand);
                    confirmed = false;
                }
            }

            if (confirmed)
            {
                callback();
            }
        }

        /// <summary>
        /// Updates the OverrideUrl in real-time as the corresponding TextBox changes.
        /// </summary>
        /// <param name="sender">The OverrideUrl input TextBox.</param>
        /// <param name="e">EventArgs for the change event.</param>
        private void entryOverrideUrlBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.WorkingCopy.OverrideUrl = ((TextBox)sender).Text;
        }
    }
}
