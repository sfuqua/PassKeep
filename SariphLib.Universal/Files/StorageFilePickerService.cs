using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage.Pickers;

namespace SariphLib.Files
{
    /// <summary>
    /// An implementation of <see cref="IFilePickerService"/> that uses Windows StorageFiles.
    /// </summary>
    public sealed class StorageFilePickerService : IFilePickerService
    {
        private readonly ResourceLoader stringLoader;
        private readonly PickerLocationId suggestedStartLocation;

        public StorageFilePickerService(ResourceLoader loader, PickerLocationId suggestedStartLocation)
        {
            this.stringLoader = loader ?? throw new ArgumentNullException(nameof(loader));
            this.suggestedStartLocation = suggestedStartLocation;
        }

        /// <summary>
        /// Picks a writable file for saving.
        /// </summary>
        /// <param name="defaultName">The default file name, optionally including file extension.</param>
        /// <param name="defaultFileExtension">The default file extension.</param>
        /// <param name="promptResourceKey">Localized resource key for the file dialog prompt.</param>
        /// <returns>A task that resolves to the picked file, or null if no file was picked.</returns>
        public async Task<ITestableFile> PickFileForWritingAsync(string defaultName, string defaultFileExtension, string promptResourceKey)
        {
            int extIndex = defaultName.LastIndexOf('.');
            if (extIndex > 0)
            {
                defaultName = defaultName.Substring(0, extIndex);
            }

            FileSavePicker picker = new FileSavePicker
            {
                SuggestedStartLocation = this.suggestedStartLocation,
                SuggestedFileName = defaultName,
                DefaultFileExtension = defaultFileExtension
            };

            picker.FileTypeChoices.Add(
                this.stringLoader.GetString(promptResourceKey),
                new List<string> { defaultFileExtension }
            );

            return new StorageFileWrapper(await picker.PickSaveFileAsync());
        }
    }
}
