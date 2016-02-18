using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Views;
using SariphLib.Infrastructure;
using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.ViewBases
{
    /// <summary>
    /// Encompasses common functionality for EntryDetailsView and GroupDetailsView, such as editing nodes.
    /// </summary>
    /// <typeparam name="TNodeViewModel"></typeparam>
    /// <typeparam name="TNode"></typeparam>
    public abstract class NodeDetailsViewBase<TNodeViewModel, TNode> : DatabaseChildViewBase<TNodeViewModel>
        where TNode : IKeePassNode
        where TNodeViewModel : class, INodeDetailsViewModel<TNode>
    {
        private MessageDialog confirmRevertDialog;
        private MessageDialog promptSaveDialog;

        private IUICommand confirmationYesCommand;
        private IUICommand confirmationNoCommand;
        private IUICommand confirmationCancelCommand;

        private bool safeToNavigate;

        protected NodeDetailsViewBase()
        {
            this.confirmRevertDialog = new MessageDialog(
                GetString("RevertPrompt"),
                GetString("RevertPromptTitle")
            )
            {
                Options = MessageDialogOptions.None
            };

            this.promptSaveDialog = new MessageDialog(
                GetString("UnsavedPrompt"),
                GetString("UnsavedPromptTitle")
            )
            {
                Options = MessageDialogOptions.None
            };

            this.confirmationYesCommand = new UICommand(GetString("Yes"));
            this.confirmationNoCommand = new UICommand(GetString("No"));
            this.confirmationCancelCommand = new UICommand(GetString("Cancel"));

            this.confirmRevertDialog.Commands.Add(this.confirmationYesCommand);
            this.confirmRevertDialog.Commands.Add(this.confirmationCancelCommand);

            this.confirmRevertDialog.DefaultCommandIndex = 0;
            this.confirmRevertDialog.CancelCommandIndex = 1;

            this.promptSaveDialog.Commands.Add(this.confirmationYesCommand);
            this.promptSaveDialog.Commands.Add(this.confirmationNoCommand);

            this.promptSaveDialog.DefaultCommandIndex = 0;
            this.promptSaveDialog.CancelCommandIndex = 1;
        }

        /// <summary>
        /// Abstracts access to the "edit" toggle button.
        /// </summary>
        public abstract ToggleButton EditToggleButton
        {
            get;
        }

        #region Common auto-event handlers

        /// <summary>
        /// Event handler for when the node requests to be reverted.
        /// Prompts the user before proceeding.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void RevertRequiredHandler(object sender, EventArgs e)
        {
            if (this.ViewModel.IsNew)
            {
                await ConfirmRevert(() =>
                {
                    this.safeToNavigate = true;
                    Frame.GoBack();
                });
            }
            else
            {
                await ConfirmRevert(() => { });
            }
        }

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
            this.safeToNavigate = false;
        }

        /// <summary>
        /// Handles cancelling navigates if the ViewModel is dirty (prompts for save).
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.Cancel || this.safeToNavigate)
            {
                return;
            }

            CancellableNavigationParameter cnp = e.Parameter as CancellableNavigationParameter;
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

                // If the ViewModel IsNew, then we need to set the safeToNavigate flag
                // to avoid an endless loop of navigates thanks to IsDirty().
                if (this.ViewModel.IsNew)
                {
                    this.safeToNavigate = true;
                }

                if (Frame.Navigate(e.SourcePageType, navParameter, e.NavigationTransitionInfo) && cnp != null)
                {
                    cnp.Callback();
                }
            };

            await PromptSaveAndThen(doNavigate);
        }

        /// <summary>
        /// Handles hotkeys for toggling between save and edit.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="shift"></param>
        /// <returns>Whether the hotkey was handled.</returns>
        public override bool HandleAcceleratorKey(VirtualKey key, bool shift)
        {
            if (!shift)
            {
                switch (key)
                {
                    case VirtualKey.D:
                        if (this.ViewModel.PersistenceService.CanSave)
                        {
                            this.EditToggleButton.IsChecked = !(this.EditToggleButton.IsChecked ?? false);
                        }
                        break;

                    case VirtualKey.S:
                        if (!this.ViewModel.IsReadOnly)
                        {
                            this.ViewModel.TrySave();
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

            Frame.Navigate(typeof(DatabaseView), new CancellableNavigationParameter(navCallback, dbViewModel));
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Saves the node and sets to ReadOnly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.TrySave();
        }

        /// <summary>
        /// Forces the toggle button stay checked if manually unchecked, until the ViewModel updates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void editToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            Dbg.Assert(sender == this.EditToggleButton);

            if (!this.ViewModel.IsReadOnly)
            {
                this.EditToggleButton.IsChecked = true;
            }
        }

        /// <summary>
        /// When exiting edit mode, prompts the user if a revert is necessary.
        /// The user can either proceed or cancel.
        /// </summary>
        /// <param name="callback">The action to take if confirmed.</param>
        /// <returns>A Task representing whether consent was granted to proceed.</returns>
        private async Task<bool> ConfirmRevert(Action callback)
        {
            bool confirmed = this.ViewModel.IsReadOnly || !this.ViewModel.IsDirty();
            if (!confirmed)
            {
                IUICommand chosenCmd = await this.confirmRevertDialog.ShowAsync();
                if (chosenCmd == this.confirmationYesCommand)
                {
                    // User chose to revert - proceed
                    this.ViewModel.Revert();
                    confirmed = true;
                }
                else
                {
                    // User chose to cancel, abort!
                    Dbg.Assert(chosenCmd == this.confirmationCancelCommand);
                    confirmed = false;
                }
            }

            if (confirmed)
            {
                callback();
            }

            return confirmed;
        }

        /// <summary>
        /// When navigating, prompts the user to save if needed.
        /// </summary>
        /// <param name="callback">The action to take if confirmed.</param>
        /// <returns>A task representing the async action.</returns>
        private async Task PromptSaveAndThen(Action callback)
        {
            bool confirmed = this.ViewModel.IsReadOnly || !this.ViewModel.IsDirty();
            if (!this.ViewModel.IsReadOnly && this.ViewModel.IsDirty())
            {
                IUICommand chosenCmd = await this.promptSaveDialog.ShowAsync();
                if (chosenCmd == this.confirmationYesCommand)
                {
                    // User chose to save
                    await this.ViewModel.TrySave();
                }
                else
                {
                    // User chose not to save - revert and continue
                    Dbg.Assert(chosenCmd == this.confirmationNoCommand);
                    this.ViewModel.Revert();
                }
            }

            callback();
        }
    }
}
