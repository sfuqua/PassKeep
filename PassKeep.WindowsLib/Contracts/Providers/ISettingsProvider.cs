using System;

namespace PassKeep.Lib.Contracts.Providers
{
    public interface ISettingsProvider
    {
        /// <summary>
        /// Returns whether the provided key exists in the settings of the app.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Contains(string key);

        /// <summary>
        /// Adds or overwrites the provided settings key with the provided value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set(string key, object value);

        /// <summary>
        /// Gets a setting, or the provided default if it doesn't exist.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T Get<T>(string key, T defaultValue);
    }
}
