// Copyright 2017 Steven Fuqua
// This file is part of the SariphLib library and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Files;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SariphLib.Testing
{
    /// <summary>
    /// A testable folder picker service that allows the test framework to configure returned results.
    /// </summary>
    public class MockFolderPickerService : IFolderPickerService
    {
        /// <summary>
        /// Fired when <see cref="LaunchFolderWithSelectionAsync(IStorageFolder, ITestableFile)"/> does its thing.
        /// </summary>
        public event EventHandler FolderLaunched;

        /// <summary>
        /// Initializes <see cref="FileToPick"/> with the provided value.
        /// </summary>
        /// <param name="pickedFolder"></param>
        public MockFolderPickerService(IStorageFolder pickedFolder)
        {
            FileToPick = pickedFolder;
        }

        /// <summary>
        /// The folder to return from <see cref="PickFolderAsync"/>.
        /// </summary>
        public IStorageFolder FileToPick { get; set; }

        /// <summary>
        /// No-op. Returns a completed task.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileToSelect"></param>
        /// <returns></returns>
        public Task LaunchFolderWithSelectionAsync(IStorageFolder folder, ITestableFile fileToSelect)
        {
            FolderLaunched?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a task that resolves to <see cref="FileToPick"/>.
        /// </summary>
        /// <returns></returns>
        public Task<IStorageFolder> PickFolderAsync()
        {
            return Task.FromResult(FileToPick);
        }
    }
}
