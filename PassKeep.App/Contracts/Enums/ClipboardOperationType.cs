namespace PassKeep.Lib.Contracts.Enums
{
    /// <summary>
    /// Represents the type of data being interfaced with on the clipboard.
    /// </summary>
    public enum ClipboardOperationType
    {
        /// <summary>
        /// No operation.
        /// </summary>
        None,

        /// <summary>
        /// A username clipboard operation.
        /// </summary>
        UserName,

        /// <summary>
        /// A password clipboard operation.
        /// </summary>
        Password,

        /// <summary>
        /// An url clipboard operation.
        /// </summary>
        Url
    }
}
