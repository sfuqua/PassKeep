// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
