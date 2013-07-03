using PassKeep.Lib.Contracts.Providers;
using Windows.Security.Cryptography;

namespace PassKeep.Lib.Providers
{
    public class CryptographicBufferRngProvider : ICryptoRngProvider
    {
        public int GetInt(int exclusiveCeiling)
        {
            return (int)(CryptographicBuffer.GenerateRandomNumber() % exclusiveCeiling);
        }
    }
}
