namespace PassKeep.Lib.Contracts.Enums
{
    /// <summary>
    /// Classification of various types of async operations
    /// that might need to block the UI.
    /// </summary>
    public enum AsyncOperationType
    {
        /// <summary>
        /// An unspecified operation that blocks the UI.
        /// Usually represents a bug.
        /// </summary>
        Unspecified,

        /// <summary>
        /// The user's identity if being verified (e.g., via Windows Hello)
        /// and the UI needs to be stalled until completed.
        /// </summary>
        IdentityVerification,

        /// <summary>
        /// The user's database is being decrypted.
        /// </summary>
        DatabaseDecryption,

        /// <summary>
        /// The user's database is being encrypted.
        /// </summary>
        DatabaseEncryption,

        /// <summary>
        /// The user's decrypted database XML is being loaded.
        /// </summary>
        DatabaseLoad,

        /// <summary>
        /// The user's credential vault is being accessed asynchronously.
        /// </summary>
        CredentialVaultAccess
    }
}
