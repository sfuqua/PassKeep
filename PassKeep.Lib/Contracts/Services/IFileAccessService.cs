using SariphLib.Files;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// A service that allows the user to pick files.
    /// </summary>
    public interface IFileAccessService
    {
        /// <summary>
        /// Allows the user to pick a file for the purpose of opening it.
        /// </summary>
        /// <returns>A task that resolves to the picked file.</returns>
        Task<ITestableFile> PickFileForOpenAsync();

        /// <summary>
        /// Allows the user to pick a file for the purposes of saving to it.
        /// </summary>
        /// <param name="defaultName">The default filename to use (with optional extension).</param>
        /// <returns>A task that resolves to the picked file.</returns>
        Task<ITestableFile> PickFileForSaveAsync(string defaultName);
    }
}
