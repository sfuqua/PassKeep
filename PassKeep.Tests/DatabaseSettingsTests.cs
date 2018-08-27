using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.KeePass.Kdf;
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
        private string dbPassword;
        private ITestableFile dbKeyFile;
        private ITestableFile saveFile;
        private KdbxDocument document;
        private IKdbxWriter writer;
        private IDatabasePersistenceService persistenceService;
        private ICredentialStorageProvider credentialStorage;
        private DatabaseSettingsViewModel settingsVm;
        private MasterKeyChangeViewModel masterKeyVm;

        public override TestContext TestContext { get; set; }

        /// <summary>
        /// Initializes the service under test.
        /// </summary>
        [TestInitialize]
        public async Task Init()
        {
            // Get database from test attributes
            Utils.DatabaseInfo dbInfo = await Utils.GetDatabaseInfoForTest(TestContext);
            this.dbPassword = dbInfo.Password;
            this.dbKeyFile = dbInfo.Keyfile;

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
            this.writer = reader.GetWriter();
            this.persistenceService = new DefaultFilePersistenceService(
                this.writer,
                this.writer,
                new StorageFileDatabaseCandidate(this.saveFile, true),
                new MockSyncContext(),
                true
            );
            this.credentialStorage = new MockCredentialProvider();
            this.masterKeyVm = new MasterKeyChangeViewModel(
                this.document,
                this.saveFile,
                new DatabaseCredentialProvider(this.persistenceService, this.credentialStorage),
                new MockFileService()
            );
            this.settingsVm = new DatabaseSettingsViewModel(this.writer);
        }

        [TestMethod, DatabaseInfo("StructureTesting")]
        public async Task UpgradeCipherSettings()
        {
            DateTime lastPasswordChange = this.document.Metadata.MasterKeyChanged.Value;

            Assert.AreEqual(EncryptionAlgorithm.Aes, this.settingsVm.Cipher, "AES should be the encryption algorithm before the test starts");
            this.writer.Cipher = EncryptionAlgorithm.ChaCha20;

            Assert.IsInstanceOfType(this.settingsVm.GetKdfParameters(), typeof(AesParameters), "AES should be the KDF before the test starts according to the VM");
            Assert.IsInstanceOfType(this.writer.KdfParameters, typeof(AesParameters), "AES should be the KDF before the test starts according to the KdbxWriter");

            this.settingsVm.KdfGuid = Argon2Parameters.Argon2Uuid;
            this.settingsVm.ArgonParallelism = 3;
            this.settingsVm.ArgonBlockCount = 24;
            this.settingsVm.KdfIterations = 2;
            
            Assert.IsInstanceOfType(this.writer.KdfParameters, typeof(Argon2Parameters), "Changes to the settings VM should be reflected in the KdbxWriter");
            Assert.IsTrue(await this.persistenceService.Save(this.document));

            KdbxReader reader = new KdbxReader();
            using (IRandomAccessStream stream = await this.saveFile.AsIStorageFile.OpenReadAsync())
            {
                await reader.ReadHeaderAsync(stream, CancellationToken.None);
                Assert.AreEqual(EncryptionAlgorithm.ChaCha20, reader.HeaderData.Cipher, "New reader should have the correct cipher");
                Argon2Parameters argonParams = reader.HeaderData.KdfParameters as Argon2Parameters;
                Assert.IsNotNull(argonParams, "Database should have properly persisted with Argon2");
                Assert.AreEqual(3, (int)argonParams.Parallelism, "Argon2 parallelism should have been persisted correctly");
                Assert.AreEqual(24, (int)argonParams.BlockCount, "Argon2 block count should have been persisted correctly");
                Assert.AreEqual(2, (int)argonParams.Iterations, "Argon2 iteration count should have been persisted correctly");

                KdbxDecryptionResult decryption = await reader.DecryptFileAsync(stream, this.dbPassword, this.dbKeyFile, CancellationToken.None);
                Assert.AreEqual(KdbxParserCode.Success, decryption.Result.Code);
                KdbxDocument document = decryption.GetDocument();
                Assert.AreEqual(lastPasswordChange, document.Metadata.MasterKeyChanged.Value, "MasterKeyChanged timestamp should not have changed");
            }
        }

        [TestMethod, DatabaseInfo("KP2_35_Kdbx4_Password")]
        public async Task DowngradeCipherSettings()
        {
            DateTime lastPasswordChange = this.document.Metadata.MasterKeyChanged.Value;

            Assert.AreEqual(EncryptionAlgorithm.ChaCha20, this.settingsVm.Cipher, "ChaCha20 should be the encryption algorithm before the test starts");
            this.writer.Cipher = EncryptionAlgorithm.Aes;

            Assert.IsInstanceOfType(this.settingsVm.GetKdfParameters(), typeof(Argon2Parameters), "Argon2 should be the KDF before the test starts according to the VM");
            Assert.IsInstanceOfType(this.writer.KdfParameters, typeof(Argon2Parameters), "Argon2 should be the KDF before the test starts according to the KdbxWriter");

            this.settingsVm.KdfGuid = AesParameters.AesUuid;
            this.settingsVm.KdfIterations = 6001;
            
            Assert.IsInstanceOfType(this.writer.KdfParameters, typeof(AesParameters), "Changes to the settings VM should be reflected in the KdbxWriter");
            Assert.IsTrue(await this.persistenceService.Save(this.document));

            KdbxReader reader = new KdbxReader();
            using (IRandomAccessStream stream = await this.saveFile.AsIStorageFile.OpenReadAsync())
            {
                await reader.ReadHeaderAsync(stream, CancellationToken.None);
                Assert.AreEqual(EncryptionAlgorithm.Aes, reader.HeaderData.Cipher, "New reader should have the correct cipher");
                AesParameters aesParams = reader.HeaderData.KdfParameters as AesParameters;
                Assert.IsNotNull(aesParams, "Database should have properly persisted with AES");
                Assert.AreEqual(6001, (int)aesParams.Rounds, "AES iteration count should have been persisted correctly");

                KdbxDecryptionResult decryption = await reader.DecryptFileAsync(stream, this.dbPassword, this.dbKeyFile, CancellationToken.None);
                Assert.AreEqual(KdbxParserCode.Success, decryption.Result.Code);
                KdbxDocument document = decryption.GetDocument();
                Assert.AreEqual(lastPasswordChange, document.Metadata.MasterKeyChanged.Value, "MasterKeyChanged timestamp should not have changed");
            }
        }

        [TestMethod, DatabaseInfo("StructureTesting")]
        public async Task ChangePassword()
        {
            string newPw = "TestPW";
            this.masterKeyVm.MasterPassword = newPw;
            Assert.IsFalse(this.masterKeyVm.ConfirmCommand.CanExecute(null), "Should not be able to confirm new password until password is entered twice");
            this.masterKeyVm.ConfirmedPassword = newPw;
            Assert.IsTrue(this.masterKeyVm.ConfirmCommand.CanExecute(null), "Should be able to confirm new password when second password matches");
            this.masterKeyVm.ConfirmedPassword = "Garbage";
            Assert.IsFalse(this.masterKeyVm.ConfirmCommand.CanExecute(null), "Mismatched passwords should not be able to be confirmed");
            this.masterKeyVm.ConfirmedPassword = newPw;

            DateTime lastPasswordChange = this.document.Metadata.MasterKeyChanged.Value;
            this.masterKeyVm.ConfirmCommand.Execute(null);
            DateTime passwordChangeTime = this.document.Metadata.MasterKeyChanged.Value;
            Assert.IsTrue(passwordChangeTime > lastPasswordChange, "MasterKeyChanged value should have changed in document metadata");

            Assert.IsTrue(await this.persistenceService.Save(this.document));

            KdbxReader reader = new KdbxReader();
            using (IRandomAccessStream stream = await this.saveFile.AsIStorageFile.OpenReadAsync())
            {
                await reader.ReadHeaderAsync(stream, CancellationToken.None);
                KdbxDecryptionResult decryption = await reader.DecryptFileAsync(stream, newPw, null, CancellationToken.None);
                Assert.AreEqual(KdbxParserCode.Success, decryption.Result.Code, "Database should decrypt with the new credentials");
                KdbxDocument document = decryption.GetDocument();
                Assert.AreEqual(passwordChangeTime, document.Metadata.MasterKeyChanged.Value, "MasterKeyChanged timestamp should have been persisted");
            }
        }
    }
}
