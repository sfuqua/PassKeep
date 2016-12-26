using PassKeep.ViewBases;
using PassKeep.Views.Controls;
using System;
using Windows.UI.Xaml;

namespace PassKeep.Views.FlyoutPages
{
    public partial class AppSettingsView : AppSettingsViewBase
    {
        public AppSettingsView()
        {
            InitializeComponent();
        }

        private async void manageCachedFilesButton_Click(object sender, RoutedEventArgs e)
        {
            await new CachedFileManagementDialog(
                await this.ViewModel.GetCachedFilesViewModelAsync()
            ).ShowAsync();
        }

        private async void managePasswordsButton_Click(object sender, RoutedEventArgs e)
        {
            await new PasswordManagementDialog(
                await this.ViewModel.GetSavedCredentialsViewModelAsync()
            ).ShowAsync();
        }
    }
}
