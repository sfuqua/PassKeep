// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
