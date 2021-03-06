﻿// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using Windows.UI.Xaml;

namespace PassKeep.Lib.Services
{
    public class AppSettingsService : BindableBase, IAppSettingsService
    {
        #region Unique keys/identifiers for settings

        private const string AppThemeSetting = "AppTheme";
        private const string ClearClipboardExitSetting = "ClearOnExit";
        private const string EnableClearClipboardTimerSetting = "EnableClipboardTimer";
        private const string ClearClipboardTimerSetting = "ClearOnTimer";
        private const string EnableLockTimerSetting = "EnableLockTimer";
        private const string LockTimerSetting = "LockTimer";
        private const string DatabaseSortModeSetting = "SortMode";
        private const string EnableMotdSetting = "EnableMotd";
        private const string CopyPasswordOnUrlLaunchSetting = "PasswordOnUrl";

        #endregion

        private ISettingsProvider settingsProvider;

        // Property-backing fields
        private ApplicationTheme _appTheme;
        private bool _enableClipboardTimer;
        private uint _clearClipboardOnTimer;
        private bool _enableLockTimer;
        private uint _lockTimer;
        private bool _enableMotd;
        private bool _copyPasswordOnUrlLaunch;
        private DatabaseSortMode.Mode _databaseSortMode;

        public AppSettingsService(ISettingsProvider settingsProvider)
        {
            DebugHelper.Assert(settingsProvider != null);
            this.settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));

            // Initialize settings properties, using default values if necessary.
            ApplicationTheme defaultTheme = ApplicationTheme.Dark;
            int iTheme = settingsProvider.Get<int>(AppThemeSetting, (int)defaultTheme);
            if (!Enum.IsDefined(typeof(ApplicationTheme), iTheme))
            {
                AppTheme = defaultTheme;
            }
            else
            {
                AppTheme = (ApplicationTheme)iTheme;
            }

            ClearClipboardOnTimer = settingsProvider.Get<uint>(ClearClipboardTimerSetting, 12);
            EnableClipboardTimer = settingsProvider.Get(EnableClearClipboardTimerSetting, true);

            LockTimer = settingsProvider.Get<uint>(LockTimerSetting, 60 * 5);
            EnableLockTimer = settingsProvider.Get(EnableLockTimerSetting, true);

            EnableMotd = settingsProvider.Get(EnableMotdSetting, true);
            CopyPasswordOnUrlOpen = settingsProvider.Get(CopyPasswordOnUrlLaunchSetting, false);

            DatabaseSortMode.Mode defaultMode = Contracts.Enums.DatabaseSortMode.Mode.DatabaseOrder;
            int iSortMode = settingsProvider.Get<int>(DatabaseSortModeSetting, (int)defaultMode);
            if (!Enum.IsDefined(typeof(DatabaseSortMode.Mode), iSortMode))
            {
                DatabaseSortMode = defaultMode;
            }
            else
            {
                DatabaseSortMode = (DatabaseSortMode.Mode)iSortMode;
            }
        }

        /// <summary>
        /// The theme to request at application start-up.
        /// </summary>
        public ApplicationTheme AppTheme
        {
            get { return this._appTheme; }
            set
            {
                if (TrySetProperty(ref this._appTheme, value))
                {
                    this.settingsProvider.Set(AppThemeSetting, (int)value);
                }
            }
        }

        /// <summary>
        /// Whether to enable the feature that clears the clipboard after a set amount of time.
        /// </summary>
        /// <remarks>Enabling this when the clipboard timer is 0 sets it to 1.</remarks>
        public bool EnableClipboardTimer
        {
            get
            {
                return this._enableClipboardTimer;
            }
            set
            {
                if (TrySetProperty(ref this._enableClipboardTimer, value))
                {
                    this.settingsProvider.Set(EnableClearClipboardTimerSetting, value);
                    if (value && ClearClipboardOnTimer == 0)
                    {
                        ClearClipboardOnTimer = 1;
                    }
                }
            }
        }

        /// <summary>
        /// The amount of time (in seconds) to wait before clearing the clipboard.
        /// </summary>
        /// <remarks>Settings this to 0 disables the feature.</remarks>
        public uint ClearClipboardOnTimer
        {
            get { return this._clearClipboardOnTimer; }
            set
            {
                if (TrySetProperty(ref this._clearClipboardOnTimer, value))
                {
                    this.settingsProvider.Set(ClearClipboardTimerSetting, value);
                    if (value == 0)
                    {
                        EnableClipboardTimer = false;
                    }
                }
            }
        }

        /// <summary>
        /// Whether to enable locking the active workspace after a set amount of inactive time.
        /// </summary>
        /// <remarks>Enabling this when the lock timer is 0 sets it to 1.</remarks>
        public bool EnableLockTimer
        {
            get { return this._enableLockTimer; }
            set
            {
                if (TrySetProperty(ref this._enableLockTimer, value))
                {
                    this.settingsProvider.Set(EnableLockTimerSetting, value);
                    if (value && LockTimer == 0)
                    {
                        LockTimer = 1;
                    }
                }
            }
        }

        /// <summary>
        /// The amount of time (in seconds) to wait before locking an active workspace after inactivity.
        /// </summary>
        /// <remarks>Settings this to 0 disables the feature.</remarks>
        public uint LockTimer
        {
            get { return this._lockTimer; }
            set
            {
                if (TrySetProperty(ref this._lockTimer, value))
                {
                    this.settingsProvider.Set(LockTimerSetting, value);
                    if (value == 0)
                    {
                        EnableLockTimer = false;
                    }
                }
            }
        }

        /// <summary>
        /// Whether the app should display a MOTD with a change log.
        /// </summary>
        public bool EnableMotd
        {
            get { return this._enableMotd; }
            set
            {
                if (TrySetProperty(ref this._enableMotd, value))
                {
                    this.settingsProvider.Set(EnableMotdSetting, value);
                }
            }
        }

        /// <summary>
        /// Whether the app should copy an entry's password when opening it's URL.
        /// </summary>
        public bool CopyPasswordOnUrlOpen
        {
            get { return this._copyPasswordOnUrlLaunch; }
            set
            {
                if (TrySetProperty(ref this._copyPasswordOnUrlLaunch, value))
                {
                    this.settingsProvider.Set(CopyPasswordOnUrlLaunchSetting, value);
                }
            }
        }

        public DatabaseSortMode.Mode DatabaseSortMode
        {
            get { return this._databaseSortMode; }
            set
            {
                if (TrySetProperty(ref this._databaseSortMode, value))
                {
                    this.settingsProvider.Set(DatabaseSortModeSetting, (int)value);
                }
            }
        }
    }
}
