using System.Threading.Tasks;

namespace SariphLib.Files
{
    /// <summary>
    /// A service that allows choosing files for various operations.
    /// </summary>
    public interface IFilePickerService
    {
        /// <summary>
        /// Picks a writable file for saving.
        /// </summary>
        /// <param name="defaultName">The default file name, optionally including file extension.</param>
        /// <param name="defaultFileExtension">The default file extension.</param>
        /// <param name="promptResourceKey">Localized resource key for the file dialog prompt.</param>
        /// <returns>A task that resolves to the picked file.</returns>
        Task<ITestableFile> PickFileForWritingAsync(string defaultName, string defaultFileExtension, string promptResourceKey);
    }
}
