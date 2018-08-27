using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Lib.Providers;
using SariphLib.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Tests
{
    /// <summary>
    /// Tests for <see cref="FileProxyProvider"/> against real files.
    /// </summary>
    [TestClass]
    public class FileProxyDepthTests
    {
        private FileProxyProvider provider;
        private StorageFolder rootFolder;

        [TestInitialize]
        public async Task Init()
        {
            this.rootFolder = ApplicationData.Current.TemporaryFolder;
            this.rootFolder = await this.rootFolder.CreateFolderAsync("Proxies", CreationCollisionOption.OpenIfExists);

            this.provider = new FileProxyProvider(this.rootFolder);
        }

        [TestMethod, Timeout(2000)]
        public async Task TestProxy()
        {
            ITestableFile existingFile = await Utils.GetDatabaseByName("StructureTesting.kdbx");
            Assert.IsFalse(await this.provider.PathIsInScopeAsync(existingFile.AsIStorageItem2), "Initial file should be out of scope");

            ITestableFile proxy = await this.provider.CreateWritableProxyAsync(existingFile);
            Assert.AreNotSame(existingFile, proxy, "Proxy and original file should not be the same object");
            Assert.AreNotEqual(existingFile.AsIStorageFile, proxy.AsIStorageFile, "Proxy and original file should not be equal");

            Assert.IsTrue(await this.provider.PathIsInScopeAsync(proxy.AsIStorageItem2), "Proxy should be in the right place");
            Assert.IsTrue(await proxy.CheckWritableAsync(), "Proxy should be writable");
        }

        [TestMethod, Timeout(2000)]
        public async Task TestProxyInScope()
        {
            StorageFile existingFile = await this.rootFolder.CreateFileAsync("tmp.txt", CreationCollisionOption.ReplaceExisting);
            ITestableFile testFile = existingFile.AsWrapper();

            Assert.IsTrue(await this.provider.PathIsInScopeAsync(existingFile));
            ITestableFile proxy = await this.provider.CreateWritableProxyAsync(testFile);

            Assert.AreSame(testFile, proxy);
        }
    }
}
