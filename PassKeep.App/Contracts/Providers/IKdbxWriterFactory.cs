using PassKeep.Lib.Contracts.KeePass;
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
        /// <returns></returns>
        IKdbxWriter Assemble(string password, StorageFile keyFile);
    }
}
