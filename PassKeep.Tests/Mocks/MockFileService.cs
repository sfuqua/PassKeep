using PassKeep.Lib.Contracts.Services;
using SariphLib.Files;
using System.Threading.Tasks;

namespace PassKeep.Tests.Mocks
{
    /// <summary>
    /// A dummy service that never returns files.
    /// </summary>
    public class MockFileService : IFileAccessService
    {
        /// <summary>
        /// Returns null.
        /// </summary>
        /// <returns></returns>
        public Task<ITestableFile> PickFileForOpenAsync()
        {
            return Task.FromResult<ITestableFile>(null);
        }

        /// <summary>
        /// Returns null.
        /// </summary>
        /// <param name="defaultName"></param>
        /// <returns></returns>
        public Task<ITestableFile> PickFileForSaveAsync(string defaultName)
        {
            return Task.FromResult<ITestableFile>(null);
        }
    }
}
