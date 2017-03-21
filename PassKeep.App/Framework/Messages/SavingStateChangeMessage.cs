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
