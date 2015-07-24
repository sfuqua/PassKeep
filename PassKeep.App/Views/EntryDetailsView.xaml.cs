using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
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

        private bool safeToNavigate;

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

        public Flyout FieldEditorFlyout
        {
            get
            {
                return Resources["fieldEditFlyout"] as Flyout;
            }
        }

        #region Auto-event handlers

        /// <summary>
        /// Auto-handler for property changes on the ViewModel. Let's us observe when there is a new FieldEditorViewModel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FieldEditorViewModel")
            {
                if (this.ViewModel.FieldEditorViewModel != null)
                {
                    IProtectedString editingString = this.ViewModel.FieldEditorViewModel.Original;

                    FrameworkElement flyoutTarget;
                    if (editingString == null)
                    {
                        // New field - show below "Fields" label
                        flyoutTarget = this.entryFieldsLabel;
                    }
                    else
                    {
                        // Existing field - show below GridView container
                        flyoutTarget = this.fieldsGridView.ContainerFromItem(editingString) as FrameworkElement;
                        Dbg.Assert(flyoutTarget != null);
                    }

                    ((FrameworkElement)this.FieldEditorFlyout.Content).DataContext = this.ViewModel;
                    this.FieldEditorFlyout.ShowAt(flyoutTarget);
                }
                else
                {
                    // Field has been committed or otherwise discarded
                    this.FieldEditorFlyout.Hide();

                    // TODO: Resize the field in the GridView if needed
                    // Currently difficult because I need a reference to the updated string. Add an event to the ViewModel?
                }
            }
        }

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
                await SaveOrRevertAndThen(() =>
                {
                    this.safeToNavigate = true;
                    Frame.GoBack();
                });
            }
            else
            {
                await SaveOrRevertAndThen(() => { });
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

            await SaveOrRevertAndThen(doNavigate);
        }

        public override bool HandleAcceleratorKey(Windows.System.VirtualKey key, bool shift)
        {
            if (!shift)
            {
                switch (key)
                {
                    case VirtualKey.D:
                        this.editToggleButton.IsChecked = !(this.editToggleButton.IsChecked ?? false);
                        break;

                    case VirtualKey.S:
                        this.ViewModel.TrySave();
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
        /// If the entry is dirty, prompts a user for a save before taking action.
        /// </summary>
        /// <param name="callback">The action to take if confirmed.</param>
        /// <returns>A Task representing whether consent was granted to proceed.</returns>
        private async Task<bool> SaveOrRevertAndThen(Action callback)
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

            return confirmed;
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

        /// <summary>
        /// Updates the Tags in real-time as the corresponding TextBox changes.
        /// </summary>
        /// <param name="sender">The Tags input TextBox.</param>
        /// <param name="e">EventArgs for the change event.</param>
        private void entryTagsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.WorkingCopy.Tags = ((TextBox)sender).Text;
        }

        /// <summary>
        /// Updates the Notes in real-time as the corresponding TextBox changes.
        /// </summary>
        /// <param name="sender">The Notes input TextBox.</param>
        /// <param name="e">EventArgs for the change event.</param>
        private void entryNotesBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ViewModel.WorkingCopy.Notes.ClearValue = ((TextBox)sender).Text;
        }

        /// <summary>
        /// Saves the node and sets to ReadOnly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButtonClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.ViewModel.TrySave();
        }

        /// <summary>
        /// Forces the toggle button stay checked if manually unchecked, until the ViewModel updates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void editToggleButton_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (!this.ViewModel.IsReadOnly)
            {
                this.editToggleButton.IsChecked = true;
            }
        }

        /// <summary>
        /// Handles propagating real-time Key changes to the field being edited.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fieldNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.ViewModel.FieldEditorViewModel != null)
            {
                this.ViewModel.FieldEditorViewModel.WorkingCopy.Key = ((TextBox)sender).Text;
            }
        }

        /// <summary>
        /// Handles propagating real-time Value changes to the field being edited.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fieldValueBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.ViewModel.FieldEditorViewModel != null)
            {
                this.ViewModel.FieldEditorViewModel.WorkingCopy.ClearValue = ((TextBox)sender).Text;
            }
        }
    }
}
