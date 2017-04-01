using PassKeep.Lib.KeePass.Dom;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Contracts.KeePass
{
    public interface IKdbxWriter
    {
        KdbxHeaderData HeaderData { get; }

        /// <summary>
        /// Writes a document to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="document">The document to write.</param>
        /// <param name="token">A token allowing the operation to be cancelled.</param>
        /// <returns>Whether the write succeeded.</returns>
        Task<bool> WriteAsync(IOutputStream stream, KdbxDocument document, CancellationToken token);
    }
}
