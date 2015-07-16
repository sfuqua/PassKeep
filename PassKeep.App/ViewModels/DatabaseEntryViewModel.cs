using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Extends <see cref="DatabaseNodeViewModel"/> to wrap an entry, specifically.
    /// </summary>
    public sealed class DatabaseEntryViewModel : DatabaseNodeViewModel, IDatabaseEntryViewModel
    {
        private ISensitiveClipboardService clipboardService;

        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        /// <param name="entry">The database entry to proxy.</param>
        public DatabaseEntryViewModel(IKeePassEntry entry, ISensitiveClipboardService clipboardService)
            : base(entry)
        {
            this.clipboardService = clipboardService;

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
    }
}
