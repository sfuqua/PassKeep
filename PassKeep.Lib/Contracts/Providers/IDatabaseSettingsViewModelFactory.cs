// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.ViewModels;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// A factory that can create settings ViewModels based on different criteria.
    /// </summary>
    public interface IDatabaseSettingsViewModelFactory
    {
        /// <summary>
        /// Assembles a ViewModel with default settings, suitable for creating a new database.
        /// </summary>
        /// <returns></returns>
        IDatabaseSettingsViewModel Assemble();

        /// <summary>
        /// Assembles a ViewModel with the provided settings provider.
        /// </summary>
        /// <param name="settingsProvider"></param>
        /// <returns></returns>
        IDatabaseSettingsViewModel Assemble(IDatabaseSettingsProvider settingsProvider);
    }
}
