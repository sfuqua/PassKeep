namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Stream cipher to use for generating random numbers.
    /// </summary>
    public enum RngAlgorithm : int
    {
        ArcFourVariant = 1,
        Salsa20 = 2,
        ChaCha20 = 3
    }
}