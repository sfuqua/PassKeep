using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.ViewBases;
using SariphLib.Infrastructure;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace PassKeep.Views
{
    /// <summary>
    /// A view for details on a specific <see cref="IEntryDetailsViewModel"/>.
    /// </summary>
    public sealed partial class EntryDetailsView : EntryDetailsViewBase
    {
        public EntryDetailsView()
            : base()
        {
            this.InitializeComponent();
        }

        public override ToggleButton EditToggleButton
        {
            get { return this.editToggleButton; }
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

        #endregion

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

        /// <summary>
        /// Opens the content menu for fields.
        /// </summary>
        /// <param name="sender">The right-tapped field container.</param>
        /// <param name="e">EventArgs for the tap.</param>
        private void Field_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ((Border)sender).ShowAttachedMenuAsContextMenu(e);
        }
    }
}
