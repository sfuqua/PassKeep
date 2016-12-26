using PassKeep.Lib.Contracts.Providers;
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

        /// <summary>
        /// Returns the original file.
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public Task<StorageFile> CreateWritableProxyAsync(StorageFile original)
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
    }
}
