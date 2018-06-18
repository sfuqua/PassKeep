// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace PassKeep.Lib.Contracts.Enums
{
    /// <summary>
    /// Status codes returned from an attempt to update database credentials.
    /// </summary>
    public enum CredentialUpdateResult
    {
        /// <summary>
        /// Credentials were updated successfully.
        /// </summary>
        Success,

        /// <summary>
        /// There was a problem accessing the credential vault.
        /// </summary>
        CredentialVaultError
    }
}
