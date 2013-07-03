namespace PassKeep.Lib.Contracts.Providers
{
    public interface ICryptoRngProvider
    {
        /// <summary>
        /// Generates a random integer x, such that x &lt;= 0 &lt;= exclusiveCeiling.
        /// </summary>
        /// <param name="exclusiveCeiling"></param>
        /// <returns></returns>
        int GetInt(int exclusiveCeiling);
    }
}
