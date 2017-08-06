// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Files;
using System.Collections.Generic;
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

        /// <summary>
        /// Returns an enumeration over the existing proxies known by this provider at the time it is
        /// called.
        /// </summary>
        /// <returns>The proxy files that this provider is aware of relative to <see cref="ProxyFolder"/>.</returns>
        Task<IEnumerable<ITestableFile>> GetKnownProxiesAsync();

        /// <summary>
        /// Attempts to delete a proxy with the given file name.
        /// </summary>
        /// <param name="proxyName">The proxy file to delete.</param>
        /// <returns>Whether deletion was successful. False could not be deleted, true if it does't exist.</returns>
        Task<bool> TryDeleteProxyAsync(string proxyName);

        /// <summary>
        /// Attempts to delete all known proxies.
        /// </summary>
        /// <returns>True if deletion was succesful, false if a proxy could not be deleted for any reaosn.</returns>
        Task<bool> TryDeleteAllProxiesAsync();
    }
}
