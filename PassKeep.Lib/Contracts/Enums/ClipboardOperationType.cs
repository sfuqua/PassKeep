// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace PassKeep.Lib.Contracts.Enums
{
    /// <summary>
    /// Represents the type of data being interfaced with on the clipboard.
    /// </summary>
    public enum ClipboardOperationType
    {
        /// <summary>
        /// No operation.
        /// </summary>
        None,

        /// <summary>
        /// A username clipboard operation.
        /// </summary>
        UserName,

        /// <summary>
        /// A password clipboard operation.
        /// </summary>
        Password,

        /// <summary>
        /// A clipboard operation that isn't a credential.
        /// </summary>
        Other
    }
}
