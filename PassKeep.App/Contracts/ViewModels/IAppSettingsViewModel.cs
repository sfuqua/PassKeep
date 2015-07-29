using System.Collections.Generic;
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
    }
}
