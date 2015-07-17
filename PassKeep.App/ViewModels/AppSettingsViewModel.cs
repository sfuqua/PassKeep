using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel used to interact with the app's settings.
    /// </summary>
    public class AppSettingsViewModel : BindableBase, IAppSettingsViewModel
    {
        private IAppSettingsService settingsService;

        private ApplicationTheme _selectedTheme;

        /// <summary>
        /// Constructs the ViewModel.
        /// </summary>
        /// <param name="settingsService">Provides access to the app's settings.</param>
        public AppSettingsViewModel(IAppSettingsService settingsService)
        {
            if (settingsService == null)
            {
                throw new ArgumentNullException(nameof(settingsService));
            }

            this.settingsService = settingsService;

            this.Themes = new List<ApplicationTheme>
            {
                ApplicationTheme.Dark,
                ApplicationTheme.Light
            };

            this._selectedTheme = settingsService.AppTheme;
        }

        /// <summary>
        /// The collection of themes available to be chosen.
        /// </summary>
        public IReadOnlyCollection<ApplicationTheme> Themes
        {
            get;
            private set;
        }

        /// <summary>
        /// The theme chosen by the user.
        /// </summary>
        public ApplicationTheme SelectedTheme
        {
            get
            {
                return this._selectedTheme;
            }
            set
            {
                if (TrySetProperty(ref this._selectedTheme, value))
                {
                    this.settingsService.AppTheme = value;
                }
            }
        }
    }
}
