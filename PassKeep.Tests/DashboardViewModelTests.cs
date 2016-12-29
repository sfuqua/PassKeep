using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.Models;
using PassKeep.Tests.Mocks;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Tests
{
    [TestClass]
    public class DashboardViewModelTests : TestClassBase
    {
        private MockStorageItemAccessList accessList;
        private IDashboardViewModel viewModel;
        private string badFileToken;

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

            this.viewModel = new DashboardViewModel(
                this.accessList,
                new MockMotdProvider(),
                new MockFileProxyProvider(),
                new FileExportService(this.accessList)
            );
        }

        [TestMethod, Timeout(1000)]
        public async Task DashboardViewModelTests_ForgetFiles()
        {
            await this.viewModel.ActivateAsync();

            Assert.IsTrue(
                this.viewModel.RecentDatabases.Count > 0,
                "RecentDatabases should not start empty."
            );
            while (this.viewModel.RecentDatabases.Count > 0)
            {
                int count = this.viewModel.RecentDatabases.Count;

                StoredFileDescriptor selectedFile = this.viewModel.RecentDatabases[0];
                
                Assert.IsTrue(
                    selectedFile.ForgetCommand.CanExecute(null),
                    "ForgetCommand should be executable as long as recent databases remain."
                );

                Assert.IsTrue(
                    this.accessList.ContainsItem(selectedFile.Token),
                    "AccessList should contain the tokens in the ViewMode list."
                );
                selectedFile.ForgetCommand.Execute(null);

                Assert.AreEqual(
                    count - 1, this.viewModel.RecentDatabases.Count,
                    "Forgetting a database should cause the count to decrement."
                );
                Assert.IsFalse(
                    this.accessList.ContainsItem(selectedFile.Token),
                    "Forgetting a database should remove it from the AccessList."
                );
            }
        }

        [TestMethod, Timeout(1000)]
        public async Task DashboardViewModelTests_GetFile()
        {
            await this.viewModel.ActivateAsync();

            StoredFileDescriptor descriptor = this.viewModel.RecentDatabases[0];
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
            Assert.AreEqual(4, this.viewModel.RecentDatabases.Count);
            Assert.IsTrue(this.viewModel.RecentDatabases.Any(db => db.Token == this.badFileToken));
            await this.viewModel.ActivateAsync();
            Assert.AreEqual(3, this.viewModel.RecentDatabases.Count);
            Assert.IsFalse(this.viewModel.RecentDatabases.Any(db => db.Token == this.badFileToken));
        }
    }
}
