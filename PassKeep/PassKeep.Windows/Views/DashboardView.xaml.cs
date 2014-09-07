using PassKeep.Contracts.Models;
using PassKeep.Framework;
using PassKeep.Models;
using PassKeep.ViewBases;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

// The Hub Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=321224

namespace PassKeep.Views
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class DashboardView : DashboardViewBase
    {
        private Grid welcomeBlurb;
        private GridView recentDatabases;

        public DashboardView()
            : base()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Adds a button to the AppBar for forgetting databases.
        /// </summary>
        /// <returns></returns>
        public override IList<ICommandBarElement> GetPrimaryCommandBarElements()
        {
            AppBarButton loadButton = new AppBarButton
            {
                Label = GetString("LoadDatabaseText"),
                Icon = new SymbolIcon(Symbol.OpenFile),
                Command = new TypedCommand<StoredFileDescriptor>(
                    file => (file != null),
                    async (file) => { await AttemptToLoadRecentDatabase(file); }
                )
            };

            // Bind the CommandParameter to this.recentDatabases.SelectedItem
            loadButton.SetBinding(
                ButtonBase.CommandParameterProperty,
                new Binding
                {
                    Source = this.recentDatabases,
                    Path = new PropertyPath("SelectedItem")
                }
            );

            AppBarButton forgetButton = new AppBarButton
            {
                Label = GetString("ForgetDatabaseText"),
                Icon = new SymbolIcon(Symbol.Delete),
                Command = this.ViewModel.ForgetCommand
            };
            
            // Bind the CommandParameter to this.recentDatabases.SelectedItem
            forgetButton.SetBinding(
                ButtonBase.CommandParameterProperty,
                new Binding
                {
                    Source = this.recentDatabases,
                    Path = new PropertyPath("SelectedItem")
                }
            );

            return new List<ICommandBarElement>
            {
                loadButton,
                forgetButton
            };
        }

        /// <summary>
        /// Updates the VisualStateManager's state based on a new window size.
        /// </summary>
        /// <param name="newWindowSize">The size to base the state on.</param>
        protected override void SetVisualState(Size windowSize)
        {
            if (this.welcomeBlurb == null)
            {
                // Page isn't fully loaded yet...
                return;
            }

            string newState = StandardState.Name;

            if (windowSize.Width < PassKeepPage.NarrowWidth)
            {
                newState = NarrowState.Name;
            }

            Debug.WriteLine("Going to new VisualState: {0}", newState);
            Debug.WriteLine(VisualStateManager.GoToState(this, newState, false));

            if (newState == StandardState.Name)
            {
                this.rootHub.Orientation = Orientation.Horizontal;
                this.welcomeBlurb.Visibility = Visibility.Visible;
                this.welcomeSection.ClearValue(HubSection.WidthProperty);
                this.recentSection.ClearValue(HubSection.WidthProperty);
                this.recentSection.ClearValue(HubSection.WidthProperty);
                this.pageTitle.ClearValue(TextBlock.FontSizeProperty);
            }
            else if (newState == NarrowState.Name)
            {
                this.rootHub.Orientation = Orientation.Vertical;
                this.welcomeBlurb.Visibility = Visibility.Collapsed;
                this.welcomeSection.Width = windowSize.Width;
                this.recentSection.Width = windowSize.Width;
                //this.pinnedSection.Width = windowSize.Width;
                this.pageTitle.FontSize = 30.0;
            }
        }

        /// <summary>
        /// Attempts to load a recent database from a StoredFileDescriptor.
        /// </summary>
        /// <param name="descriptor">The descriptor to load.</param>
        private async Task AttemptToLoadRecentDatabase(StoredFileDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
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
        /// Stores a reference to the welcome blurb Grid when it loads.
        /// </summary>
        /// <param name="sender">The Grid we're interested in.</param>
        /// <param name="e">EventArgs for the Loaded event.</param>
        private void welcomeBlurb_Loaded(object sender, RoutedEventArgs e)
        {
            this.welcomeBlurb = (Grid)sender;
            DetermineVisualState();
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
                new
                {
                    file = file,
                    isSampleFile = isSample
                }
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
            Debug.Assert(tappedDescriptor != null);
            await AttemptToLoadRecentDatabase(tappedDescriptor);
        }

        /// <summary>
        /// Invoked when the recentDatabases GridView has loaded within its HubSection.
        /// </summary>
        /// <param name="sender">The GridView.</param>
        /// <param name="e">EventArgs for the load event.</param>
        private void recentDatabases_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(sender is GridView);
            this.recentDatabases = (GridView)sender;

            RaisePrimaryCommandsAvailable();
        }

        /// <summary>
        /// Invoked when the selection of the recentDatabases GridView has changed the SelectedItem.
        /// </summary>
        /// <param name="sender">The GridView.</param>
        /// <param name="e">EventArgs for the selection event.</param>
        private void recentDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                this.BottomAppBar.IsOpen = true;
            }
        }
    }
}
