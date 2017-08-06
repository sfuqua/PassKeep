// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace PassKeep.Lib.Contracts.Enums
{
    /// <summary>
    /// How the app was activated.
    /// </summary>
    public enum ActivationMode
    {
        /// <summary>
        /// Activated via a standard app launch.
        /// </summary>
        Regular,

        /// <summary>
        /// Activated via global search.
        /// </summary>
        Search,

        /// <summary>
        /// Activated via opening a file.
        /// </summary>
        File
    }
}
