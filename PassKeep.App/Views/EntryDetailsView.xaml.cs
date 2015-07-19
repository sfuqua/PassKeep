using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Infrastructure;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
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

        #region Auto-event handlers

        /// <summary>
        /// Event handler for the database starting to be persisted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartedSaveHandler(object sender, CancellableEventArgs e)
        {
            RaiseStartedLoading(new LoadingStartedEventArgs(GetString("Saving"), e.Cts));
        }

        /// <summary>
        /// Event handler for the database no longer being persisted.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StoppedSaveHandler(object sender, EventArgs e)
        {
            RaiseDoneLoading();
        }

        #endregion

        /// <summary>
        /// Handles making sure the breadcrumbs are in sync after a navigate/tinkering with the backstack.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.ViewModel.NavigationViewModel.SetGroup(this.ViewModel.WorkingCopy.Parent);       
        }

        /// <summary>
        /// Handles cancelling navigates if the ViewModel is dirty (prompts for save).
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.Cancel)
            {
                return;
            }

            CancelableNavigationParameter cnp = e.Parameter as CancelableNavigationParameter;
            if (cnp != null)
            {
                e.Cancel = true;
            }

            if (ViewModel.IsReadOnly || !ViewModel.IsDirty())
            {
                // Immediately redo the navigate with the real parameter if necessary...
                if (cnp != null)
                {
                    e.Cancel = true;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        () =>
                        {
                            if (Frame.Navigate(e.SourcePageType, cnp.WrappedParameter, e.NavigationTransitionInfo))
                            {
                                cnp.Callback();
                            }
                        }
                    );
                }
                return;
            }

            // If the ViewModel is dirty, then we need to prompt before we navigate.
            e.Cancel = true;
            Action doNavigate = () =>
            {
                object navParameter = (cnp != null ? cnp.WrappedParameter : e.Parameter);
                if (cnp != null)
                {
                    if (Frame.Navigate(e.SourcePageType, cnp.WrappedParameter, e.NavigationTransitionInfo))
                    {
                        cnp.Callback();
                    }
                }
                else
                {
                    Frame.Navigate(e.SourcePageType, e.Parameter, e.NavigationTransitionInfo);
                }
            };

            await PromptSaveAndThen(doNavigate);
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
        public override Task RequestBreadcrumbNavigation(IDatabaseViewModel dbViewModel, IDatabaseNavigationViewModel navViewModel, IKeePassGroup clickedGroup)
        {
            Action navCallback = () =>
            {
                navViewModel.SetGroup(clickedGroup);
            };

            Frame.Navigate(typeof(DatabaseView), new CancelableNavigationParameter(navCallback, dbViewModel));
            return Task.FromResult<object>(null);
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
                    // User chose not to save - revert and continue
                    this.ViewModel.Revert();
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
