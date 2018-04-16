namespace PassKeep.Lib.Contracts.Enums
{
    /// <summary>
    /// Status codes returned from an attempt to update database credentials.
    /// </summary>
    public enum CredentialUpdateResult
    {
        /// <summary>
        /// Credentials were updated successfully.
        /// </summary>
        Success,

        /// <summary>
        /// There was a problem accessing the credential vault.
        /// </summary>
        CredentialVaultError
    }
}
