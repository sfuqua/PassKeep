﻿using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Contracts.Models;
using PassKeep.Tests.Mocks;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.ViewModels;
using PassKeep.Models;
using System.Threading.Tasks;
using Windows.Storage;
using System;

namespace PassKeep.Tests
{
    [TestClass]
    public class DashboardViewModelTests : TestClassBase
    {
        private IDatabaseAccessList accessList;
        private IDashboardViewModel viewModel;

        public override TestContext TestContext
        {
            get;
            set;
        }

        [TestInitialize]
        public void Initialize()
        {
            this.accessList = new MockStorageItemAccessList();
            this.accessList.Add(
                new MockStorageFile { Name = "Some Metadata" },
                "Some Metadata"
            );

            this.accessList.Add(
                new MockStorageFile { Name ="Some more metadata" },
                "Some more metadata"
            );

            this.accessList.Add(
                new MockStorageFile { Name = "A test file" },
                "A test file"
            );

            this.viewModel = new DashboardViewModel(this.accessList, new MockMotdProvider(), new MockFileProxyProvider());
        }

        [TestMethod, Timeout(1000)]
        public void DashboardViewModelTests_ForgetFiles()
        {
            Assert.IsTrue(
                this.viewModel.RecentDatabases.Count > 0,
                "RecentDatabases should not start empty."
            );
            while (this.viewModel.RecentDatabases.Count > 0)
            {
                int count = this.viewModel.RecentDatabases.Count;

                StoredFileDescriptor selectedFile = this.viewModel.RecentDatabases[0];
                
                Assert.IsTrue(
                    this.viewModel.ForgetCommand.CanExecute(selectedFile),
                    "ForgetCommand should be executable as long as recent databases remain."
                );

                Assert.IsTrue(
                    this.accessList.ContainsItem(selectedFile.Token),
                    "AccessList should contain the tokens in the ViewMode list."
                );
                this.viewModel.ForgetCommand.Execute(selectedFile);

                Assert.AreEqual(
                    count - 1, this.viewModel.RecentDatabases.Count,
                    "Forgetting a database should cause the count to decrement."
                );
                Assert.IsFalse(
                    this.accessList.ContainsItem(selectedFile.Token),
                    "Forgetting a database should remove it from the AccessList."
                );
            }

            Assert.IsFalse(
                this.viewModel.ForgetCommand.CanExecute(null),
                "Forgetting all databases should make the command non-executable."
            );
        }

        [TestMethod, Timeout(1000)]
        public async Task DashboardViewModelTests_GetFile()
        {
            StoredFileDescriptor descriptor = this.viewModel.RecentDatabases[0];
            IStorageFile file = await this.viewModel.GetFileAsync(descriptor);
            Assert.IsNotNull(file, "Fetched file should not be null");
            Assert.AreEqual(descriptor.Metadata, file.Name, "Correct file should be fetched.");
        }
    }
}
