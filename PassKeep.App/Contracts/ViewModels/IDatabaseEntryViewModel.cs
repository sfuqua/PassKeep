using PassKeep.EventArgClasses;
using System;
using System.Windows.Input;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Extends <see cref="IDatabaseNodeViewModel"/> to enable
    /// copying of credentials.
    /// </summary>
    public interface IDatabaseEntryViewModel : IDatabaseNodeViewModel
    {
        /// <summary>
        /// Fired when the user requests to copy credentials (username or password).
        /// </summary>
        event EventHandler<CopyRequestedEventArgs> CopyRequested;

        /// <summary>
        /// Command for requesting copy of the username credential.
        /// </summary>
        ICommand RequestCopyUsernameCommand { get; }

        /// <summary>
        /// Command for requesting copy of the password credential.
        /// </summary>
        ICommand RequestCopyPasswordCommand { get; }
    }
}
