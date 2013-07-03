using System;
using PassKeep.Common;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace PassKeep.ViewModels
{
    public class ConfigurationViewModel : BindableBase
    {
        public const string AutoLoadSetting = "AutoLoad";
        public const string DatabaseToken = "{14DC5685-34C4-4E54-8807-4E0E7CA23839}";
        public const string SampleSetting = "ShowSample";
        public const string ClearClipboardExitSetting = "ClearOnExit";
        public const string EnableClearClipboardTimerSetting = "EnableClipboardTimer";
        public const string ClearClipboardTimerSetting = "ClearOnTimer";
        public const string EnableLockTimerSetting = "EnableLockTimer";
        public const string LockTimerSetting = "LockTimer";
        public const string DatabaseSortModeSetting = "SortMode";

        private ApplicationDataContainer settings
            = ApplicationData.Current.RoamingSettings;

        private bool _autoLoadEnabled;
        public bool AutoLoadEnabled
        {
            get { return _autoLoadEnabled; }
            set
            {
                if (SetProperty(ref _autoLoadEnabled, value))
                {
                    settings.Values[AutoLoadSetting] = value;
                    if (value == false && 
                        StorageApplicationPermissions.FutureAccessList.ContainsItem(DatabaseToken))
                    {
                        StorageApplicationPermissions.FutureAccessList.Remove(DatabaseToken);
                    }
                }
            }
        }

        private bool _sampleEnabled;
        public bool SampleEnabled
        {
            get { return _sampleEnabled; }
            set
            {
                if (SetProperty(ref _sampleEnabled, value))
                {
                    settings.Values[SampleSetting] = value;
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
                if (SetProperty(ref _enableClipboardTimer, value))
                {
                    settings.Values[EnableClearClipboardTimerSetting] = value;
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
                if (SetProperty(ref _clearClipboardOnTimer, value))
                {
                    settings.Values[ClearClipboardTimerSetting] = value;
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
                if (SetProperty(ref _enableLockTimer, value))
                {
                    settings.Values[EnableLockTimerSetting] = value;
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
                if (SetProperty(ref _lockTimer, value))
                {
                    settings.Values[LockTimerSetting] = value;
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
                if (SetProperty(ref _databaseSortMode, value))
                {
                    settings.Values[DatabaseSortModeSetting] = (int)value;
                }
            }
        }

        public ConfigurationViewModel()
        {
            var settings = ApplicationData.Current.RoamingSettings.Values;

            bool? autoloadSetting = settings[ConfigurationViewModel.AutoLoadSetting] as bool?;
            AutoLoadEnabled = autoloadSetting ?? true;

            bool? sampleSetting = settings[ConfigurationViewModel.SampleSetting] as bool?;
            SampleEnabled = sampleSetting ?? true;

            uint? clipboardTimerSetting = settings[ConfigurationViewModel.ClearClipboardTimerSetting] as uint?;
            ClearClipboardOnTimer = clipboardTimerSetting ?? 12;
            bool? enableClipboardTimerSetting = settings[ConfigurationViewModel.EnableClearClipboardTimerSetting] as bool?;
            EnableClipboardTimer = enableClipboardTimerSetting ?? true;

            uint? suspendLockSetting = settings[ConfigurationViewModel.LockTimerSetting] as uint?;
            LockTimer = suspendLockSetting ?? (60 * 5);
            bool? enabledLockTimerSetting = settings[ConfigurationViewModel.EnableLockTimerSetting] as bool?;
            EnableLockTimer = enabledLockTimerSetting ?? true;

            //bool? enableBackupSetting = settings[ConfigurationViewModel.EnableBackupSetting] as bool?;
            //EnableBackup = enableBackupSetting ?? false;

            int? iSortMode = settings[ConfigurationViewModel.DatabaseSortModeSetting] as int?;
            if (!iSortMode.HasValue || !Enum.IsDefined(typeof(DatabaseSortMode.Mode), iSortMode.Value))
            {
                DatabaseSortMode = PassKeep.ViewModels.DatabaseSortMode.Mode.DatabaseOrder;
            }
            else
            {
                DatabaseSortMode = (DatabaseSortMode.Mode)iSortMode;
            }
        }
    }
}
