using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Providers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Tests
{
    [TestClass]
    public class CachedFilesViewModelTests
    {
        private IFileProxyProvider proxyProvider;
        private ICachedFilesViewModelFactory viewModelFactory;

        /// <summary>
        /// Inits the proxy provider and ViewModelFactory.
        /// </summary>
        [TestInitialize]
        public async Task Init()
        {
            StorageFolder rootFolder = ApplicationData.Current.TemporaryFolder;
            rootFolder = await rootFolder.CreateFolderAsync("Proxies", CreationCollisionOption.OpenIfExists);

            this.proxyProvider = new FileProxyProvider(rootFolder);
            Assert.AreEqual(0, (await rootFolder.GetFilesAsync()).Count, "Test should start with no proxies");

            this.viewModelFactory = new CachedFilesViewModelFactory(this.proxyProvider);
        }

        /// <summary>
        /// Ensures no files remain after test execution.
        /// </summary>
        [TestCleanup]
        public async Task Cleanup()
        {
            await (await this.viewModelFactory.AssembleAsync()).DeleteAllAsyncCommand.ExecuteAsync(null);
            Assert.AreEqual(0, (await this.proxyProvider.ProxyFolder.GetFilesAsync()).Count, "Test should end with no proxies");
        }

        /// <summary>
        /// Tests that the ViewModel contains the right number of entries on creation.
        /// </summary>
        /// <returns></returns>
        [TestMethod, Timeout(1000)]
        public async Task CachedFilesViewModelTests_FileCount()
        {
            int nFiles = 3;
            for (int i = 0; i < nFiles; i++)
            {
                await CreateRandomFile();
            }

            ICachedFilesViewModel vm = await this.viewModelFactory.AssembleAsync();
            Assert.AreEqual(nFiles, vm.CachedFiles.Count);
        }

        /// <summary>
        /// Tests that the ViewModel contains the entries after
        /// deleting an entry.
        /// </summary>
        /// <returns></returns>
        [TestMethod, Timeout(1000)]
        public async Task CachedFilesViewModelTests_DeleteOne()
        {
            int nFiles = 3;
            string[] fileNames = new string[nFiles];

            for (int i = 0; i < nFiles; i++)
            {
                fileNames[i] = await CreateRandomFile();
            }

            ICachedFilesViewModel vm = await this.viewModelFactory.AssembleAsync();
            Assert.AreEqual(nFiles, vm.CachedFiles.Count);
            for (int i = 0; i < nFiles; i++)
            {
                // Order is not guaranteed to be the same
                Assert.IsTrue(fileNames.Contains(vm.CachedFiles[i].Metadata));
            }

            int iDelete = 1;
            string deletedName = vm.CachedFiles[iDelete].Metadata;
            await vm.DeleteFileAsyncCommand.ExecuteAsync(vm.CachedFiles[iDelete]);

            Assert.AreEqual(nFiles - 1, vm.CachedFiles.Count, "CachedFiles.Count should decrement on delete");
            Assert.IsFalse(vm.CachedFiles.Select(desc => desc.Metadata).Contains(deletedName), "The right entry should have been deleted");

            ICachedFilesViewModel newVm = await this.viewModelFactory.AssembleAsync();
            Assert.AreEqual(nFiles - 1, newVm.CachedFiles.Count, "Assembling a new ViewModel should reflect the deleted entry");
        }

        /// <summary>
        /// Helper that adds a file to the proxy folder.
        /// </summary>
        /// <returns>A task representing the name of the new file.</returns>
        private async Task<string> CreateRandomFile()
        {
            string fileName = $"{Guid.NewGuid()}.txt";
            StorageFile newFile = await this.proxyProvider.ProxyFolder.CreateFileAsync(fileName);
            return newFile.Name;
        }
    }
}
