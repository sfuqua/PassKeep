using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.KeePassTests
{
    public partial class KdbxParseTests
    {
        private async Task roundTrip()
        {
            ReaderResult initialHeaderResult = await reader.ReadHeader(await this.thisTestInfo.Database.OpenReadAsync());
            Assert.AreEqual(ReaderResult.Success, initialHeaderResult, "Initial header read should be successful");

            KdbxDecryptionResult result = await reader.DecryptFile(await this.thisTestInfo.Database.OpenReadAsync(), this.thisTestInfo.Password, this.thisTestInfo.Keyfile);

            Assert.AreEqual(ReaderResult.Success, result.Result, "File should have initially decrypted properly");
            KdbxDocument kdbxDoc = new KdbxDocument(result.GetXmlDocument().Root, reader.HeaderData.GenerateRng());
            IKdbxWriter writer = reader.GetWriter();
            using (var stream = new InMemoryRandomAccessStream())
            {
                bool writeResult = await writer.Write(stream, kdbxDoc);
                Assert.IsTrue(writeResult, "File should have written successfully");

                stream.Seek(0);
                KdbxReader newReader = new KdbxReader();
                var result2 = await newReader.ReadHeader(stream);

                Assert.AreEqual(ReaderResult.Success, result2, "Header should been read back successfully after write");

                var result3 = await newReader.DecryptFile(stream, this.thisTestInfo.Password, this.thisTestInfo.Keyfile);

                Assert.AreEqual(ReaderResult.Success, result3.Result, "File should have decrypted successfully after write");

                KdbxDocument roundTrippedDocument = new KdbxDocument(result3.GetXmlDocument().Root, newReader.HeaderData.GenerateRng());
                Assert.AreEqual(kdbxDoc, roundTrippedDocument, "Round-tripped document should be equal to original document");
            }
        }

        [TestMethod]
        public async Task RoundTrip_Degenerate()
        {
            await roundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_CustomKeyFile()
        {
            await roundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_SampleKeyFile()
        {
            await roundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_KeyFile_32bytes()
        {
            await roundTrip();
        }

        [TestMethod]
        [DatabaseInfo("Degenerate.kdbx", Password="degenerate")]
        public async Task MultiEdit_Degenerate()
        {
            StorageFolder work = await Utils.GetWorkFolder();
            StorageFile workDb = await this.thisTestInfo.Database.CopyAsync(work, "Work.kdbx", NameCollisionOption.ReplaceExisting);

            IKdbxWriter writer;
            KdbxDocument doc;

            var reader = new KdbxReader();
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                var headerResult = await reader.ReadHeader(stream);
                Assert.AreEqual(headerResult, ReaderResult.Success);
            }

            KdbxDecryptionResult bodyResult = null;
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                bodyResult = await reader.DecryptFile(stream, this.thisTestInfo.Password, this.thisTestInfo.Keyfile);
                Assert.AreEqual(bodyResult.Result, ReaderResult.Success);
            }

            writer = reader.GetWriter();
            doc = new KdbxDocument(bodyResult.GetXmlDocument().Root, reader.HeaderData.GenerateRng());

            Assert.IsTrue(await writer.Write(workDb, doc));

            doc.Root.DatabaseGroup.Groups.RemoveAt(doc.Root.DatabaseGroup.Groups.Count - 1);
            Assert.IsTrue(await writer.Write(workDb, doc));

            reader = new KdbxReader();
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                var headerResult = await reader.ReadHeader(stream);
                Assert.AreEqual(headerResult, ReaderResult.Success);
            }
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                bodyResult = await reader.DecryptFile(stream, this.thisTestInfo.Password, this.thisTestInfo.Keyfile);
                Assert.AreEqual(bodyResult.Result, ReaderResult.Success);
            }

            writer = reader.GetWriter();
            doc = new KdbxDocument(bodyResult.GetXmlDocument().Root, reader.HeaderData.GenerateRng());

            doc.Root.DatabaseGroup.Groups.RemoveAt(doc.Root.DatabaseGroup.Groups.Count - 1);
            Assert.IsTrue(await writer.Write(workDb, doc));

            reader = new KdbxReader();
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                var headerResult = await reader.ReadHeader(stream);
                Assert.AreEqual(headerResult, ReaderResult.Success);
            }
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                bodyResult = await reader.DecryptFile(stream, this.thisTestInfo.Password, this.thisTestInfo.Keyfile);
                Assert.AreEqual(bodyResult.Result, ReaderResult.Success);
            }

            writer = reader.GetWriter();
            doc = new KdbxDocument(bodyResult.GetXmlDocument().Root, reader.HeaderData.GenerateRng());
        }
    }
}
