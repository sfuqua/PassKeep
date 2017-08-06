// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Infrastructure;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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

            ViewModel = viewModel;
            InitializeComponent();
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
            await ViewModel.DeleteAllAsyncCommand.ExecuteAsync(null);
            deferral.Complete();
        }

        /// <summary>
        /// Shows the context menu for the cached file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RelativePanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            Dbg.Assert(element != null);

            element.ShowAttachedMenuAsContextMenu(e);
        }
    }
}
