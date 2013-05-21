using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace PassKeep.KeePassLib.SecurityTokens
{
    public class MasterPassword : ISecurityToken
    {
        private string _password;
        public MasterPassword(string password)
        {
            Debug.Assert(!string.IsNullOrEmpty(password));
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("password cannot be null or empty", "password");
            }

            _password = password;
        }

        public Task<IBuffer> GetBuffer()
        {
            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var hash = sha256.CreateHash();

            IBuffer passwordData = CryptographicBuffer.ConvertStringToBinary(_password, BinaryStringEncoding.Utf8);
            hash.Append(passwordData);
            return Task.FromResult(hash.GetValueAndReset());
        }
    }
}
