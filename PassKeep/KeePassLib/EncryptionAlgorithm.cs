namespace PassKeep.KeePassLib
{
    /// <summary>
    /// The algorithm used to encrypt/decrypt the database XML
    /// with the transformed composite key.
    /// </summary>
    public enum EncryptionAlgorithm
    {
        Aes,
        ChaCha20
    }
}
