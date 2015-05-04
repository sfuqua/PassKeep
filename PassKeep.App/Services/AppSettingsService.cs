using System;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using Windows.Storage.AccessCache;
using PassKeep.Lib.Contracts.Enums;
using SariphLib.Mvvm;
using SariphLib.Infrastructure;

namespace PassKeep.Lib.Services
{
    public class AppSettingsService : BindableBase, IAppSettingsService
    {
        private ISettingsProvider _provider;

        #region Unique keys/identifiers for settings

        private const string AutoLoadSetting = "AutoLoad";
        private const string DatabaseToken = "{14DC5685-34C4-4E54-8807-4E0E7CA23839}";
        private const string SampleSetting = "ShowSample";
        private const string ClearClipboardExitSetting = "ClearOnExit";
        private const string EnableClearClipboardTimerSetting = "EnableClipboardTimer";
        private const string ClearClipboardTimerSetting = "ClearOnTimer";
        private const string EnableLockTimerSetting = "EnableLockTimer";
        private const string LockTimerSetting = "LockTimer";
        private const string DatabaseSortModeSetting = "SortMode";

        #endregion

        private bool _autoLoadEnabled;
        public bool AutoLoadEnabled
        {
            get { return _autoLoadEnabled; }
            set
            {
                _autoLoadEnabled = value;
                _provider.Set(AutoLoadSetting, value);

                if (value == false &&
                    StorageApplicationPermissions.FutureAccessList.ContainsItem(DatabaseToken))
                {
                    StorageApplicationPermissions.FutureAccessList.Remove(DatabaseToken);
                }
            }
        }

        private bool _sampleEnabled;
        public bool SampleEnabled
        {
            get { return _sampleEnabled; }
            set
            {
                if (TrySetProperty(ref this._sampleEnabled, value))
                {
                    this._provider.Set(SampleSetting, value);
                }
            }
        }

        private bool _enableClipboardTimer;
        public bool EnableClipboardTimer
        {
            get
            {
                return _enableClipboardTimer;
            }
            set
            {
                if (TrySetProperty(ref this._enableClipboardTimer, value))
                {
                    _provider.Set(EnableClearClipboardTimerSetting, value);
                    if (value && ClearClipboardOnTimer == 0)
                    {
                        ClearClipboardOnTimer = 1;
                    }
                }
            }
        }

        private uint _clearClipboardOnTimer;
        public uint ClearClipboardOnTimer
        {
            get { return _clearClipboardOnTimer; }
            set
            {
                if (TrySetProperty(ref this._clearClipboardOnTimer, value))
                {
                    _provider.Set(ClearClipboardTimerSetting, value);
                    if (value == 0)
                    {
                        EnableClipboardTimer = false;
                    }
                }
            }
        }

        private bool _enableLockTimer;
        public bool EnableLockTimer
        {
            get { return _enableLockTimer; }
            set
            {
                if (TrySetProperty(ref this._enableLockTimer, value))
                {
                    _provider.Set(EnableLockTimerSetting, value);
                    if (value && LockTimer == 0)
                    {
                        LockTimer = 1;
                    }
                }
            }
        }

        private uint _lockTimer;
        public uint LockTimer
        {
            get { return _lockTimer; }
            set
            {
                if (TrySetProperty(ref this._lockTimer, value))
                {
                    _provider.Set(LockTimerSetting, value);
                    if (value == 0)
                    {
                        EnableLockTimer = false;
                    }
                }
            }
        }

        private DatabaseSortMode.Mode _databaseSortMode;
        public DatabaseSortMode.Mode DatabaseSortMode
        {
            get { return _databaseSortMode; }
            set
            {
                if (TrySetProperty(ref this._databaseSortMode, value))
                {
                    _provider.Set(DatabaseSortModeSetting, (int)value);
                }
            }
        }

        public AppSettingsService(ISettingsProvider provider)
        {
            Dbg.Assert(provider != null);
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _provider = provider;

            AutoLoadEnabled = _provider.Get(AutoLoadSetting, true);
            SampleEnabled = _provider.Get(SampleSetting, true);

            ClearClipboardOnTimer = _provider.Get<uint>(ClearClipboardTimerSetting, 12);
            EnableClipboardTimer = _provider.Get(EnableClearClipboardTimerSetting, true);

            LockTimer = _provider.Get<uint>(LockTimerSetting, 60 * 5);
            EnableLockTimer = _provider.Get(EnableLockTimerSetting, true);

            DatabaseSortMode.Mode defaultMode = PassKeep.Lib.Contracts.Enums.DatabaseSortMode.Mode.DatabaseOrder;
            int iSortMode = _provider.Get<int>(DatabaseSortModeSetting, (int)defaultMode);
            if (!Enum.IsDefined(typeof(DatabaseSortMode.Mode), iSortMode))
            {
                DatabaseSortMode = defaultMode;
            }
            else
            {
                DatabaseSortMode = (DatabaseSortMode.Mode)iSortMode;
            }
        }
    }
}
