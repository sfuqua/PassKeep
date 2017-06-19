using PassKeep.Models;
using PassKeep.Util;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.KeePassLib
{
    /// <summary>
    /// Represents an object that can consume/read KDBX document files.
    /// </summary>
    public interface IKdbxReader
    {
        /// <summary>
        /// Allows read access to the parsed header data of the document, if ReadHeader has been called.
        /// </summary>
        KdbxHeaderData HeaderData
        {
            get;
        }

        /// <summary>
        /// Asynchronously validates the cleartext header of a document.
        /// </summary>
        /// <param name="stream">An IRandomAccessStream contains the data to read.</param>
        /// <param name="token">A token allowing the parse to be cancelled.</param>
        /// <returns>A Task representing the result of the read operation.</returns>
        Task<ReaderResult> ReadHeaderAsync(IRandomAccessStream stream, CancellationToken token);

        /// <summary>
        /// Asynchronously attempts to unlock the document file.
        /// </summary>
        /// <param name="stream">An IRandomAccessStream containing the document to unlock (including the header).</param>
        /// <param name="rawKey">The aggregate raw key to use for decrypting the database.</param>
        /// <param name="token">A token allowing the parse to be cancelled.</param>
        /// <returns>A Task representing the result of the descryiption operation.</returns>
        Task<KdbxDecryptionResult> DecryptFile(IRandomAccessStream stream, IBuffer rawKey, CancellationToken token);

        /// <summary>
        /// Asynchronously attempts to unlock the document file.
        /// </summary>
        /// <param name="stream">An IRandomAccessStream containing the document to unlock (including the header).</param>
        /// <param name="password">The master password used to unlock the document.</param>
        /// <param name="keyfile">A keyfile used to unlock the document.</param>
        /// <param name="token">A token allowing the parse to be cancelled.</param>
        /// <returns>A Task representing the result of the descryiption operation.</returns>
        Task<KdbxDecryptionResult> DecryptFileAsync(IRandomAccessStream stream, string password, ITestableFile keyfile, CancellationToken token);

        /// <summary>
        /// Generates an IKdbxWriter compatible with this IKdbxReader.
        /// </summary>
        /// <returns>An IKdbxWriter that outputs files compatible with this IKdbxReader configuration.</returns>
        IKdbxWriter GetWriter();
    }
}
