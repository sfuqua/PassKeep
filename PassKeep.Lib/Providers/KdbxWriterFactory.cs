using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.KeePass.Kdf;
using PassKeep.Lib.KeePass.SecurityTokens;
using SariphLib.Files;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace PassKeep.Lib.Providers
{
    public sealed class KdbxWriterFactory : IKdbxWriterFactory
    {
        /// <summary>
        /// Assembles a writer with the given security tokens.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="keyFile"></param>
        /// <param name="cipher">The algorithm to use for encrypting the database.</param>
        /// <param name="kdfParams">Information about how to transform the user's key.</param>
        /// <returns></returns>
        public IKdbxWriter Assemble(string password, ITestableFile keyFile, EncryptionAlgorithm cipher, KdfParameters kdfParams)
        {
            IList<ISecurityToken> tokens = new List<ISecurityToken>();
            if (!String.IsNullOrEmpty(password))
            {
                tokens.Add(new MasterPassword(password));
            }

            if (keyFile != null)
            {
                tokens.Add(new KeyFile(keyFile));
            }

            return new KdbxWriter(
                tokens,
                cipher,
                RngAlgorithm.Salsa20,
                CompressionAlgorithm.GZip,
                kdfParams
            );
        }
    }
}
