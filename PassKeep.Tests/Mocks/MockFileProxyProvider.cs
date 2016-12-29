using PassKeep.Lib.Contracts.Providers;
using SariphLib.Files;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of <see cref="IFileProxyProvider"/> that returns a configurable result.
    /// </summary>
    public class MockFileProxyProvider : IFileProxyProvider
    {
        /// <summary>
        /// Value to return from <see cref="PathIsInScopeAsync(IStorageItem2)"/>.
        /// </summary>
        public bool ScopeValue
        {
            get;
            set;
        }

        public StorageFolder ProxyFolder
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the original file.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public Task<ITestableFile> CreateWritableProxyAsync(ITestableFile original)
        {
            return Task.FromResult(original);
        }

        /// <summary>
        /// Returns <see cref="ScopeValue"/>.
        /// </summary>
        /// <param name="storageItem"></param>
        /// <returns></returns>
        public Task<bool> PathIsInScopeAsync(IStorageItem2 storageItem)
        {
            return Task.FromResult(ScopeValue);
        }

        /// <summary>
        /// Returns an empty enumerable.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<ITestableFile>> GetKnownProxiesAsync()
        {
            return Task.FromResult(Enumerable.Empty<ITestableFile>());
        }

        /// <summary>
        /// Returns true.
        /// </summary>
        /// <param name="proxyName"></param>
        /// <returns></returns>
        public Task<bool> TryDeleteProxyAsync(string proxyName)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Returns true.
        /// </summary>
        /// <returns></returns>
        public Task<bool> TryDeleteAllProxiesAsync()
        {
            return Task.FromResult(true);
        }
    }
}
