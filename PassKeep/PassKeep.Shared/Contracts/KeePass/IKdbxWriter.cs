using PassKeep.Lib.KeePass.Dom;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    public interface IKdbxWriter
    {
        /// <summary>
        /// Writes a document to the specified StorageFile.
        /// </summary>
        /// <param name="file">The StorageFile to write to.</param>
        /// <param name="document">The document to write.</param>
        /// <param name="token">A token allowing the operation to be cancelled.</param>
        /// <returns>Whether the write succeeded.</returns>
        Task<bool> Write(StorageFile file, KdbxDocument document, CancellationToken token);

        /// <summary>
        /// Writes a document to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="document">The document to write.</param>
        /// <param name="token">A token allowing the operation to be cancelled.</param>
        /// <returns>Whether the write succeeded.</returns>
        Task<bool> Write(IOutputStream stream, KdbxDocument document, CancellationToken token);
    }
}
