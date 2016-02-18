using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SariphLib.Files;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.Storage;
using Windows.ApplicationModel;

namespace SariphLib.Tests
{
    [TestClass]
    public class FileHelpers
    {
        public const string ReadOnlyFileName = "ReadOnly.txt";
        public const string WritableFileName = "Writable.txt";

        /// <summary>
        /// Helper to retrieve a test file built into the package.
        /// </summary>
        /// <param name="file">The file name to retrieve.</param>
        /// <returns>The fetched file.</returns>
        public static async Task<StorageFile> GetTestFile(string file)
        {
            StorageFolder installFolder = Package.Current.InstalledLocation;
            return await (await installFolder.GetFolderAsync("TestFiles")).GetFileAsync(file);
        }

        /// <summary>
        /// Helper to copy a file to the temp folder (e.g., for writing).
        /// </summary>
        /// <param name="file">The file to copy.</param>
        /// <returns>A copy of the file.</returns>
        public static async Task<StorageFile> GetTempCopy(StorageFile file)
        {
            return await file.CopyAsync(ApplicationData.Current.TemporaryFolder, file.Name, NameCollisionOption.ReplaceExisting);
        }

        [TestMethod]
        public async Task TestWritableForReadOnly()
        {
            Assert.IsFalse(await (await GetTempCopy(await GetTestFile(ReadOnlyFileName))).CheckWritableAsync());
        }

        [TestMethod]
        public async Task TestWritableTrue()
        {
            Assert.IsTrue(await (await GetTempCopy(await GetTestFile(WritableFileName))).CheckWritableAsync());
        }

        [TestMethod]
        public async Task TestWritableLocked()
        {
            // The test process locks files in the package directory - ReadOnly is false but
            // you cannot open them for writing. 
            Assert.IsFalse(await (await GetTestFile(WritableFileName)).CheckWritableAsync());
        }

        [TestMethod]
        public async Task SetReadOnly()
        {
            StorageFile writableCopy = await GetTempCopy(await GetTestFile(WritableFileName));
            await writableCopy.SetFileAttributesAsync(FileAttributes.ReadOnly);
            Assert.IsFalse(await writableCopy.CheckWritableAsync());
        }

        [TestMethod]
        public async Task ClearReadOnly()
        {
            StorageFile writableCopy = await GetTempCopy(await GetTestFile(ReadOnlyFileName));
            await writableCopy.ClearFileAttributesAsync(FileAttributes.ReadOnly);
            Assert.IsTrue(await writableCopy.CheckWritableAsync(bypassShortcut: true));
        }
    }
}
