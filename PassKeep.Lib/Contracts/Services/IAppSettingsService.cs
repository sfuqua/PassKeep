using PassKeep.Lib.Contracts.Enums;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace PassKeep.Lib.Contracts.Services
{
    public interface IAppSettingsService : INotifyPropertyChanged
    {
        /// <summary>
        /// The theme to request at application start-up.
        /// </summary>
        ApplicationTheme AppTheme { get; set; }

        /// <summary>
        /// Whether to enable automatically clearing the clipboard on copy.
        /// </summary>
        bool EnableClipboardTimer { get; set; }

        /// <summary>
        /// How long (in seconds) it takes to clear the clipboard.
        /// </summary>
        uint ClearClipboardOnTimer { get; set; }

        /// <summary>
        /// Whether to lock the workspace after a timeout.
        /// </summary>
        bool EnableLockTimer { get; set; }

        /// <summary>
        /// How long (in seconds) it takes to lock the workspace.
        /// </summary>
        uint LockTimer { get; set; }

        /// <summary>
        /// Whether the app should display a MOTD with a change log.
        /// </summary>
        bool EnableMotd { get; set; }

        /// <summary>
        /// How to sort the document in the main view.
        /// </summary>
        DatabaseSortMode.Mode DatabaseSortMode { get; set; }
    }
}
