using PassKeep.Lib.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PassKeep.Views.Controls
{
    public sealed partial class PasswordManagementDialog : ContentDialog
    {
        public PasswordManagementDialog(ISavedCredentialsViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            this.ViewModel = viewModel;
            this.InitializeComponent();
        }

        public ISavedCredentialsViewModel ViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// Asynchronously deletes all saved passwords before closing the dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ContentDialogButtonClickDeferral deferral = args.GetDeferral();
            await this.ViewModel.DeleteAllAsyncCommand.ExecuteAsync(null);
            deferral.Complete();
        }
    }
}
