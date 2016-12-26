using SariphLib.Files;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.Providers
{
    /// <summary>
    /// Interface used to determine whether a given folder contains a path, and if not,
    /// create proxies to the path within the scope of the desired folder.
    /// </summary>
    public interface IFileProxyProvider
    {
        /// <summary>
        /// The location in which proxies are stored.
        /// </summary>
        StorageFolder ProxyFolder { get; }

        /// <summary>
        /// Recursively determines whether the specified storage item exists in the
        /// data folder.
        /// </summary>
        /// <param name="storageItem">The storage item to check.</param>
        /// <returns>Whether the item exists in a folder the app controls.</returns>
        Task<bool> PathIsInScopeAsync(IStorageItem2 storageItem);

        /// <summary>
        /// Returns a writable copy of the specified file in the application's roaming folder.
        /// If the original file already meets these criteria, it is returned as-is.
        /// </summary>
        /// <param name="original">The file to generate a writable, roaming proxy for.</param>
        /// <returns>A copy of <paramref name="original"/> (if necessary) that roams and is writable.</returns>
        Task<ITestableFile> CreateWritableProxyAsync(ITestableFile original);
    }
}
