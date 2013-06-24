using System;
using System.Security.Cryptography;
using PassKeep.Lib.Contracts.Providers;

namespace PassKeep.Lib.Providers
{
    public class NetCryptoRngProvider : ICryptoRngProvider
    {
        private RandomNumberGenerator rng;

        public NetCryptoRngProvider()
        {
            rng = new RNGCryptoServiceProvider();
        }

        private int getInt()
        {
            byte[] buffer = new byte[sizeof(int)];
            rng.GetBytes(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }

        public int GetInt(int exclusiveCeiling)
        {
            return getInt() % exclusiveCeiling;
        }
    }
}
