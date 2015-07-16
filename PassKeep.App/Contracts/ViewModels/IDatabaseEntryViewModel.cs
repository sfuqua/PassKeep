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
        /// Command for requesting copy of the username credential.
        /// </summary>
        ICommand RequestCopyUsernameCommand { get; }

        /// <summary>
        /// Command for requesting copy of the password credential.
        /// </summary>
        ICommand RequestCopyPasswordCommand { get; }
    }
}
