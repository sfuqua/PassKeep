using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    /// <summary>
    /// Represents an object that can consume/read KDBX database files.
    /// </summary>
    public interface IKdbxReader
    {
        /// <summary>
        /// Allows read access to the parsed header data of the database, if ReadHeader has been called.
        /// </summary>
        KdbxHeaderData HeaderData
        {
            get;
        }

        /// <summary>
        /// Asynchronously validates the cleartext header of a database.
        /// </summary>
        /// <param name="stream">An IRandomAccessStream contains the data to read.</param>
        /// <returns>A Task representing the result of the read operation.</returns>
        Task<ReaderResult> ReadHeader(IRandomAccessStream stream);

        /// <summary>
        /// Asynchronously attempts to unlock the database file.
        /// </summary>
        /// <param name="stream">An IRandomAccessStream containing the database to unlock (including the header).</param>
        /// <param name="password">The master password used to unlock the database.</param>
        /// <param name="keyfile">A keyfile used to unlock the database.</param>
        /// <returns>A Task representing the result of the descryiption operation.</returns>
        Task<KdbxDecryptionResult> DecryptFile(IRandomAccessStream stream, string password, StorageFile keyfile);
        
        /// <summary>
        /// Cancels any pending read/unlock operations.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Generates an IKdbxWriter compatible with this IKdbxReader.
        /// </summary>
        /// <returns>An IKdbxWriter that outputs files compatible with this IKdbxReader configuration.</returns>
        IKdbxWriter GetWriter();
    }
}
