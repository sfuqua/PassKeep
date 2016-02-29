namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Provides a message-of-the-day for users.
    /// </summary>
    public interface IMotdProvider
    {
        /// <summary>
        /// Evaluates whether the message-of-the-day should be displayed.
        /// </summary>
        bool ShouldDisplay { get; }

        /// <summary>
        /// Provides the title of the message-of-the-day.
        /// </summary>
        /// <returns>A title.</returns>
        string GetTitle();

        /// <summary>
        /// Provides a body for the message-of-the-day.
        /// </summary>
        /// <returns>The body.</returns>
        string GetBody();

        /// <summary>
        /// Provides a description of an action that will dismiss the message-of-the-day.
        /// </summary>
        /// <returns>Dismissal text.</returns>
        string GetDismiss();

        /// <summary>
        /// Flags this MOTD as "displayed" so it will not display again in the
        /// current or future sessions.
        /// </summary>
        void MarkAsDisplayed();
    }
}
