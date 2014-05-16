namespace PassKeep.Lib.Contracts.Enums
{
    /// <summary>
    /// Represents the type of a clipboard clear timer.
    /// </summary>
    public enum ClipboardTimerType
    {
        /// <summary>
        /// No timer active.
        /// </summary>
        None,

        /// <summary>
        /// Clearing the clipboard for a username copy.
        /// </summary>
        UserName,

        /// <summary>
        /// Clearing the clipboard for a password copy.
        /// </summary>
        Password
    }
}
