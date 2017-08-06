// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
