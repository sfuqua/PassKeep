using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Services;
using PassKeep.Models;
using PassKeep.Tests.Attributes;
using PassKeep.Tests.Mocks;
using SariphLib.Files;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

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
            Utils.DatabaseInfo dbInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
            this.fileUnderTest = await dbInfo.Database.CopyAsync(
                ApplicationData.Current.TemporaryFolder,
                $"PersistenceTestDb-{Guid.NewGuid()}.kdbx",
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

        [TestCleanup]
        public async Task Cleanup()
        {
            await this.fileUnderTest.DeleteAsync();
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
        [TestMethod, Timeout(2000), DatabaseInfo("StructureTesting")]
        public async Task BasicWritableFileValidation()
        {
            Assert.IsTrue(this.serviceUnderTest.CanSave, "Should be able to save a writable database.");
            Assert.IsFalse(this.serviceUnderTest.IsSaving, "IsSaving should default to false.");

            int propertyChangedCount = 0;
            this.serviceUnderTest.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.serviceUnderTest.IsSaving))
                {
                    Interlocked.Increment(ref propertyChangedCount);
                }
            };

            Task<bool> savingTask = this.serviceUnderTest.Save(this.document);
            Assert.IsTrue(this.serviceUnderTest.IsSaving, "IsSaving should be true when a save is in progress.");
            Assert.IsTrue(await savingTask, "Saving a writable database should succeed.");
            Assert.AreEqual(2, propertyChangedCount, "IsSaving should change once during the course of the save.");
            Assert.IsFalse(this.serviceUnderTest.IsSaving, "IsSaving should be false once saving is complete.");
        }

        /// <summary>
        /// Validates that subsequent database saves cancel each other 
        /// and only the last one succeeds.
        /// </summary>
        /// <returns></returns>
        [TestMethod, Timeout(2000), DatabaseInfo("ManyMoreRounds_Password")]
        public async Task SubsequentSavesValidation()
        {
            int propertyChangedCount = 0;
            this.serviceUnderTest.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.serviceUnderTest.IsSaving))
                {
                    Interlocked.Increment(ref propertyChangedCount);
                }
            };

            Task<bool> firstSave = Task.Run(() => this.serviceUnderTest.Save(this.document));
            Task<bool> secondSave = Task.Run(() => this.serviceUnderTest.Save(this.document));
            Task<bool> thirdSave = Task.Run(() => this.serviceUnderTest.Save(this.document));
            Task<bool> fourthSave = this.serviceUnderTest.Save(this.document);

            bool[] results = await Task.WhenAll(firstSave, secondSave, thirdSave, fourthSave);
            Assert.AreEqual(1, results.Count(result => result), "Only one parallel save should suceed.");
            
            // If a save is preempted we do NOT transition out of the IsSaving state
            Assert.AreEqual(2, propertyChangedCount, "IsSaving should only have changed twice once the dust settles.");
        }
    }
}
