// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using System;
using System.IO;
using System.Text;

namespace PassKeep.Lib.KeePass.Helpers
{
    /// <summary>
    /// Helper for resolving placeholders as defined here:
    /// http://keepass.info/help/base/placeholders.html
    /// 
    /// KeePass 2.x placeholders are defined to be case INsensitive.
    /// 
    /// Intentionally unsupported for now:
    /// {APPDIR}
    /// {BROWSER} 
    /// {DB_*}
    /// {ENV_PROGRAMFILES} and environment variables
    /// Field references
    /// {PICKCHARS}
    /// {NEWPASSWORD}
    /// {HMACOTP}
    /// {BASE}
    /// {T-REPLACE-RX}
    /// {T-CONV}
    /// {CMD}
    /// </summary>
    public static class PlaceholderResolver
    {
        private const char PlaceholderOpen = '{';
        private const char PlaceholderClose = '}';

        /// <summary>
        /// Given a string, potentially with KeePass placeholders, and the entry the string
        /// is associated with, resolves all placeholders and returns a new string.
        /// </summary>
        /// <param name="input">The string to resolve.</param>
        /// <param name="entry">The entry associated with <paramref name="input"/>.</param>
        /// <returns>A new string with relevant placeholders replaced.</returns>
        public static string Resolve(string input, IKeePassEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder builder = new StringBuilder(input.Length);

            int len = input.Length;
            for (int i = 0; i < len; i++)
            {
                // If we are starting a new placeholder, look ahead in the string for
                // the end of it. 
                // If we find it:
                // * Append the placeholder's value to 'builder'
                // * Move 'i' to the end of the token
                // If we don't:
                // * Append the "placeholder" token to 'builder'
                // * Move 'i' to the end of the string, we're done
                if (input[i] == PlaceholderOpen)
                {
                    if (i == len - 1)
                    {
                        builder.Append(PlaceholderOpen);
                    }
                    else
                    {
                        StringBuilder tokenBuilder = new StringBuilder();
                        for (int j = i + 1; j < len; j++)
                        {
                            if (input[j] == PlaceholderOpen)
                            {
                                // We hit a "nested" token - we discard the work so far and start over.
                                builder.Append(PlaceholderOpen);
                                builder.Append(tokenBuilder.ToString());
                                tokenBuilder.Clear();

                                // If this is the last character we'll never finish a token, so go ahead
                                // and get this over with because we're about to exit the loop.
                                if (j == len - 1)
                                {
                                    builder.Append(PlaceholderOpen);
                                    i = j;
                                }
                            }
                            else if (input[j] == PlaceholderClose)
                            {
                                string token = tokenBuilder.ToString();
                                string replaced = ResolvePlaceholder(token, entry);
                                if (replaced == null)
                                {
                                    // If the token is not valid, we don't replace it.
                                    replaced = $"{PlaceholderOpen}{token}{PlaceholderClose}";
                                }

                                builder.Append(replaced);

                                // Move i forward now that we've evaluated it
                                i = j;
                                break;
                            }
                            else
                            {
                                tokenBuilder.Append(input[j]);
                                if (j == len - 1)
                                {
                                    // If we reached the end of the string, we don't have a
                                    // complete placeholder. Just append what we have and bail.
                                    builder.Append(PlaceholderOpen);
                                    builder.Append(tokenBuilder.ToString());
                                    i = j;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Otherwise we're not in a placeholder/token, so keep on trucking.
                    builder.Append(input[i]);
                }
            }

            return builder.ToString();
        }

        private static string ResolvePlaceholder(string token, IKeePassEntry entry)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            string placeholder = token;
            string specifier = null;

            int firstColon = token.IndexOf(':');
            if (firstColon >= 0)
            {
                placeholder = token.Substring(0, firstColon);
                specifier = token.Substring(firstColon + 1);
            }

            if (specifier == null)
            {
                if (placeholder.Equals("TITLE", StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Title.ClearValue;
                }
                else if (placeholder.Equals("USERNAME", StringComparison.OrdinalIgnoreCase))
                {
                    return entry.UserName.ClearValue;
                }
                else if (placeholder.Equals("URL", StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Url.ClearValue;
                }
                else if (placeholder.Equals("PASSWORD", StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Password.ClearValue;
                }
                else if (placeholder.Equals("NOTES", StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Notes.ClearValue;
                }
                else if (placeholder.Equals("GROUP", StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Parent?.Title.ClearValue;
                }
                else if (placeholder.Equals("GROUP_NOTES", StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Parent?.Notes.ClearValue;
                }
                else if (placeholder.Equals("ENV_DIRSEP", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.DirectorySeparatorChar.ToString();
                }
                else if (placeholder.Equals("DT_SIMPLE", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.Now.ToString("yyyyMMddHHmmss");
                }
                else if (placeholder.Equals("DT_YEAR", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.Now.ToString("yyyy");
                }
                else if (placeholder.Equals("DT_MONTH", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.Now.ToString("MM");
                }
                else if (placeholder.Equals("DT_DAY", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.Now.ToString("dd");
                }
                else if (placeholder.Equals("DT_HOUR", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.Now.ToString("HH");
                }
                else if (placeholder.Equals("DT_MINUTE", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.Now.ToString("mm");
                }
                else if (placeholder.Equals("DT_SECOND", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.Now.ToString("ss");
                }
                else if (placeholder.Equals("DT_UTC_SIMPLE", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                }
                else if (placeholder.Equals("DT_UTC_YEAR", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.UtcNow.ToString("yyyy");
                }
                else if (placeholder.Equals("DT_UTC_MONTH", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.UtcNow.ToString("MM");
                }
                else if (placeholder.Equals("DT_UTC_DAY", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.UtcNow.ToString("dd");
                }
                else if (placeholder.Equals("DT_UTC_HOUR", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.UtcNow.ToString("HH");
                }
                else if (placeholder.Equals("DT_UTC_MINUTE", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.UtcNow.ToString("mm");
                }
                else if (placeholder.Equals("DT_UTC_SECOND", StringComparison.OrdinalIgnoreCase))
                {
                    return DateTime.UtcNow.ToString("ss");
                }
            }
            else
            {
                if (placeholder.Equals("URL", StringComparison.OrdinalIgnoreCase))
                {
                    return GetUrlComponent(entry.Url.ClearValue, specifier);
                }
                else if (placeholder.Equals("C", StringComparison.OrdinalIgnoreCase))
                {
                    // C:foo is a comment
                    return string.Empty;
                }
            }

            // Not a valid placeholder
            return null;
        }

        /// <summary>
        /// Helper for getting a component of an URL based on KeePass placeholder
        /// syntax.
        /// </summary>
        /// <param name="url">The absolute URL to split apart.</param>
        /// <param name="specifier">The KeePass URL placeholder specifier.</param>
        /// <returns>Null if <paramref name="specifier"/> is invalid, else the
        /// specified component of <paramref name="url"/>. If the URL is invalid
        /// or the specified component does not exist, returns an empty string.</returns>
        public static string GetUrlComponent(string url, string specifier)
        {
            if (specifier == null)
            {
                throw new ArgumentNullException(nameof(specifier));
            }

            Uri thisUri;
            try
            {
                thisUri = new Uri(url, UriKind.Absolute);
            }
            catch (FormatException)
            {
                // Invalid URIs return empty strings for their components
                // This may differ from KeePass in some cases (e.g. absolute URI)
                return string.Empty;
            }


            if (specifier.Equals("RMVSCM", StringComparison.OrdinalIgnoreCase))
            {
                // "Remove Scheme"
                // Also removes the : and // if they exist
                string withoutScheme = thisUri.AbsoluteUri.Substring(thisUri.Scheme.Length);
                
                if (withoutScheme.Length == 0 || withoutScheme[0] != ':')
                {
                    return string.Empty;
                }

                int substrIndex = 1;
                if (withoutScheme.Length >= 3 &&
                    withoutScheme[1] == '/' && withoutScheme[2] == '/')
                {
                    substrIndex = 3;
                }

                return withoutScheme.Substring(substrIndex);
            }
            else if (specifier.Equals("SCM", StringComparison.OrdinalIgnoreCase))
            {
                return thisUri.Scheme;
            }
            else if (specifier.Equals("HOST", StringComparison.OrdinalIgnoreCase))
            {
                return thisUri.Host;
            }
            else if (specifier.Equals("PORT", StringComparison.OrdinalIgnoreCase))
            {
                return thisUri.Port.ToString();
            }
            else if (specifier.Equals("PATH", StringComparison.OrdinalIgnoreCase))
            {
                return thisUri.AbsolutePath;
            }
            else if (specifier.Equals("QUERY", StringComparison.OrdinalIgnoreCase))
            {
                return thisUri.Query;
            }
            else if (specifier.Equals("USERINFO", StringComparison.OrdinalIgnoreCase))
            {
                return thisUri.UserInfo;
            }
            else if (specifier.Equals("USERNAME", StringComparison.OrdinalIgnoreCase))
            {
                string userInfo = thisUri.UserInfo;
                int colonIndex = userInfo.IndexOf(':');
                if (colonIndex < 0)
                {
                    return userInfo;
                }

                return userInfo.Substring(0, colonIndex);
            }
            else if (specifier.Equals("PASSWORD", StringComparison.OrdinalIgnoreCase))
            {
                string userInfo = thisUri.UserInfo;
                int colonIndex = userInfo.IndexOf(':');
                if (colonIndex < 0 || colonIndex == userInfo.Length - 1)
                {
                    return string.Empty;
                }

                return userInfo.Substring(colonIndex + 1);
            }
            else
            {
                // Not a valid specifier
                return null;
            }
        }
    }
}
