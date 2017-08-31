// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using System;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// An identity verification service that uses Windows Hello for authentication.
    /// </summary>
    public sealed class HelloBasedVerificationService : IIdentityVerificationService
    {
        /// <summary>
        /// The resource key for the prompt to show when validating the user's identity.
        /// </summary>
        public const string PromptResourceKey = "IdentityVerificationPrompt";

        private IResourceProvider resourceProvider;

        /// <summary>
        /// Constructs the service using the specified resource provider.
        /// </summary>
        /// <param name="resourceProvider">The provider to use for localized strings
        /// shown to the user when verifying identity.</param
        public HelloBasedVerificationService(IResourceProvider resourceProvider)
        {
            this.resourceProvider = resourceProvider ?? throw new ArgumentNullException(nameof(resourceProvider));
        }

        /// <summary>
        /// Determines whether the app is currently capable of validating identity.
        /// </summary>
        /// <returns>A task representing whether identity can be verified at this time.</returns>
        public async Task<UserConsentVerifierAvailability> CheckVerifierAvailabilityAsync()
        {
            return await UserConsentVerifier.CheckAvailabilityAsync();
        }

        /// <summary>
        /// Verifies the user's identity.
        /// </summary>
        /// <returns>True if identity was verified, else false.</returns>
        public async Task<bool> VerifyIdentityAsync()
        {
            UserConsentVerificationResult result =
                await UserConsentVerifier.RequestVerificationAsync(
                    this.resourceProvider.GetString(PromptResourceKey)
                );
            return result == UserConsentVerificationResult.Verified;
        }
    }
}
