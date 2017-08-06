// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.ViewBases;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace PassKeep.Views
{
    /// <summary>
    /// A view for details on a specific <see cref="IGroupDetailsViewModel"/>.
    /// </summary>
    public sealed partial class GroupDetailsView : GroupDetailsViewBase
    {
        public GroupDetailsView()
            : base()
        {
            InitializeComponent();
        }

        public override ToggleButton EditToggleButton
        {
            get { return this.editToggleButton; }
        }

        /// <summary>
        /// Updates the Notes in real-time as the corresponding TextBox changes.
        /// </summary>
        /// <param name="sender">The Notes input TextBox.</param>
        /// <param name="e">EventArgs for the change event.</param>
        private void groupNotesBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.WorkingCopy.Notes.ClearValue = ((TextBox)sender).Text;
        }
    }
}
