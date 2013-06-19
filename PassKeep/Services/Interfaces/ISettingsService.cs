namespace PassKeep.Services.Interfaces
{
    public interface ISettingsService
    {
        /// <summary>
        /// Whether to automatically load a remembered database.
        /// </summary>
        bool AutoLoadEnabled { get; set; }

        /// <summary>
        /// Whether to show the sample database.
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
    }
}
