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
