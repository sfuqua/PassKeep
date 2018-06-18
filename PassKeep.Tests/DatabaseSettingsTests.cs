using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.Models;
using PassKeep.Tests.Attributes;
using PassKeep.Tests.Mocks;
using SariphLib.Files;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Tests
{
    [TestClass]
    public class DatabaseSettingsTests : TestClassBase
    {
        private ITestableFile saveFile;
        private KdbxDocument document;

        private IDatabasePersistenceService persistenceService;
        private ICredentialStorageProvider credentialStorage;
        private MasterKeyChangeViewModel vmUnderTest;

        public override TestContext TestContext { get; set; }

        /// <summary>
        /// Initializes the service under test.
        /// </summary>
        [TestInitialize]
        public async Task Init()
        {
            // Get database from test attributes
            Utils.DatabaseInfo dbInfo = await Utils.GetDatabaseInfoForTest(TestContext);

            // Assert that databases named *ReadOnly* are actually readonly after a clone
            if (dbInfo.Database.Name.IndexOf("ReadOnly", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Assert.IsFalse(
                    await dbInfo.Database.CheckWritableAsync(),
                    $"This file is expected to be read-only; please verify this before testing: {dbInfo.Database.Name}"
                );
            }

            this.saveFile = (await dbInfo.Database.AsIStorageFile.CopyAsync(
                ApplicationData.Current.TemporaryFolder,
                $"PersistenceTestDb-{Guid.NewGuid()}.kdbx",
                NameCollisionOption.ReplaceExisting
            )).AsWrapper();

            // Use a KdbxReader to parse the database and get a corresponding writer
            KdbxReader reader = new KdbxReader();
            using (IRandomAccessStream stream = await this.saveFile.AsIStorageFile.OpenReadAsync())
            {
                await reader.ReadHeaderAsync(stream, CancellationToken.None);
                KdbxDecryptionResult decryption = await reader.DecryptFileAsync(stream, dbInfo.Password, dbInfo.Keyfile, CancellationToken.None);
                Assert.AreEqual(KdbxParserCode.Success, decryption.Result.Code);
                this.document = decryption.GetDocument();
            }

            // Construct services we can use for the test
            IKdbxWriter writer = reader.GetWriter();
            this.persistenceService = new DefaultFilePersistenceService(
                writer,
                writer,
                new StorageFileDatabaseCandidate(this.saveFile, true),
                new MockSyncContext(),
                true
            );
            this.credentialStorage = new MockCredentialProvider();
            this.vmUnderTest = new MasterKeyChangeViewModel(
                this.document,
                this.saveFile,
                new DatabaseCredentialProvider(this.persistenceService, this.credentialStorage),
                new MockFileService()
            );
        }

        [TestMethod, DatabaseInfo("StructureTesting")]
        public async Task ConfigureSerializationSettings()
        {
            await Task.CompletedTask;
        }
    }
}
