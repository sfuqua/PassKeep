using PassKeep.Lib.Contracts.Providers;
using SariphLib.Files;
using SariphLib.Infrastructure;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Lib.Providers
{
    /// <summary>
    /// Implementation of <see cref="IFileProxyProvider"/> that lets the application
    /// configure where it would like to keep proxies.
    /// </summary>
    public class FileProxyProvider : IFileProxyProvider
    {
        private readonly StorageFolder rootFolder;

        /// <summary>
        /// Initializes the provider with the specified root.
        /// </summary>
        /// <param name="rootFolder">The folder to use as a parent for proxies.</param>
        public FileProxyProvider(StorageFolder rootFolder)
        {
            if (rootFolder == null)
            {
                throw new ArgumentNullException(nameof(rootFolder));
            }

            this.rootFolder = rootFolder;
        }

        /// <summary>
        /// The folder used to house proxies.
        /// </summary>
        public StorageFolder ProxyFolder
        {
            get { return this.rootFolder; }
        }

        /// <summary>
        /// Recursively determines whether the specified storage item exists in the
        /// data folder.
        /// </summary>
        /// <param name="storageItem">The storage item to check.</param>
        /// <returns>Whether the item exists in a folder the app controls.</returns>
        public async Task<bool> PathIsInScopeAsync(IStorageItem2 storageItem)
        {
            if (storageItem == null)
            {
                return false;
            }

            if (ProxyFolder.IsEqual(storageItem))
            {
                return true;
            }

            return await PathIsInScopeAsync(
                await storageItem.GetParentAsync().AsTask().ConfigureAwait(false)
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns a writable copy of the specified file in the root folder.
        /// If the original file already meets these criteria, it is returned as-is.
        /// </summary>
        /// <param name="original">The file to generate a writable proxy for in the expected location.</param>
        /// <returns>A copy of <paramref name="original"/> (if necessary) that is in the right spot and is writable.</returns>
        public async Task<ITestableFile> CreateWritableProxyAsync(ITestableFile original)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            string originalPath = original.AsIStorageItem.Path;
            if (await PathIsInScopeAsync(original.AsIStorageItem2).ConfigureAwait(false))
            {
                if (await original.AsIStorageFile.CheckWritableAsync().ConfigureAwait(false))
                {
                    Dbg.Trace($"Existing file {originalPath} does not need to be proxied");
                    return original;
                }
                else
                {
                    Dbg.Trace($"Existing file {originalPath} could not be used as a proxy because it's not writable");
                }
            }
            else
            {
                Dbg.Trace($"Existing file {originalPath} could not be used as a proxy because it's in the wrong path");
            }

            StorageFile proxy = await original.AsIStorageFile.CopyAsync(ProxyFolder, original.AsIStorageItem.Name, NameCollisionOption.GenerateUniqueName)
                .AsTask().ConfigureAwait(false);
            await proxy.ClearFileAttributesAsync(FileAttributes.ReadOnly).ConfigureAwait(false);

            Dbg.Trace($"Existing file {originalPath} proxied as {proxy.Path}");

            return proxy.AsWrapper();
        }
    }
}
