// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

namespace PassKeep.Framework.Messages
{
    /// <summary>
    /// A message that represents a change in whether a database is
    /// currently being saved.
    /// </summary>
    public sealed class SavingStateChangeMessage : MessageBase
    {
        /// <summary>
        /// Constructs the message.
        /// </summary>
        /// <param name="newState">Whether the database is being saved.</param>
        public SavingStateChangeMessage(bool newState)
        {
            IsNowSaving = newState;
        }

        /// <summary>
        /// The new "IsSaving" state represented by this message.
        /// </summary>
        public bool IsNowSaving
        {
            get;
            private set;
        }
    }
}
