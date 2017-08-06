// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Provides an interface for fetching localized resources.
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// Returns a string resource given a lookup.
        /// </summary>
        /// <param name="key">The resource to fetch.</param>
        /// <returns>The fetched resource.</returns>
        string GetString(string key);
    }
}
