using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.ViewModels;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace PassKeep.Contracts.ViewModels
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
    }
}
