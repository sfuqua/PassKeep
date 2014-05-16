﻿using System;
using PassKeep.Lib.Contracts.Providers;
using Windows.Storage;

namespace PassKeep.Lib.Providers
{
    public class RoamingAppDataSettingsProvider : ISettingsProvider
    {
        private ApplicationDataContainer container;

        public bool Contains(string key)
        {
            return container.Values.ContainsKey(key);
        }

        public void Set(string key, object value)
        {
            container.Values[key] = value;
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (!Contains(key))
            {
                return defaultValue;
            }

            object value = container.Values[key];
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
            container = ApplicationData.Current.RoamingSettings;
        }
    }
}