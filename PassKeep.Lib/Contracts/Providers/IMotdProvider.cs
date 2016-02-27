namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Provides a message-of-the-day for users.
    /// </summary>
    public interface IMotdProvider
    {
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
    }
}
