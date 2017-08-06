// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using System;

namespace PassKeep.Lib.KeePass.Helpers
{
    /// <summary>
    /// Helper class for handling URL launching logic.
    /// </summary>
    /// <remarks>
    /// See http://keepass.info/help/base/autourl.html for details. Not all of this is implemented, but this class
    /// can help with more (i.e. placeholders) in the future.
    /// </remarks>
    public static class KdbxUrlResolver
    {
        /// <summary>
        /// Attempts to construct a URI string from either <paramref name="entry"/>'s URL or override URL.
        /// </summary>
        /// <param name="entry">The entry to construct a URI for.</param>
        /// <returns>The constructed URI string.</returns>
        public static string ConstructUriString(this IKeePassEntry entry)
        {
            string uriCandidate = entry.OverrideUrl;
            if (String.IsNullOrEmpty(uriCandidate))
            {
                uriCandidate = entry.Url?.ClearValue ?? string.Empty;
            }
            
            return PlaceholderResolver.Resolve(uriCandidate, entry);
        }

        /// <summary>
        /// Attempts to construct a URI from either <paramref name="entry"/>'s URL or override URL.
        /// </summary>
        /// <param name="entry">The entry to construct a URI for.</param>
        /// <returns>A launchable URI, or null.</returns>
        public static Uri GetLaunchableUri(this IKeePassEntry entry)
        {
            string uriCandidate = entry.ConstructUriString();
            if (uriCandidate == null)
            {
                return null;
            }

            try
            {
                Uri uri = new Uri(uriCandidate);
                return uri;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
