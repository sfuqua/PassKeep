// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
