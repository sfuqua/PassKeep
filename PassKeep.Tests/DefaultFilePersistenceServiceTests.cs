using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using SariphLib.Files;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using PassKeep.Lib.Services;
using PassKeep.Lib.KeePass.IO;
using Windows.Storage.Streams;
using System.Threading;
using PassKeep.Models;
using PassKeep.Tests.Mocks;
using PassKeep.Tests.Attributes;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;

namespace PassKeep.Tests
{
    /// <summary>
    /// Tests for <see cref="DefaultFilePersistenceServiceTests"/>.
    /// </summary>
    [TestClass]
    public sealed class DefaultFilePersistenceServiceTests : TestClassBase
    {
        private StorageFile fileUnderTest;
        private KdbxDocument document;
        private DefaultFilePersistenceService serviceUnderTest;

        /// <summary>
        /// TestContext for managing test data.
        /// </summary>
        public override TestContext TestContext { get; set; }

        /// <summary>
        /// Initializes the service under test.
        /// </summary>
        [TestInitialize]
        public async Task Init()
        {
            // Get database from test attributes
            await ApplicationData.Current.ClearAsync(ApplicationDataLocality.Temporary);
            Utils.DatabaseInfo dbInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
            this.fileUnderTest = await dbInfo.Database.CopyAsync(
                ApplicationData.Current.TemporaryFolder,
                "PersistenceTestDb.kdbx",
                NameCollisionOption.ReplaceExisting
            );

            // Use a KdbxReader to parse the database and get a corresponding writer
            KdbxReader reader = new KdbxReader();
            using (IRandomAccessStream stream = await this.fileUnderTest.OpenReadAsync())
            {
                await reader.ReadHeader(stream, CancellationToken.None);
                KdbxDecryptionResult decryption = await reader.DecryptFile(stream, dbInfo.Password, dbInfo.Keyfile, CancellationToken.None);
                Assert.AreEqual(KdbxParserCode.Success, decryption.Result.Code);
                this.document = decryption.GetDocument();
            }

            // Construct a service we can use for the test
            this.serviceUnderTest = new DefaultFilePersistenceService(
                reader.GetWriter(),
                new StorageFileDatabaseCandidate(this.fileUnderTest),
                new MockSyncContext(),
                await this.fileUnderTest.CheckWritableAsync(true)
            );
        }

        /// <summary>
        /// Validate that <see cref="DefaultFilePersistenceService"/> handles readonly files appropriately.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod, DatabaseInfo("ReadOnly_Password")]
        public async Task ReadOnlyFileValidation()
        {
            bool eventFired = false;
            this.serviceUnderTest.PropertyChanged += (s, e) =>
            {
                eventFired = true;
            };

            Assert.IsFalse(this.serviceUnderTest.CanSave, "Should not be able to save a readonly database.");
            Assert.IsFalse(this.serviceUnderTest.IsSaving, "IsSaving should default to false.");
            Assert.IsFalse(await this.serviceUnderTest.Save(this.document), "Saving a readonly database should be unsuccessful.");

            await AwaitableTimeout(1000);

            Assert.IsFalse(eventFired, "PropertyChanged should not fire when attempting to save a readonly database.");
        }

        /// <summary>
        /// Validate that <see cref="DefaultFilePersistenceService"/> performs as expected for writable files
        /// in a basic (single save) scenario.
        /// </summary>
        /// <returns>A task representing the test.</returns>
        [TestMethod, Timeout(1000), DatabaseInfo("StructureTesting")]
        public async Task BasicWritableFileValidation()
        {
            Assert.IsTrue(this.serviceUnderTest.CanSave, "Should be able to save a writable database.");
            Assert.IsFalse(this.serviceUnderTest.IsSaving, "IsSaving should default to false.");

            int propertyChangedCount = 0;
            this.serviceUnderTest.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.serviceUnderTest.IsSaving))
                {
                    propertyChangedCount++;
                }
            };

            Task<bool> savingTask = this.serviceUnderTest.Save(this.document);
            Assert.IsTrue(this.serviceUnderTest.IsSaving, "IsSaving should be true when a save is in progress.");
            Assert.IsTrue(await savingTask, "Saving a writable database should succeed.");
            Assert.AreEqual(2, propertyChangedCount, "IsSaving should change once during the course of the save.");
            Assert.IsFalse(this.serviceUnderTest.IsSaving, "IsSaving should be false once saving is complete.");
        }
    }
}
