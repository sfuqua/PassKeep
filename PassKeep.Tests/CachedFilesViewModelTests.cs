using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Tests.Mocks;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Tests
{
    [TestClass]
    public class CachedFilesViewModelTests : TestClassBase
    {
        private IFileProxyProvider proxyProvider;
        private ICachedFilesViewModelFactory viewModelFactory;
        private MockUserPromptingService promptService;

        public override TestContext TestContext
        {
            get;
            set;
        }

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

            IDatabaseAccessList accessList = new MockStorageItemAccessList();
            this.promptService = new MockUserPromptingService();
            IFileAccessService fileService = new MockFileService();
            this.viewModelFactory = new CachedFilesViewModelFactory(
                accessList,
                new FileExportService(accessList, fileService),
                this.proxyProvider,
                this.promptService,
                this.promptService,
                fileService
            );
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
            Assert.AreEqual(nFiles, vm.StoredFiles.Count);
        }

        /// <summary>
        /// Tests that the ViewModel contains the entries after
        /// deleting an entry.
        /// </summary>
        /// <returns></returns>
        [TestMethod, Timeout(30000)]
        public async Task CachedFilesViewModelTests_DeleteOne()
        {
            int nFiles = 3;
            string[] fileNames = new string[nFiles];

            for (int i = 0; i < nFiles; i++)
            {
                fileNames[i] = await CreateRandomFile();
            }

            ICachedFilesViewModel vm = await this.viewModelFactory.AssembleAsync();
            Assert.AreEqual(nFiles, vm.StoredFiles.Count);
            for (int i = 0; i < nFiles; i++)
            {
                // Order is not guaranteed to be the same
                Assert.IsTrue(fileNames.Contains(vm.StoredFiles[i].Metadata));
            }

            this.promptService.Result = true;

            int iDelete = 1;
            string deletedName = vm.StoredFiles[iDelete].Metadata;
            await vm.StoredFiles[iDelete].ForgetCommand.ExecuteAsync(null);

            Assert.AreEqual(nFiles - 1, vm.StoredFiles.Count, "CachedFiles.Count should decrement on delete");
            Assert.IsFalse(vm.StoredFiles.Select(desc => desc.Metadata).Contains(deletedName), "The right entry should have been deleted");

            await AwaitableTimeout(200);
            ICachedFilesViewModel newVm = await this.viewModelFactory.AssembleAsync();
            Assert.AreEqual(nFiles - 1, newVm.StoredFiles.Count, "Assembling a new ViewModel should reflect the deleted entry");
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
