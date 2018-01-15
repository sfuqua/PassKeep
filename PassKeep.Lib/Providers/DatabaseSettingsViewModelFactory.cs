// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.ViewModels;

namespace PassKeep.Lib.Providers
{
    public class DatabaseSettingsViewModelFactory : IDatabaseSettingsViewModelFactory
    {
        /// <summary>
        /// Assembles a ViewModel with default settings, suitable for creating a new database.
        /// </summary>
        /// <returns></returns>
        public IDatabaseSettingsViewModel Assemble()
        {
            return Assemble(new InMemoryDatabaseSettingsProvider());
        }

        /// <summary>
        /// Assembles a ViewModel with the provided settings provider.
        /// </summary>
        /// <param name="settingsProvider"></param>
        /// <returns></returns>
        public IDatabaseSettingsViewModel Assemble(IDatabaseSettingsProvider settingsProvider)
        {
            return new DatabaseSettingsViewModel(settingsProvider);
        }
    }
}
