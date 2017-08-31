// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Providers;
using System;
using Windows.ApplicationModel.Resources;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// Provides resources by wrapping a <see cref="ResourceLoader"/>.
    /// </summary>
    public class ResourceProvider : IResourceProvider
    {
        private ResourceLoader resourceLoader;

        /// <summary>
        /// Constructs an instance using the given <see cref="ResourceLoader"/>.
        /// </summary>
        /// <param name="resourceLoader">The <see cref="ResourceLoader"/> to wrap.</param>
        public ResourceProvider(ResourceLoader resourceLoader)
        {
            this.resourceLoader = resourceLoader ?? throw new ArgumentNullException(nameof(resourceLoader));
        }

        /// <summary>
        /// Fetches a string from the underlying <see cref="ResourceLoader"/>.
        /// </summary>
        /// <param name="key">The string to fetch.</param>
        /// <returns>The fetched string resource.</returns>
        public string GetString(string key)
        {
            return this.resourceLoader.GetString(key);
        }
    }
}
