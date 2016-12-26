using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Views.Controls;
using System;
using Windows.UI.Xaml;

namespace PassKeep.Views.Flyouts
{
    public sealed partial class AppSettingsFlyout
    {
        public AppSettingsFlyout(
            IAppSettingsViewModel viewModel
        )
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            this.ViewModel = viewModel;
            InitializeComponent();
        }

        public IAppSettingsViewModel ViewModel
        {
            get;
            private set;
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
