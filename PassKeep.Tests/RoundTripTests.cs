﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Contracts.Models;
using PassKeep.Tests.Attributes;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Services;
using PassKeep.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using PassKeep.Lib.Providers;
using SariphLib.Files;
using PassKeep.Tests.Mocks;

namespace PassKeep.Tests
{
    public partial class KdbxParseTests
    {
        private async Task RoundTrip()
        {
            ReaderResult initialHeaderResult = await this.reader.ReadHeaderAsync(await this.thisTestInfo.Database.AsIStorageFile.OpenReadAsync(), CancellationToken.None);
            Assert.AreEqual(ReaderResult.Success, initialHeaderResult, "Initial header read should be successful");

            KdbxDecryptionResult result = await this.reader.DecryptFileAsync(await this.thisTestInfo.Database.AsIStorageFile.OpenReadAsync(), this.thisTestInfo.Password, this.thisTestInfo.Keyfile, CancellationToken.None);

            Assert.AreEqual(ReaderResult.Success, result.Result, "File should have initially decrypted properly");
            KdbxDocument kdbxDoc = result.GetDocument();
            IKdbxWriter writer = this.reader.GetWriter();
            using (var stream = new InMemoryRandomAccessStream())
            {
                bool writeResult = await writer.WriteAsync(stream, kdbxDoc, CancellationToken.None);
                Assert.IsTrue(writeResult, "File should have written successfully");

                stream.Seek(0);
                KdbxReader newReader = new KdbxReader();
                ReaderResult result2 = await newReader.ReadHeaderAsync(stream, CancellationToken.None);

                Assert.AreEqual(ReaderResult.Success, result2, "Header should been read back successfully after write");

                KdbxDecryptionResult result3 = await newReader.DecryptFileAsync(stream, this.thisTestInfo.Password, this.thisTestInfo.Keyfile, CancellationToken.None);

                Assert.AreEqual(ReaderResult.Success, result3.Result, "File should have decrypted successfully after write");

                KdbxDocument roundTrippedDocument = result3.GetDocument();
                Assert.AreEqual(kdbxDoc, roundTrippedDocument, "Round-tripped document should be equal to original document");
            }
        }

        [TestMethod]
        public async Task RoundTrip_Degenerate()
        {
            await RoundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_CustomKeyFile()
        {
            await RoundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_SampleKeyFile()
        {
            await RoundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_KeyFile_32bytes()
        {
            await RoundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_KP2_08_MiniKeePass()
        {
            await RoundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_KP2_35_Kdbx4_Password()
        {
            await RoundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_KP2_35_Kdbx3_Binaries_Password()
        {
            await RoundTrip();
        }

        [TestMethod]
        public async Task RoundTrip_KP2_35_Kdbx4_Binaries_Password()
        {
            await RoundTrip();
        }

        [TestMethod]
        [DatabaseInfo("Degenerate.kdbx", Password="degenerate")]
        public async Task MultiEdit_Degenerate()
        {
            StorageFileDatabaseCandidateFactory factory = new StorageFileDatabaseCandidateFactory(new MockFileProxyProvider { ScopeValue = true });
            StorageFolder work = await Utils.GetWorkFolder();
            IDatabaseCandidate workDb = await factory.AssembleAsync(
                (await this.thisTestInfo.Database.AsIStorageFile.CopyAsync(work, "Work.kdbx", NameCollisionOption.ReplaceExisting))
                    .AsWrapper()
            );

            IKdbxWriter writer;
            KdbxDocument doc;

            var reader = new KdbxReader();
            using (IRandomAccessStream stream = await workDb.GetRandomReadAccessStreamAsync())
            {
                ReaderResult headerResult = await reader.ReadHeaderAsync(stream, CancellationToken.None);
                Assert.AreEqual(headerResult, ReaderResult.Success);
            }

            KdbxDecryptionResult bodyResult = null;
            using (IRandomAccessStream stream = await workDb.GetRandomReadAccessStreamAsync())
            {
                bodyResult = await reader.DecryptFileAsync(stream, this.thisTestInfo.Password, this.thisTestInfo.Keyfile, CancellationToken.None);
                Assert.AreEqual(bodyResult.Result, ReaderResult.Success);
            }

            writer = reader.GetWriter();
            doc = bodyResult.GetDocument();

            IDatabasePersistenceService persistor = new DefaultFilePersistenceService(writer, writer, workDb, new MockSyncContext(), await workDb.File.CheckWritableAsync());

            Assert.IsTrue(persistor.CanSave);
            Assert.IsTrue(await persistor.Save(doc));

            // Remove the last group
            doc.Root.DatabaseGroup.Children.RemoveAt(
                doc.Root.DatabaseGroup.Children.IndexOf(
                    doc.Root.DatabaseGroup.Children.Last(node => node is IKeePassGroup)
                )
            );
            Assert.IsTrue(await persistor.Save(doc));

            reader = new KdbxReader();
            using (IRandomAccessStream stream = await workDb.GetRandomReadAccessStreamAsync())
            {
                ReaderResult headerResult = await reader.ReadHeaderAsync(stream, CancellationToken.None);
                Assert.AreEqual(headerResult, ReaderResult.Success);
            }
            using (IRandomAccessStream stream = await workDb.GetRandomReadAccessStreamAsync())
            {
                bodyResult = await reader.DecryptFileAsync(stream, this.thisTestInfo.Password, this.thisTestInfo.Keyfile, CancellationToken.None);
                Assert.AreEqual(bodyResult.Result, ReaderResult.Success);
            }

            writer = reader.GetWriter();
            doc = bodyResult.GetDocument();

            doc.Root.DatabaseGroup.Children.RemoveAt(
                doc.Root.DatabaseGroup.Children.IndexOf(
                    doc.Root.DatabaseGroup.Children.Last(node => node is IKeePassGroup)
                )
            );
            Assert.IsTrue(await persistor.Save(doc));

            reader = new KdbxReader();
            using (IRandomAccessStream stream = await workDb.GetRandomReadAccessStreamAsync())
            {
                ReaderResult headerResult = await reader.ReadHeaderAsync(stream, CancellationToken.None);
                Assert.AreEqual(headerResult, ReaderResult.Success);
            }
            using (IRandomAccessStream stream = await workDb.GetRandomReadAccessStreamAsync())
            {
                bodyResult = await reader.DecryptFileAsync(stream, this.thisTestInfo.Password, this.thisTestInfo.Keyfile, CancellationToken.None);
                Assert.AreEqual(bodyResult.Result, ReaderResult.Success);
            }

            writer = reader.GetWriter();
            doc = bodyResult.GetDocument();
        }
    }
}
