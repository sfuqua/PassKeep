using System;
using Windows.Security.Credentials.UI;

namespace PassKeep.Lib.EventArgClasses
{
    /// <summary>
    /// An event that conveys verifier availability to the view.
    /// </summary>
    public class IdentityVerifierAvailabilityEventArgs : EventArgs
    {
        private readonly UserConsentVerifierAvailability availability;

        public IdentityVerifierAvailabilityEventArgs(UserConsentVerifierAvailability availability)
        {
            this.availability = availability;
        }

        /// <summary>
        /// The verifiy availability represented by this event.
        /// </summary>
        public UserConsentVerifierAvailability Availability => this.availability;
    }
}
