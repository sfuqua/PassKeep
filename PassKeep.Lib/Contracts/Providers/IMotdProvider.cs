// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Models;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Provides a message-of-the-day for users.
    /// </summary>
    public interface IMotdProvider
    {
        /// <summary>
        /// Gets a <see cref="MessageOfTheDay"/> for immediate display.
        /// Subsequent calls will return a MOTD that is set not to display.
        /// </summary>
        /// <returns>A <see cref="MessageOfTheDay"/> that may or may not be set
        /// to display, but subsequent calls on the same build should not display.</returns>
        MessageOfTheDay GetMotdForDisplay();
    }
}
