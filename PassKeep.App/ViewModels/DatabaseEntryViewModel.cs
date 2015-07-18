using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Windows.Input;
using Windows.System;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Extends <see cref="DatabaseNodeViewModel"/> to wrap an entry, specifically.
    /// </summary>
    public sealed class DatabaseEntryViewModel : DatabaseNodeViewModel, IDatabaseEntryViewModel
    {
        private ISensitiveClipboardService clipboardService;
        private Uri entryUri;

        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        /// <param name="entry">The database entry to proxy.</param>
        public DatabaseEntryViewModel(IKeePassEntry entry, ISensitiveClipboardService clipboardService)
            : base(entry)
        {
            this.clipboardService = clipboardService;
            try
            {
                this.entryUri = new Uri(entry.Url.ClearValue);
            }
            catch (Exception)
            {
                this.entryUri = null;
            }

            this.RequestCopyUsernameCommand = new ActionCommand(
                () =>
                {
                    this.clipboardService.CopyCredential(((IKeePassEntry)this.Node).UserName.ClearValue, ClipboardOperationType.UserName);
                }
            );

            this.RequestCopyPasswordCommand = new ActionCommand(
                () =>
                {
                    this.clipboardService.CopyCredential(((IKeePassEntry)this.Node).Password.ClearValue, ClipboardOperationType.UserName);
                }
            );

            this.RequestLaunchUrlCommand = new ActionCommand(
                () => this.entryUri != null,
                async () =>
                {
                    await Launcher.LaunchUriAsync(this.entryUri);
                }
            );
        }

        /// <summary>
        /// Command for requesting copy of the username credential.
        /// </summary>
        public ICommand RequestCopyUsernameCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Command for requesting copy of the password credential.
        /// </summary>
        public ICommand RequestCopyPasswordCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Command for requesting a launch of the entry's URL.
        /// </summary>
        public ICommand RequestLaunchUrlCommand
        {
            get;
            private set;
        }
    }
}
