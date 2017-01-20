using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.Models;
using PassKeep.Tests.Mocks;
using System.Linq;
using System.Threading.Tasks;
using System;
using Windows.Storage;
using SariphLib.Files;
using PassKeep.Lib.Contracts.Providers;
using System.IO;

namespace PassKeep.Tests
{
    [TestClass]
    public class DashboardViewModelTests : TestClassBase
    {
        private MockStorageItemAccessList accessList;
        private IFileProxyProvider proxyProvider;
        private IDashboardViewModel viewModel;
        private string badFileToken;
        private string proxyFileName;

        public override TestContext TestContext
        {
            get;
            set;
        }

        [TestInitialize]
        public async Task Initialize()
        {
            this.accessList = new MockStorageItemAccessList();
            this.accessList.Add(
                new MockStorageFile { Name = "Some Metadata" },
                "Some Metadata"
            );

            this.accessList.Add(
                new MockStorageFile { Name = "Some more metadata" },
                "Some more metadata"
            );

            badFileToken = this.accessList.Add(
                null,
                "Bad"
            );

            this.accessList.Add(
                new MockStorageFile { Name = "A test file" },
                "A test file"
            );

            this.proxyFileName = "temp.txt";
            StorageFile proxy = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(this.proxyFileName, CreationCollisionOption.OpenIfExists);
            this.accessList.Add(
                proxy.AsWrapper(),
                this.proxyFileName
            );

            this.proxyProvider = new FileProxyProvider(ApplicationData.Current.TemporaryFolder);
            this.viewModel = new DashboardViewModel(
                this.accessList,
                new MockMotdProvider(),
                this.proxyProvider,
                new FileExportService(this.accessList)
            );
        }

        [TestMethod, Timeout(1000)]
        public async Task DashboardViewModelTests_ForgetFiles()
        {
            await this.viewModel.ActivateAsync();

            Assert.IsTrue(
                this.viewModel.StoredFiles.Count > 0,
                "RecentDatabases should not start empty."
            );
            while (this.viewModel.StoredFiles.Count > 0)
            {
                int count = this.viewModel.StoredFiles.Count;

                StoredFileDescriptor selectedFile = this.viewModel.StoredFiles[0];
                
                Assert.IsTrue(
                    selectedFile.ForgetCommand.CanExecute(selectedFile),
                    "ForgetCommand should be executable as long as recent databases remain."
                );

                Assert.IsTrue(
                    this.accessList.ContainsItem(selectedFile.Token),
                    "AccessList should contain the tokens in the ViewMode list."
                );
                await selectedFile.ForgetCommand.ExecuteAsync(selectedFile);

                Assert.AreEqual(
                    count - 1, this.viewModel.StoredFiles.Count,
                    "Forgetting a database should cause the count to decrement."
                );
                Assert.IsFalse(
                    this.accessList.ContainsItem(selectedFile.Token),
                    "Forgetting a database should remove it from the AccessList."
                );
            }
        }

        [TestMethod, Timeout(1000)]
        public async Task DashboardViewModelTests_ForgetProxyWithConsent()
        {
            await this.viewModel.ActivateAsync();

            Assert.IsNotNull(
                await this.proxyProvider.ProxyFolder.GetFileAsync(this.proxyFileName)
            );

            int originalCount = this.viewModel.StoredFiles.Count;

            StoredFileDescriptor proxyDescriptor = this.viewModel.StoredFiles.Last();
            Assert.IsTrue(proxyDescriptor.IsAppOwned);

            await proxyDescriptor.ForgetCommand.ExecuteAsync(proxyDescriptor);
            Assert.AreEqual(originalCount - 1, this.viewModel.StoredFiles.Count);

            // XXX - This shouldn't be needed but it seems GetFileAsync succeeds after a DeleteAsync without a short wait
            await AwaitableTimeout(200);

            try
            {
                StorageFile proxyFile = await this.proxyProvider.ProxyFolder.GetFileAsync(this.proxyFileName);
                Assert.Fail("The file should have been deleted");
            }
            catch (FileNotFoundException)
            {
                // Pass test
            }
        }

        [TestMethod, Timeout(1000)]
        public async Task DashboardViewModelTests_ForgetWithoutConsent()
        {
            await this.viewModel.ActivateAsync();

            int originalCount = this.viewModel.StoredFiles.Count;
            StoredFileDescriptor descriptor = this.viewModel.StoredFiles.First();

            this.viewModel.RequestForgetDescriptor += (s, e) =>
            {
                e.Reject();
            };

            await descriptor.ForgetCommand.ExecuteAsync(descriptor);
            Assert.AreEqual(originalCount, this.viewModel.StoredFiles.Count);
        }

        [TestMethod, Timeout(1000)]
        public async Task DashboardViewModelTests_GetFile()
        {
            await this.viewModel.ActivateAsync();

            StoredFileDescriptor descriptor = this.viewModel.StoredFiles[0];
            IStorageFile file = (await this.viewModel.GetFileAsync(descriptor)).AsIStorageFile;
            Assert.IsNotNull(file, "Fetched file should not be null");
            Assert.AreEqual(descriptor.Metadata, file.Name, "Correct file should be fetched.");
        }

        /// <summary>
        /// Tests that the view model's asynchronous initialization deletes
        /// any files that no longer resolve properly.
        /// </summary>
        /// <returns></returns>
        [TestMethod, Timeout(1000)]
        public async Task DashboardViewModelTests_InitBadFiles()
        {
            Assert.AreEqual(5, this.viewModel.StoredFiles.Count);
            Assert.IsTrue(this.viewModel.StoredFiles.Any(db => db.Token == this.badFileToken));
            await this.viewModel.ActivateAsync();
            Assert.AreEqual(4, this.viewModel.StoredFiles.Count);
            Assert.IsFalse(this.viewModel.StoredFiles.Any(db => db.Token == this.badFileToken));
        }
    }
}
