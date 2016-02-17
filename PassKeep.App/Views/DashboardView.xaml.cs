using PassKeep.Contracts.Models;
using PassKeep.Framework;
using PassKeep.Framework.Messages;
using PassKeep.Models;
using PassKeep.ViewBases;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
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
                NavigateToOpenedFile(await DatabaseCandidateFactory.AssembleAsync(storedFile));
            }
        }

        /// <summary>
        /// Prompts the user for a location to save their new database.
        /// </summary>
        /// <param name="sender">The "new database" button.</param>
        /// <param name="e">Args for the click.</param>
        private async void NewDatabase_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = "Database",
                DefaultFileExtension = ".kdbx"
            };

            picker.FileTypeChoices.Add("KeePass 2.x Database", new List<string> { ".kdbx" });

            StorageFile pickedFile = await picker.PickSaveFileAsync();
            if (pickedFile != null)
            {
                Frame.Navigate(
                    typeof(DatabaseCreationView),
                    new NavigationParameter(
                        new
                        {
                            file = pickedFile
                        }
                    )
                );
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
                async file =>
                {
                    NavigateToOpenedFile(await DatabaseCandidateFactory.AssembleAsync(file));
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
                await DatabaseCandidateFactory.AssembleAsync(await subFolder.GetFileAsync("SampleDatabase.kdbx"));

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
