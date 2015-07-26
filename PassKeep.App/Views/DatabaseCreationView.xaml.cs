using System;
using PassKeep.ViewBases;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;

namespace PassKeep.Views
{
    /// <summary>
    /// View used to create a new database file.
    /// </summary>
    public sealed partial class DatabaseCreationView : DatabaseCreationViewBase
    {
        public DatabaseCreationView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles adding an event handler for Frame navigation to delete orphan StorageFiles.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Frame.Navigated += FrameNavigated;
        }

        /// <summary>
        /// Deletes orphaned StorageFiles if the ViewModel did not save.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FrameNavigated(object sender, NavigationEventArgs e)
        {
            Frame.Navigated -= FrameNavigated;
            await this.ViewModel.File.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }
    }
}
