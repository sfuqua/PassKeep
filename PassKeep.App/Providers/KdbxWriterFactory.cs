using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.KeePass.SecurityTokens;
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
        /// <returns></returns>
        public IKdbxWriter Assemble(string password, StorageFile keyFile)
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
                RngAlgorithm.Salsa20,
                CompressionAlgorithm.GZip,
                transformRounds: 6000
            );
        }
    }
}
