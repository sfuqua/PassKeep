// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using PassKeep.Lib.Contracts.Providers;
using Windows.Storage;

namespace PassKeep.Lib.Providers
{
    public class RoamingAppDataSettingsProvider : ISettingsProvider
    {
        private ApplicationDataContainer container;

        public bool Contains(string key)
        {
            return this.container.Values.ContainsKey(key);
        }

        public void Set(string key, object value)
        {
            this.container.Values[key] = value;
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (!Contains(key))
            {
                return defaultValue;
            }

            object value = this.container.Values[key];
            if (value is T)
            {
                return (T)value;
            }
            else
            {
                return defaultValue;
            }
        }

        public RoamingAppDataSettingsProvider()
        {
            this.container = ApplicationData.Current.RoamingSettings;
        }
    }
}
