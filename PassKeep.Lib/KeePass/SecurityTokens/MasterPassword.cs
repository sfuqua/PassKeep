// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using SariphLib.Diagnostics;
using System;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.SecurityTokens
{
    /// <summary>
    /// A security token represented by a string password
    /// </summary>
    public class MasterPassword : ISecurityToken
    {
        private string _password;

        /// <summary>
        /// Constructs an instance from the provided password
        /// </summary>
        /// <param name="password"></param>
        public MasterPassword(string password)
        {
            DebugHelper.Assert(!String.IsNullOrEmpty(password));
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("password cannot be null or empty", "password");
            }

            _password = password;
        }

        /// <summary>
        /// Asynchonrously computes the security token data represented by the password
        /// </summary>
        /// <returns>The SHA256 hash of the plaintext password</returns>
        public Task<IBuffer> GetBufferAsync()
        {
            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var hash = sha256.CreateHash();

            IBuffer passwordData = CryptographicBuffer.ConvertStringToBinary(_password, BinaryStringEncoding.Utf8);
            hash.Append(passwordData);
            return Task.FromResult(hash.GetValueAndReset());
        }
    }
}
