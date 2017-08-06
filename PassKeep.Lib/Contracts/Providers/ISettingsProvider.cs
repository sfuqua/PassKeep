// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// An interface for storing and retrieving data from some sort of storage.
    /// </summary>
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
