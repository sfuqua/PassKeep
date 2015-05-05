using PassKeep.Contracts.Models;
using PassKeep.Framework;
using PassKeep.Models;
using PassKeep.ViewBases;
using SariphLib.Infrastructure;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Views
{
    /// <summary>
    /// Displays welcome text and a dashboard of previously accessed databases.
    /// </summary>
    public sealed partial class DashboardView : DashboardViewBase
    {
        public DashboardView()
            : base()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Attempts to load a recent database from a StoredFileDescriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor to load.</param>
        private async Task AttemptToLoadRecentDatabase(StoredFileDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            IStorageFile storedFile = await this.ViewModel.GetFileAsync(descriptor);
            if (storedFile == null)
            {
                Debug.WriteLine("Warning: Could not fetch StorageFile. Forgetting descriptor.");
                this.ViewModel.ForgetCommand.Execute(descriptor);
            }
            else
            {
                Debug.WriteLine("Retrieved StorageFile from descriptor.");
                NavigateToOpenedFile(new StorageFileDatabaseCandidate(storedFile));
            }
        }

        /// <summary>
        /// Attempts to open a database of the user's choosing.
        /// </summary>
        /// <param name="sender">The "open database" button.</param>
        /// <param name="e">Args for the click.</param>
        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("User clicked the 'open database' button.");
            await PickFile(
                file =>
                {
                    NavigateToOpenedFile(new StorageFileDatabaseCandidate(file));
                }
            );
        }

        /// <summary>
        /// Attempts to open the sample database.
        /// </summary>
        /// <param name="sender">The "open sample" button.</param>
        /// <param name="e">Args for the click.</param>
        private async void OpenSample_Click(object sender, RoutedEventArgs e)
        {
            // Locate the sample file
            StorageFolder installFolder = Package.Current.InstalledLocation;
            StorageFolder subFolder = await installFolder.GetFolderAsync("Assets");
            IDatabaseCandidate sample = 
                new StorageFileDatabaseCandidate(await subFolder.GetFileAsync("SampleDatabase.kdbx"));

            NavigateToOpenedFile(sample, true);
        }

        /// <summary>
        /// Navigates the Frame to the DatabaseUnlockView for the specified database file.
        /// </summary>
        /// <param name="file">The file to begin unlocking.</param>
        /// <param name="isSample">Whether this is the sample database.</param>
        private void NavigateToOpenedFile(IDatabaseCandidate file, bool isSample = false)
        {
            Frame.Navigate(
                typeof(DatabaseUnlockView),
                new NavigationParameter(
                    new {
                        file = file,
                        isSampleFile = isSample
                    }
                )
            );
        }

        /// <summary>
        /// Handles primary taps/clicks on recent databases.
        /// Attempts to open the files.
        /// </summary>
        /// <param name="sender">The GridView.</param>
        /// <param name="e">EventArgs for the click.</param>
        private async void recentDatabases_ItemClick(object sender, ItemClickEventArgs e)
        {
            StoredFileDescriptor tappedDescriptor = e.ClickedItem as StoredFileDescriptor;
            Dbg.Assert(tappedDescriptor != null);
            await AttemptToLoadRecentDatabase(tappedDescriptor);
        }
    }
}
