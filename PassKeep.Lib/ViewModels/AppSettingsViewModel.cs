using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using PassKeep.Lib.Contracts.Providers;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// A ViewModel used to interact with the app's settings.
    /// </summary>
    public class AppSettingsViewModel : AbstractViewModel, IAppSettingsViewModel
    {
        private readonly IAppSettingsService settingsService;
        private readonly ICachedFilesViewModelFactory cachedFilesViewModelFactory;
        private readonly ISavedCredentialsViewModelFactory savedCredentialsViewModelFactory;

        private ApplicationTheme _selectedTheme;
        private bool _clipboardClearTimerEnabled, _idleLockTimerEnabled, _motdEnabled, _copyPasswordOnUrl;
        private int _clipboardClearTimerMax, _idleLockTimerMax;

        /// <summary>
        /// Constructs the ViewModel.
        /// </summary>
        /// <param name="settingsService">Provides access to the app's settings.</param>
        /// <param name="cachedFilesViewModelFactory">ViewModel factory for managing cached files.</param>
        /// <param name="savedCredentialsViewModelFactory">ViewModel factory for managing saved credentials.</param>
        public AppSettingsViewModel(
            IAppSettingsService settingsService,
            ICachedFilesViewModelFactory cachedFilesViewModelFactory,
            ISavedCredentialsViewModelFactory savedCredentialsViewModelFactory
        )
        {
            if (settingsService == null)
            {
                throw new ArgumentNullException(nameof(settingsService));
            }

            if (cachedFilesViewModelFactory == null)
            {
                throw new ArgumentNullException(nameof(cachedFilesViewModelFactory));
            }

            if (savedCredentialsViewModelFactory == null)
            {
                throw new ArgumentNullException(nameof(savedCredentialsViewModelFactory));
            }

            this.settingsService = settingsService;
            this.cachedFilesViewModelFactory = cachedFilesViewModelFactory;
            this.savedCredentialsViewModelFactory = savedCredentialsViewModelFactory;

            Themes = new List<ApplicationTheme>
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
            this._copyPasswordOnUrl = settingsService.CopyPasswordOnUrlOpen;
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

        /// <summary>
        /// Whether to copy an entry's password when its URL is opened.
        /// </summary>
        public bool CopyPasswordOnUrlLaunch
        {
            get { return this._copyPasswordOnUrl; }
            set
            {
                if (TrySetProperty(ref this._copyPasswordOnUrl, value))
                {
                    this.settingsService.CopyPasswordOnUrlOpen = value;
                }
            }
        }

        /// <summary>
        /// Gets a ViewModel for managing cached files.
        /// </summary>
        /// <returns>A task that completes when the ViewModel is ready to use.</returns>
        public Task<ICachedFilesViewModel> GetCachedFilesViewModelAsync()
        {
            return this.cachedFilesViewModelFactory.AssembleAsync();
        }

        /// <summary>
        /// Gets a ViewModel for managing saved credentials.
        /// </summary>
        /// <returns>A task that completes when the ViewModel is ready to use.</returns>
        public Task<ISavedCredentialsViewModel> GetSavedCredentialsViewModelAsync()
        {
            return this.savedCredentialsViewModelFactory.AssembleAsync();
        }
    }
}
