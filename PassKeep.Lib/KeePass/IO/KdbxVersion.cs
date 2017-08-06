// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace PassKeep.Lib.KeePass.IO
{
    /// <summary>
    /// Handles switching parser logic based on the file format.
    /// </summary>
    public enum KdbxVersion
    {
        /// <summary>
        /// Unknown/unspecified version.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Legacy 3.0/3.1 KDBX file format.
        /// </summary>
        Three,

        /// <summary>
        /// Modern (as of KP 2.35) KDBX 4 file format.
        /// </summary>
        Four
    }
}
