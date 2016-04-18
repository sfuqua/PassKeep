using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// A ViewModel for a settings page or pane.
    /// </summary>
    public interface IAppSettingsViewModel : IViewModel
    {
        /// <summary>
        /// The collection of themes available to be chosen.
        /// </summary>
        IReadOnlyCollection<ApplicationTheme> Themes { get; }

        /// <summary>
        /// The theme chosen by the user.
        /// </summary>
        ApplicationTheme SelectedTheme { get; set; }

        /// <summary>
        /// Whether the clipboard automatically clears after a timeout.
        /// </summary>
        bool ClipboardClearTimerEnabled { get; set; }

        /// <summary>
        /// How long to wait before clearing the clipboard if <see cref="ClipboardClearTimerEnabled"/> is enabled.
        /// </summary>
        int ClipboardClearTimerMaxInSeconds { get; set; }

        /// <summary>
        /// Whether the workspace automatically locks after a set idle period.
        /// </summary>
        bool LockIdleTimerEnabled { get; set; }

        /// <summary>
        /// How long to wait before locking the workspace if <see cref="LockIdleTimerEnabled"/> is enabled.
        /// </summary>
        int LockIdleTimerMaxInSeconds { get; set; }

        /// <summary>
        /// Whether to enable showing a message-of-the-day on updates.
        /// </summary>
        bool MotdEnabled { get; set; }

        /// <summary>
        /// Whether to copy an entry's password when its URL is opened.
        /// </summary>
        bool CopyPasswordOnUrlLaunch { get; set; }

        /// <summary>
        /// Gets a ViewModel for managing saved credentials.
        /// </summary>
        /// <returns>A task that completes when the ViewModel is ready to use.</returns>
        Task<ISavedCredentialsViewModel> GetSavedCredentialsViewModelAsync();
    }
}
