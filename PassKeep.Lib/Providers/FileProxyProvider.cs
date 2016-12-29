using PassKeep.Lib.Contracts.Providers;
using SariphLib.Files;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using FileNotFoundException = System.IO.FileNotFoundException;

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

        /// <summary>
        /// Returns an enumeration over the existing proxies known by this provider at the time it is
        /// called.
        /// </summary>
        /// <returns>The proxy files that this provider is aware of relative to <see cref="ProxyFolder"/>.</returns>
        public async Task<IEnumerable<ITestableFile>> GetKnownProxiesAsync()
        {
            return (await ProxyFolder.GetFilesAsync().AsTask().ConfigureAwait(false))
                .Select(file => file.AsWrapper());
        }

        /// <summary>
        /// Attempts to delete a proxy with the given file name.
        /// </summary>
        /// <param name="proxyName">The proxy file to delete.</param>
        /// <returns>Whether deletion was successful. False could not be deleted, true if it does't exist.</returns>
        public async Task<bool> TryDeleteProxyAsync(string proxyName)
        {
            Dbg.Trace($"Attempting to delete proxy '{proxyName}'");
            try
            {
                StorageFile file = await ProxyFolder.GetFileAsync(proxyName).AsTask().ConfigureAwait(false);
                await file.DeleteAsync().AsTask().ConfigureAwait(false);
                Dbg.Trace("Proxy deletion successful");
                return true;
            }
            catch (FileNotFoundException)
            {
                Dbg.Trace($"Warning: Returning true from {nameof(TryDeleteProxyAsync)}({proxyName}) due to FileNotFound");
                return true;
            }
            catch (Exception ex)
            {
                Dbg.Trace($"Warning: Returning false from {nameof(TryDeleteProxyAsync)}({proxyName}) due to {ex}");
                return false;
            }
        }

        /// <summary>
        /// Attempts to delete all known proxies.
        /// </summary>
        /// <returns>True if deletion was succesful, false if a proxy could not be deleted for any reaosn.</returns>
        public async Task<bool> TryDeleteAllProxiesAsync()
        {
            Dbg.Trace($"Attepting to delete all known proxies");
            var files = await ProxyFolder.GetFilesAsync().AsTask().ConfigureAwait(false);

            IEnumerable<Task> deleteTasks = files.Select(file => file.DeleteAsync().AsTask());
            Task deletion = Task.WhenAll(deleteTasks);

            try
            {
                await deletion.ConfigureAwait(false);
                Dbg.Trace("Aggregate proxy deletion succeeded");
                return true;
            }
            catch
            {
#if DEBUG
                Dbg.Trace($"Aggregate proxy deletion failed - individual exceptions to follow: {deletion.Exception}");
                foreach (Exception ex in deletion.Exception.InnerExceptions)
                {
                    Dbg.Trace($"{nameof(TryDeleteAllProxiesAsync)} inner exception: {ex}");
                }
#endif
                return false;
            }
        }
    }
}
