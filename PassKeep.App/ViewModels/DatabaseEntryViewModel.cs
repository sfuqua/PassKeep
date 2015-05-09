using PassKeep.EventArgClasses;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Windows.Input;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Extends <see cref="DatabaseNodeViewModel"/> to wrap an entry, specifically.
    /// </summary>
    public sealed class DatabaseEntryViewModel : DatabaseNodeViewModel, IDatabaseEntryViewModel
    {
        /// <summary>
        /// Fired when credentials should be copied to the clipboard.
        /// </summary>
        public event EventHandler<CopyRequestedEventArgs> CopyRequested;

        /// <summary>
        /// Initializes the ViewModel.
        /// </summary>
        /// <param name="entry">The database entry to proxy.</param>
        public DatabaseEntryViewModel(IKeePassEntry entry)
            : base(entry)
        {
            this.RequestCopyUsernameCommand = new ActionCommand(
                () => { FireCopyRequested(ClipboardTimerType.UserName); }
            );

            this.RequestCopyPasswordCommand = new ActionCommand(
                () => { FireCopyRequested(ClipboardTimerType.Password); }
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

        private void FireCopyRequested(ClipboardTimerType copyType)
        {
            CopyRequested?.Invoke(this, new CopyRequestedEventArgs((IKeePassEntry)this.Node, copyType));
        }
    }
}
