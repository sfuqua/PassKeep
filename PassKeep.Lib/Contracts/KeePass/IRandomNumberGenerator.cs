namespace PassKeep.Lib.Contracts.KeePass
{
    public interface IRandomNumberGenerator
    {
        byte[] GetBytes(uint numBytes);
        IRandomNumberGenerator Clone();
        RngAlgorithm Algorithm { get; }
    }
}
