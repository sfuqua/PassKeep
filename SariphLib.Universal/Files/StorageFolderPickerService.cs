// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;

namespace SariphLib.Files
{
    /// <summary>
    /// An implementation of <see cref="IFolderPickerService"/> built on top of Windows APIs.
    /// </summary>
    public class StorageFolderPickerService : IFolderPickerService
    {
        /// <summary>
        /// Asynchronously retrieves a folder, potentially based on a user's input.
        /// </summary>
        /// <returns>A task that resolves to the picked folder.</returns>
        public async Task<IStorageFolder> PickFolderAsync()
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add("*");
            return await picker.PickSingleFolderAsync();
        }

        /// <summary>
        /// Opens the specified folder with the specified file highlighted/selected.
        /// </summary>
        /// <param name="folder">The folder to launch.</param>
        /// <param name="fileToSelect">The file to select in the launched folder.</param>
        /// <returns>A task that resolves when the operation is completed.</returns>
        public async Task LaunchFolderWithSelectionAsync(IStorageFolder folder, ITestableFile fileToSelect)
        {
            FolderLauncherOptions options = new FolderLauncherOptions();
            options.ItemsToSelect.Add(fileToSelect.AsIStorageItem);
            await Launcher.LaunchFolderAsync(folder, options);
        }
    }
}
