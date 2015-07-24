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
            this.InitializeComponent();
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
            this.ViewModel.WorkingCopy.Notes.ClearValue = ((TextBox)sender).Text;
        }
    }
}
