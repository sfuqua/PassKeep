using PassKeep.Lib.Contracts.Services;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// A configurable test implementation of <see cref="IIdentityVerificationService"/>.
    /// </summary>
    public sealed class MockIdentityVerifier : IIdentityVerificationService
    {
        public UserConsentVerifierAvailability CanVerify { get; set; }

        public bool Verified { get; set; }

        /// <summary>
        /// Asynchronously returns <see cref="CanVerify"/>.
        /// </summary>
        /// <returns></returns>
        public Task<UserConsentVerifierAvailability> CheckVerifierAvailabilityAsync()
        {
            return Task.FromResult(CanVerify);
        }

        /// <summary>
        /// Asynchronously returns <see cref="Verified"/>.
        /// </summary>
        /// <returns></returns>
        public Task<bool> VerifyIdentityAsync()
        {
            return Task.FromResult(Verified);
        }
    }
}
