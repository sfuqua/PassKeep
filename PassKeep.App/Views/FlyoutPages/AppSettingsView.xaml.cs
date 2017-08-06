// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
                await ViewModel.GetCachedFilesViewModelAsync()
            ).ShowAsync();
        }

        private async void managePasswordsButton_Click(object sender, RoutedEventArgs e)
        {
            await new PasswordManagementDialog(
                await ViewModel.GetSavedCredentialsViewModelAsync()
            ).ShowAsync();
        }
    }
}
