// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System.Threading.Tasks;
using Windows.Storage;

namespace SariphLib.Files
{
    /// <summary>
    /// Represents a service that is capable of picking and interacting with folders
    /// on the filesystem.
    /// </summary>
    public interface IFolderPickerService
    {
        /// <summary>
        /// Asynchronously retrieves a folder, potentially based on a user's input.
        /// </summary>
        /// <returns>A task that resolves to the picked folder.</returns>
        Task<IStorageFolder> PickFolderAsync();

        /// <summary>
        /// Opens the specified folder with the specified file highlighted/selected.
        /// </summary>
        /// <param name="folder">The folder to launch.</param>
        /// <param name="fileToSelect">The file to select in the launched folder.</param>
        /// <returns>A task that resolves when the operation is completed.</returns>
        Task LaunchFolderWithSelectionAsync(IStorageFolder folder, ITestableFile fileToSelect);
    }
}
