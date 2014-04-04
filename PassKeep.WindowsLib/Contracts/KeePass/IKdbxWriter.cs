using PassKeep.Lib.KeePass.Dom;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    public interface IKdbxWriter
    {
        /// <summary>
        /// Writes a database to the specified StorageFile.
        /// </summary>
        /// <param name="file">The StorageFile to write to.</param>
        /// <param name="document">The document to write.</param>
        /// <returns>Whether the write succeeded.</returns>
        Task<bool> Write(StorageFile file, KdbxDocument document);

        /// <summary>
        /// Writes a database to the specified stream.
        /// </summary>
        /// <param name="file">The stream to write to.</param>
        /// <param name="document">The document to write.</param>
        /// <returns>Whether the write succeeded.</returns>
        Task<bool> Write(IOutputStream stream, KdbxDocument document);

        /// <summary>
        /// Attempts to cancel a pending write.
        /// </summary>
        void Cancel();
    }
}
