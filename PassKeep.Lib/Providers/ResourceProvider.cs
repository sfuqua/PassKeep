﻿using PassKeep.Lib.Contracts.Providers;
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
            if (resourceLoader == null)
            {
                throw new ArgumentNullException(nameof(resourceLoader));
            }

            this.resourceLoader = resourceLoader;
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