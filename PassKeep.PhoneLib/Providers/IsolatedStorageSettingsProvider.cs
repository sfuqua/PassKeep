using System.IO.IsolatedStorage;
using PassKeep.Lib.Contracts.Providers;

namespace PassKeep.Lib.Providers
{
    public class IsolatedStorageSettingsProvider : ISettingsProvider
    {
        private IsolatedStorageSettings settings;

        public bool Contains(string key)
        {
            return settings.Contains(key);
        }

        public void Set(string key, object value)
        {
            settings[key] = value;
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (!Contains(key))
            {
                return defaultValue;
            }

            object value = settings[key];
            if (value is T)
            {
                return (T)value;
            }
            else
            {
                return defaultValue;
            }
        }

        public IsolatedStorageSettingsProvider()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
        }
    }
}
