using PassKeep.Lib.Contracts.ViewModels;
using System;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Views.Controls
{
    public sealed partial class PasswordManagementDialog : ContentDialog
    {
        /// <summary>
        /// Creates an instance of the dialog with a default text blurb.
        /// </summary>
        /// <param name="viewModel"></param>
        public PasswordManagementDialog(ISavedCredentialsViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            ViewModel = viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Creates an instance of the dialog with an overridden prompt blurb.
        /// </summary>
        /// <param name=""></param>
        /// <param name="blurb">Text to display at the top of the dialog.</param>
        public PasswordManagementDialog(ISavedCredentialsViewModel viewModel, string blurb)
            : this(viewModel)
        {
            this.blurb.Text = blurb;
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
            await ViewModel.DeleteAllAsyncCommand.ExecuteAsync(null);
            deferral.Complete();
        }
    }
}
