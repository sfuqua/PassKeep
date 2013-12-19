using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.KeePassTests
{
    public partial class KdbxParseTests
    {
        private async Task roundTrip()
        {
            StorageFile keyfile = await getKeyFile(thisTestInfo.Keyfile);
            DecryptionResult result = await reader.DecryptFile(await (await getDatabaseFileForTest()).OpenReadAsync(), thisTestInfo.Password, keyfile);

            if (result.Error == KeePassError.None)
            {
                KdbxDocument kdbxDoc = new KdbxDocument(result.GetXmlDocument().Root, result.GetDocumentRng());
                IKdbxWriter writer = reader.GetWriter();
                using (var stream = new InMemoryRandomAccessStream())
                {
                    bool writeResult = await writer.Write(stream, kdbxDoc);
                    Assert.IsTrue(writeResult);

                    stream.Seek(0);
                    KdbxReader newReader = new KdbxReader();
                    var result2 = await newReader.ReadHeader(stream);

                    Assert.IsTrue(result2 == KeePassError.None);

                    var result3 = await newReader.DecryptFile(stream, thisTestInfo.Password, keyfile);

                    Assert.IsTrue(result3.Error == KeePassError.None);

                    KdbxDocument roundTrippedDocument = new KdbxDocument(result3.GetXmlDocument().Root, result3.GetDocumentRng());
                    Assert.AreEqual(kdbxDoc, roundTrippedDocument);
                }
            }
            else
            {
                Assert.IsTrue(false);
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
        public async Task MultiEdit_Degenerate()
        {
            StorageFile degenDb = await Utils.GetPackagedFile("Databases", "Degenerate.kdbx");
            StorageFolder work = await Utils.GetWorkFolder();
            StorageFile workDb = await degenDb.CopyAsync(work, "Work.kdbx", NameCollisionOption.ReplaceExisting);

            string password = "degenerate";
            StorageFile keyFile = null;

            IKdbxWriter writer;
            KdbxDocument doc;

            var reader = new KdbxReader();
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                var headerResult = await reader.ReadHeader(stream);
                Assert.AreEqual(headerResult, KeePassError.None);
            }

            DecryptionResult bodyResult = null;
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                bodyResult = await reader.DecryptFile(stream, password, keyFile);
                Assert.AreEqual(bodyResult.Error, KeePassError.None);
            }

            writer = reader.GetWriter();
            doc = new KdbxDocument(bodyResult.GetXmlDocument().Root, bodyResult.GetDocumentRng());

            Assert.IsTrue(await writer.Write(workDb, doc));

            doc.Root.DatabaseGroup.Groups.RemoveAt(doc.Root.DatabaseGroup.Groups.Count - 1);
            Assert.IsTrue(await writer.Write(workDb, doc));

            reader = new KdbxReader();
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                var headerResult = await reader.ReadHeader(stream);
                Assert.AreEqual(headerResult, KeePassError.None);
            }
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                bodyResult = await reader.DecryptFile(stream, password, keyFile);
                Assert.AreEqual(bodyResult.Error, KeePassError.None);
            }

            writer = reader.GetWriter();
            doc = new KdbxDocument(bodyResult.GetXmlDocument().Root, bodyResult.GetDocumentRng());

            doc.Root.DatabaseGroup.Groups.RemoveAt(doc.Root.DatabaseGroup.Groups.Count - 1);
            Assert.IsTrue(await writer.Write(workDb, doc));

            reader = new KdbxReader();
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                var headerResult = await reader.ReadHeader(stream);
                Assert.AreEqual(headerResult, KeePassError.None);
            }
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                bodyResult = await reader.DecryptFile(stream, password, keyFile);
                Assert.AreEqual(bodyResult.Error, KeePassError.None);
            }

            writer = reader.GetWriter();
            doc = new KdbxDocument(bodyResult.GetXmlDocument().Root, bodyResult.GetDocumentRng());
        }
    }
}
