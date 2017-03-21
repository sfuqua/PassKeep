using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Tests.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Tests
{
    /// <summary>
    /// Tests for KdbxHistory implementation.
    /// </summary>
    [TestClass]
    public class HistoryTests : TestClassBase
    {
        private IRandomNumberGenerator rng;
        private KdbxDocument document;

        public override TestContext TestContext { get; set; }

        /// <summary>
        /// Parses a document for the test.
        /// </summary>
        /// <returns></returns>
        [TestInitialize]
        public async Task Initialize()
        {
            Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(TestContext);
            KdbxReader reader = new KdbxReader();

            using (IRandomAccessStream stream = await databaseInfo.Database.AsIStorageFile.OpenReadAsync())
            {
                Assert.IsFalse((await reader.ReadHeader(stream, CancellationToken.None)).IsError);
                KdbxDecryptionResult decryption = await reader.DecryptFile(stream, databaseInfo.Password, databaseInfo.Keyfile, CancellationToken.None);

                Assert.IsFalse(decryption.Result.IsError);
                this.document = decryption.GetDocument();
                this.rng = reader.HeaderData.GenerateRng();
            }
        }

        [TestMethod, DatabaseInfo("StructureTesting")]
        public void HistoryStartsEmpty()
        {
            IKeePassEntry newEntry = GetNewEntry();
            Assert.IsNotNull(newEntry.History, "New history should not be null");
            Assert.AreEqual(0, newEntry.History.Entries.Count, "New history should be empty");
        }

        [TestMethod, DatabaseInfo("StructureTesting")]
        public void HistoryUpdatesOnEntryUpdate()
        {
            IKeePassEntry newEntry = GetNewEntry();
            newEntry.Title.ClearValue = "Foo";

            IKeePassEntry editedEntry = newEntry.Clone();
            editedEntry.Title.ClearValue = "Bar";

            newEntry.SyncTo(editedEntry);
            Assert.AreEqual(1, newEntry.History.Entries.Count);
            Assert.AreEqual(0, editedEntry.History.Entries.Count);

            Assert.AreEqual("Bar", newEntry.Title.ClearValue);
            Assert.AreEqual("Foo", newEntry.History.Entries[0].Title.ClearValue);
        }

        private IKeePassEntry GetNewEntry()
        {
            return new KdbxEntry(this.document.Root.DatabaseGroup, this.rng, this.document.Metadata);
        }
    }
}
