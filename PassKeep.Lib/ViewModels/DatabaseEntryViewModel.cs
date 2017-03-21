using PassKeep.Lib.KeePass.Helpers;
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
        private readonly ISensitiveClipboardService clipboardService;
        private readonly IAppSettingsService settingsService;
        private Uri entryUri;

        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        /// <param name="entry">The database entry to proxy.</param>
        /// <param name="isReadOnly">Whether the database can currently be edited.</param>
        /// <param name="clipboardService">Clipboard service used for requesting credential copies.</param>
        /// <param name="settingsService">Service used to check settings for launching URLs.</param>
        public DatabaseEntryViewModel(
            IKeePassEntry entry,
            bool isReadOnly,
            ISensitiveClipboardService clipboardService,
            IAppSettingsService settingsService
        ) : base(entry, isReadOnly)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (clipboardService == null)
            {
                throw new ArgumentNullException(nameof(clipboardService));
            }

            if (settingsService == null)
            {
                throw new ArgumentNullException(nameof(settingsService));
            }

            this.clipboardService = clipboardService;
            this.settingsService = settingsService;
            this.entryUri = entry.GetLaunchableUri();

            RequestCopyUsernameCommand = new ActionCommand(
                () =>
                {
                    this.clipboardService.CopyCredential(((IKeePassEntry)Node).UserName.ClearValue, ClipboardOperationType.UserName);
                }
            );

            RequestCopyPasswordCommand = new ActionCommand(
                () =>
                {
                    this.clipboardService.CopyCredential(((IKeePassEntry)Node).Password.ClearValue, ClipboardOperationType.UserName);
                }
            );

            RequestCopyUrlCommand = new ActionCommand(
                () =>
                {
                    this.clipboardService.CopyCredential(((IKeePassEntry)Node).ConstructUriString(), ClipboardOperationType.Other);
                }
            );

            RequestLaunchUrlCommand = new ActionCommand(
                () => this.entryUri != null,
                async () =>
                {
                    if (this.settingsService.CopyPasswordOnUrlOpen)
                    {
                        RequestCopyPasswordCommand.Execute(null);
                    }
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
        /// Command for requesting copy of the entry's url.
        /// </summary>
        public ICommand RequestCopyUrlCommand
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
