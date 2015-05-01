using PassKeep.Lib.Contracts.Providers;
using System.Collections.Generic;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// A simple settings provider that keeps all settings in memory.
    /// </summary>
    public class InMemorySettingsProvider : ISettingsProvider
    {
        private Dictionary<string, object> storage;

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public InMemorySettingsProvider()
        {
            this.storage = new Dictionary<string, object>();
        }

        /// <summary>
        /// Returns whether the provided key exists in the settings of the app.
        /// </summary>
        /// <param name="key">The lookup key.</param>
        /// <returns>Whether the data exists in the settings provider.</returns>
        public bool Contains(string key)
        {
            return this.storage.ContainsKey(key);
        }

        /// <summary>
        /// Adds or overwrites the provided settings key with the provided value.
        /// </summary>
        /// <param name="key">The lookup key.</param>
        /// <param name="value">The value to store.</param>
        public void Set(string key, object value)
        {
            this.storage[key] = value;
        }

        /// <summary>
        /// Gets a setting, or the provided default if it doesn't exist.
        /// </summary>
        /// <param name="key">The lookup key.</param>
        /// <param name="defaultValue">The default value to return, if the data cannot be found.</param>
        /// <returns>The value corresponding to <paramref name="key"/> if it exists,
        /// else <paramref name="defaultValue"/></returns>
        public T Get<T>(string key, T defaultValue)
        {
            if (!this.storage.ContainsKey(key))
            {
                return defaultValue;
            }

            object value = this.storage[key];
            if (value is T)
            {
                return (T)value;
            }

            return defaultValue;
        }
    }
}
