using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
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

        public override bool HandleAcceleratorKey(Windows.System.VirtualKey key)
        {
            // No accelerator key to handle
            return false;
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
        /// EventHandler for user interaction with the BreadcrumbNavigator.
        /// </summary>
        /// <param name="sender">The BreadcrumbNavigator.</param>
        /// <param name="e">EventArgs provided the clicked group.</param>
        private async void Breadcrumb_GroupClicked(object sender, GroupClickedEventArgs e)
        {
            IKeePassGroup clickedGroup = e.Group;
            Debug.Assert(clickedGroup != null);

            await PromptSaveAndThen(
                () => {
                    Debug.WriteLine("Updating View to breadcrumb: {0}", e.Group.Title.ClearValue);
                    this.ViewModel.NavigationViewModel.SetGroup(clickedGroup);

                    Frame.Navigate(
                        typeof(DatabaseView),
                        new NavigationParameter(
                            new {
                                document = this.ViewModel.Document,
                                databasePersistenceService = this.ViewModel.PersistenceService,
                                navigationViewModel = this.ViewModel.NavigationViewModel
                            }
                        )
                    );
                }
            );
        }
    }
}
