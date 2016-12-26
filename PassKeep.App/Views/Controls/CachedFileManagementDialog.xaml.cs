using PassKeep.Lib.Contracts.ViewModels;
using System;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Views.Controls
{
    public sealed partial class CachedFileManagementDialog : ContentDialog
    {
        /// <summary>
        /// Creates an instance of the dialog with a default text blurb.
        /// </summary>
        /// <param name="viewModel"></param>
        public CachedFileManagementDialog(ICachedFilesViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            this.ViewModel = viewModel;
            this.InitializeComponent();
        }

        /// <summary>
        /// Creates an instance of the dialog with an overridden prompt blurb.
        /// </summary>
        /// <param name=""></param>
        /// <param name="blurb">Text to display at the top of the dialog.</param>
        public CachedFileManagementDialog(ICachedFilesViewModel viewModel, string blurb)
            : this(viewModel)
        {
            this.blurb.Text = blurb;
        }

        public ICachedFilesViewModel ViewModel
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
