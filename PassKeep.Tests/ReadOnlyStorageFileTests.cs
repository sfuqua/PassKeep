using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Services;
using PassKeep.Models;
using PassKeep.Tests.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Tests
{
    [TestClass]
    public class ReadOnlyStorageFileTests : TestClassBase
    {
        public override TestContext TestContext
        {
            get; set;
        }

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod, DatabaseInfo("ReadOnly_Password")]
        public async Task PersistenceServiceCannotSave()
        {
            Utils.DatabaseInfo databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);

            Assert.IsTrue(databaseInfo.Database.Attributes.HasFlag(FileAttributes.ReadOnly), "Database file should be read-only");
            IKdbxReader reader = new KdbxReader();
            using (IRandomAccessStream stream = await databaseInfo.Database.OpenReadAsync())
            {
                await reader.ReadHeader(stream, CancellationToken.None);
                await reader.DecryptFile(stream, databaseInfo.Password, null, CancellationToken.None);
            }

            IDatabasePersistenceService service = new DefaultFilePersistenceService(
                reader.GetWriter(),
                new StorageFileDatabaseCandidate(databaseInfo.Database)
            );

            Assert.IsFalse(service.CanSave, "Should not be able to save a read-only file");
        }
    }
}
