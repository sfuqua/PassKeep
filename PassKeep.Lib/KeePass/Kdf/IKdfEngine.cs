using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.Kdf
{
    /// <summary>
    /// Encryption algorithm used to transform the user's composite key
    /// into a key for decrypting the database.
    /// </summary>
    public interface IKdfEngine
    {
        /// <summary>
        /// Asynchronously transforms the user's key.
        /// </summary>
        /// <param name="compositeKey">The key to transform.</param>
        /// <param name="token">Token used to cancel the transform task.</param>
        /// <returns>The transformed key.</returns>
        Task<IBuffer> TransformKeyAsync(IBuffer compositeKey, CancellationToken token);
    }
}
