using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.KeePassLib;
using PassKeep.Models;
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
using PassKeep.KeePassLib.SecurityTokens;

namespace PassKeep.KeePassTests
{
    public partial class KdbxParseTests
    {
        private async Task roundTrip()
        {
            StorageFile keyfile = await getKeyFile(thisTestInfo.Keyfile);
            KeePassError result = await reader.DecryptFile(thisTestInfo.Password, keyfile);

            if (result == KeePassError.None)
            {
                KdbxDocument kdbxDoc = new KdbxDocument(reader.Document.Root, reader.GetRng());
                KdbxWriter writer = reader.GetWriter();
                using (var stream = new InMemoryRandomAccessStream())
                {
                    bool writeResult = await writer.Write(stream, kdbxDoc);
                    Assert.IsTrue(writeResult);

                    stream.Seek(0);
                    KdbxReader newReader = new KdbxReader(stream);
                    var result2 = await newReader.ReadHeader();

                    Assert.IsTrue(result2 == KeePassError.None);

                    var result3 = await newReader.DecryptFile(thisTestInfo.Password, keyfile);

                    Assert.IsTrue(result3 == KeePassError.None);

                    KdbxDocument roundTrippedDocument = new KdbxDocument(newReader.Document.Root, writer.GetRng());
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

            KdbxWriter writer;
            KdbxDocument doc;
            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var reader = new KdbxReader(stream))
                {
                    var headerResult = await reader.ReadHeader();
                    Assert.AreEqual(headerResult, KeePassError.None);
                    var bodyResult = await reader.DecryptFile(password, keyFile);
                    Assert.AreEqual(bodyResult, KeePassError.None);

                    writer = reader.GetWriter();
                    doc = new KdbxDocument(reader.Document.Root, reader.GetRng());
                }
            }

            Assert.IsTrue(await writer.Write(workDb, doc));

            doc.Root.DatabaseGroup.Groups.RemoveAt(doc.Root.DatabaseGroup.Groups.Count - 1);
            Assert.IsTrue(await writer.Write(workDb, doc));

            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var reader = new KdbxReader(stream))
                {
                    var headerResult = await reader.ReadHeader();
                    Assert.AreEqual(headerResult, KeePassError.None);
                    var bodyResult = await reader.DecryptFile(password, keyFile);
                    Assert.AreEqual(bodyResult, KeePassError.None);

                    writer = reader.GetWriter();
                    doc = new KdbxDocument(reader.Document.Root, reader.GetRng());
                }
            }

            doc.Root.DatabaseGroup.Groups.RemoveAt(doc.Root.DatabaseGroup.Groups.Count - 1);
            Assert.IsTrue(await writer.Write(workDb, doc));

            using (var stream = await workDb.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var reader = new KdbxReader(stream))
                {
                    var headerResult = await reader.ReadHeader();
                    Assert.AreEqual(headerResult, KeePassError.None);
                    var bodyResult = await reader.DecryptFile(password, keyFile);
                    Assert.AreEqual(bodyResult, KeePassError.None);

                    writer = reader.GetWriter();
                    doc = new KdbxDocument(reader.Document.Root, reader.GetRng());
                }
            }
        }
    }
}
