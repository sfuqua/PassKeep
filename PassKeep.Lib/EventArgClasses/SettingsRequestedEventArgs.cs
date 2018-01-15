// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;
using System;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// Provides a <see cref="IDatabaseSettingsViewModel"/> populated with
    /// settings for the current database.
    /// </summary>
    public class SettingsRequestedEventArgs : EventArgs
    {
        private readonly IDatabaseSettingsViewModel settingsViewModel;

        public SettingsRequestedEventArgs(IDatabaseSettingsViewModel viewModel)
        {
            this.settingsViewModel = viewModel;
        }

        /// <summary>
        /// Gets the Settings ViewModel associated with this event.
        /// </summary>
        public IDatabaseSettingsViewModel SettingsViewModel => this.settingsViewModel;
    }
}
