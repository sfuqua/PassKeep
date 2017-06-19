namespace PassKeep.KeePassLib.Crypto
{
    public interface IRandomNumberGenerator
    {
        byte[] GetBytes(uint numBytes);
        IRandomNumberGenerator Clone();
        RngAlgorithm Algorithm { get; }
    }
}
