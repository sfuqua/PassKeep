namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Represents an interface to a service capable of generating integers in a cryptographically
    /// secure fashion.
    /// </summary>
    public interface ICryptoRngProvider
    {
        /// <summary>
        /// Generates a random integer x, such that x &lt;= 0 &lt;= <paramref name="exclusiveCeiling"/>.
        /// </summary>
        /// <param name="exclusiveCeiling"></param>
        /// <returns></returns>
        int GetInt(int exclusiveCeiling);
    }
}
