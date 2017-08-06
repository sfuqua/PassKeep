// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System.Threading.Tasks;
using Windows.Security.Credentials.UI;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// A service that validates that the user is the owner of a database.
    /// </summary>
    public interface IIdentityVerificationService
    {
        /// <summary>
        /// Determines whether the app is currently capable of validating identity.
        /// </summary>
        /// <returns>A task representing whether identity can be verified at this time.</returns>
        Task<UserConsentVerifierAvailability> CheckVerifierAvailabilityAsync();

        /// <summary>
        /// Verifies the user's identity.
        /// </summary>
        /// <returns>True if identity was verified, else false.</returns>
        Task<bool> VerifyIdentityAsync();
    }
}
