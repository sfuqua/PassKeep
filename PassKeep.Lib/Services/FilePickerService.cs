﻿using PassKeep.Lib.Contracts.Services;
using SariphLib.Files;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// A service that allows the user to pick files using Windows file pickers.
    /// </summary>
    public class FilePickerService : IFileAccessService
    {
        private readonly string extension;
        private readonly string fileTypeDescription;

        /// <summary>
        /// Initializes the service with information about the file type being picked.
        /// </summary>
        /// <param name="extension">The file extension supported - dot is optional.</param>
        /// <param name="fileTypeDescription">A user-friendly description of the file type.</param>
        public FilePickerService(string extension, string fileTypeDescription)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (string.IsNullOrEmpty(fileTypeDescription))
            {
                throw new ArgumentNullException(nameof(fileTypeDescription));
            }

            // Fix the extension format if needed
            if (extension[0] != '.')
            {
                extension = $".{extension}";
            }

            this.extension = extension;
            this.fileTypeDescription = fileTypeDescription;
        }

        /// <summary>
        /// Allows the user to pick a file for the purpose of opening it, using a <see cref="FileOpenPicker"/>.
        /// </summary>
        /// <returns>A task that resolves to the picked file.</returns>
        public async Task<ITestableFile> PickFileForOpenAsync()
        {
            FileOpenPicker picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            // Not all databases end in .kdbx
            picker.FileTypeFilter.Add("*");

            return (await picker.PickSingleFileAsync()).AsWrapper();
        }

        /// <summary>
        /// Allows the user to pick a file for the purposes of saving to it, using a <see cref="FileSavePicker"/>.
        /// </summary>
        /// <param name="defaultName">The default filename to use (with optional extension).</param>
        /// <returns>A task that resolves to the picked file.</returns>
        public async Task<ITestableFile> PickFileForSaveAsync(string defaultName)
        {
            int extIndex = defaultName.LastIndexOf('.');
            if (extIndex > 0)
            {
                defaultName = defaultName.Substring(0, extIndex);
            }

            FileSavePicker picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = defaultName,
                DefaultFileExtension = this.extension
            };

            picker.FileTypeChoices.Add(
                this.fileTypeDescription,
                new List<string> { this.extension }
            );

            return (await picker.PickSaveFileAsync()).AsWrapper();
        }
    }
}