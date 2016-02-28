using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel used to interact with the app's settings.
    /// </summary>
    public class AppSettingsViewModel : AbstractViewModel, IAppSettingsViewModel
    {
        private IAppSettingsService settingsService;

        private ApplicationTheme _selectedTheme;
        private bool _clipboardClearTimerEnabled, _idleLockTimerEnabled, _motdEnabled;
        private int _clipboardClearTimerMax, _idleLockTimerMax;

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
            this._clipboardClearTimerEnabled = settingsService.EnableClipboardTimer;
            this._idleLockTimerEnabled = settingsService.EnableLockTimer;
            this._clipboardClearTimerMax = (int)settingsService.ClearClipboardOnTimer;
            this._idleLockTimerMax = (int)settingsService.LockTimer;
            this._motdEnabled = settingsService.EnableMotd;
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

        /// <summary>
        /// Whether the clipboard automatically clears after a timeout.
        /// </summary>
        public bool ClipboardClearTimerEnabled
        {
            get { return this._clipboardClearTimerEnabled; }
            set
            {
                if (TrySetProperty(ref this._clipboardClearTimerEnabled, value))
                {
                    this.settingsService.EnableClipboardTimer = value;
                }
            }
        }

        /// <summary>
        /// How long to wait before clearing the clipboard if <see cref="ClipboardClearTimerEnabled"/> is enabled.
        /// </summary>
        public int ClipboardClearTimerMaxInSeconds
        {
            get { return this._clipboardClearTimerMax; }
            set
            {
                if (TrySetProperty(ref this._clipboardClearTimerMax, value))
                {
                    this.settingsService.ClearClipboardOnTimer = (uint)value;
                }
            }
        }

        /// <summary>
        /// Whether the workspace automatically locks after a set idle period.
        /// </summary>
        public bool LockIdleTimerEnabled
        {
            get { return this._idleLockTimerEnabled; }
            set
            {
                if (TrySetProperty(ref this._idleLockTimerEnabled, value))
                {
                    this.settingsService.EnableLockTimer = value;
                }
            }
        }

        /// <summary>
        /// How long to wait before locking the workspace if <see cref="LockIdleTimerEnabled"/> is enabled.
        /// </summary>
        public int LockIdleTimerMaxInSeconds
        {
            get { return this._idleLockTimerMax; }
            set
            {
                if (TrySetProperty(ref this._idleLockTimerMax, value))
                {
                    this.settingsService.LockTimer = (uint)value;
                }
            }
        }

        /// <summary>
        /// Whether the app should display a version history MOTD on new releases.
        /// </summary>
        public bool MotdEnabled
        {
            get { return this._motdEnabled; }
            set
            {
                if (TrySetProperty(ref this._motdEnabled, value))
                {
                    this.settingsService.EnableMotd = value;
                }
            }
        }
    }
}
