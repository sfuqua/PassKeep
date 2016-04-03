using PassKeep.Lib.Contracts.Services;
using System.Threading.Tasks;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// A configurable test implementation of <see cref="IIdentityVerificationService"/>.
    /// </summary>
    public sealed class MockIdentityVerifier : IIdentityVerificationService
    {
        public bool CanVerify { get; set; }

        public bool Verified { get; set; }

        /// <summary>
        /// Asynchronously returns <see cref="CanVerify"/>.
        /// </summary>
        /// <returns></returns>
        public Task<bool> CanVerifyIdentityAsync()
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
