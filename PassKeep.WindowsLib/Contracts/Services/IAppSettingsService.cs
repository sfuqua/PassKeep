using PassKeep.Lib.Contracts.Enums;
using System.ComponentModel;

namespace PassKeep.Lib.Contracts.Services
{
    public interface IAppSettingsService : INotifyPropertyChanged
    {
        /// <summary>
        /// Whether to automatically load a remembered document.
        /// </summary>
        bool AutoLoadEnabled { get; set; }

        /// <summary>
        /// Whether to show the sample document.
        /// </summary>
        bool SampleEnabled { get; set; }

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
        /// How to sort the document in the main view.
        /// </summary>
        DatabaseSortMode.Mode DatabaseSortMode { get; set; }
    }
}
