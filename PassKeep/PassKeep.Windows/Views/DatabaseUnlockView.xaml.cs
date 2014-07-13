using PassKeep.Lib.EventArgClasses;
using PassKeep.Models;
using PassKeep.ViewBases;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PassKeep.Views
{
    /// <summary>
    /// A View used to unlock/decrypt a candidate database file.
    /// </summary>
    public sealed partial class DatabaseUnlockView : DatabaseUnlockViewBase
    {

        public DatabaseUnlockView()
            : base()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles the ENTER key for the password input box.
        /// </summary>
        /// <param name="sender">The password box.</param>
        /// <param name="e">EventArgs for the KeyUp event.</param>
        private void PasswordBox_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (unlockButton.Command.CanExecute(null))
                {
                    Debug.WriteLine("{0} got [ENTER], attempting to unlock database...", this.GetType());
                    unlockButton.Command.Execute(null);
                }
                else
                {
                    Debug.WriteLine("{0} got [ENTER], but database is not currently unlockable.", this.GetType());
                }
            }
        }

        /// <summary>
        /// Handles letting the user select a different database from this View, without navigating.
        /// </summary>
        /// <param name="sender">The button that invokes this command.</param>
        /// <param name="e">EventArgs for the click.</param>
        private async void DifferentDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("User clicked the 'open different database' button.");
            await PickFile(
                file =>
                {
                    this.ViewModel.CandidateFile = new StorageFileDatabaseCandidate(file);
                }
            );
        }

        /// <summary>
        /// Handles letting the user select a keyfile.
        /// </summary>
        /// <param name="sender">The button that invokes this command.</param>
        /// <param name="e">EventArgs for the click.</param>
        private async void ChooseKeyfileButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("User clicked the 'choose keyfile' button.");
            await PickFile(
                file =>
                {
                    this.ViewModel.KeyFile = file;
                },
                () =>
                {
                    this.ViewModel.KeyFile = null;
                }
            );
        }

        /// <summary>
        /// Auto-event handler for the ViewModel's DocumentReady event.
        /// </summary>
        /// <param name="sender">The ViewModel.</param>
        /// <param name="e">EventArgs with the new document.</param>
        public void DocumentReadyHandler(object sender, DocumentReadyEventArgs e)
        {
            Frame.Navigate(
                typeof(DatabaseView),
                new
                {
                    document = e.Document,
                    writer = e.Writer,
                    candidate = this.ViewModel.CandidateFile
                }
            );
        }
    }
}
