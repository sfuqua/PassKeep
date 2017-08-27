// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Models;
using PassKeep.Models;
using PassKeep.ViewBases;
using SariphLib.Files;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

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
            InitializeComponent();
        }

        /// <summary>
        /// Handles showing the MOTD if necessary.
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MessageOfTheDay motd = ViewModel.RequestMotd();
            if (motd.ShouldDisplay)
            {
                ContentDialog motdDialog = new ContentDialog
                {
                    Title = motd.Title,

                    Content = motd.Body,

                    PrimaryButtonText = motd.DismissText,
                    IsPrimaryButtonEnabled = true,
                    PrimaryButtonCommand = new TypedCommand<ContentDialog>(d => d.Hide()),
                };

                motdDialog.PrimaryButtonCommandParameter = motdDialog;
                motdDialog.Style = (Style)Resources["ScrollableContentDialogStyle"];

                await motdDialog.ShowAsync();
            }
        }

        [AutoWire(nameof(IDashboardViewModel.RequestOpenFile))]
        public async void RequestOpenFileHandler(IDashboardViewModel sender, StoredFileDescriptor eventArgs)
        {
            await AttemptToLoadRecentDatabase(eventArgs);
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

            ITestableFile storedFile = await ViewModel.GetFileAsync(descriptor);
            if (storedFile == null)
            {
                Debug.WriteLine("Warning: Could not fetch StorageFile. Forgetting descriptor.");
                descriptor.ForgetCommand.Execute(null);
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
            await PickKdbxForSaveAsync(
                "Database",
                kdbx =>
                {
                    Frame.Navigate(
                        typeof(DatabaseCreationView),
                        new NavigationParameter(
                            new
                            {
                                file = kdbx
                            }
                        )
                    );
                }
            );
        }

        /// <summary>
        /// Attempts to open a database of the user's choosing.
        /// </summary>
        /// <param name="sender">The "open database" button.</param>
        /// <param name="e">Args for the click.</param>
        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("User clicked the 'open database' button.");
            await PickFileForOpenAndContinueAsync(
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
                await DatabaseCandidateFactory.AssembleAsync(
                    new StorageFileWrapper(await subFolder.GetFileAsync("SampleDatabase.kdbx"))
                );

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
            DebugHelper.Assert(tappedDescriptor != null);
            await AttemptToLoadRecentDatabase(tappedDescriptor);
        }

        /// <summary>
        /// Handler for right-tap events on a <see cref="StoredFileDescriptor"/>. Handles displaying the context menu.
        /// </summary>
        /// <param name="sender">The container being tapped.</param>
        /// <param name="e">EventArgs for the tap.</param>
        private void storedFileTemplate_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            DebugHelper.Assert(element != null);

            element.ShowAttachedMenuAsContextMenu(e);
        }
    }
}
