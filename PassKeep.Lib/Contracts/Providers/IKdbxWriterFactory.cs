using PassKeep.Lib.Contracts.KeePass;
using SariphLib.Files;
using Windows.Storage;

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
        /// <param name="transformRounds">The number of times to encrypt the security tokens.</param>
        /// <returns></returns>
        IKdbxWriter Assemble(string password, ITestableFile keyFile, ulong transformRounds);
    }
}
