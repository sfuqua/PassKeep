// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Kdf;
using SariphLib.Files;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Generates writers capable of persisting a database.
    /// </summary>
    public interface IKdbxWriterFactory
    {
        /// <summary>
        /// Assembles a writer with the given security tokens.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="keyFile"></param>
        /// <param name="cipher">The algorithm to use for encrypting the database.</param>
        /// <param name="kdfParams">Information about how to transform the user's key.</param>
        /// <returns></returns>
        IKdbxWriter Assemble(string password, ITestableFile keyFile, EncryptionAlgorithm cipher, KdfParameters kdfParams);
    }
}
