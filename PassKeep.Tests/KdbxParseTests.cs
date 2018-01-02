using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using DatabaseInfo = PassKeep.Tests.Utils.DatabaseInfo;

namespace PassKeep.Tests
{
    [TestClass]
    public partial class KdbxParseTests
    {
        private DatabaseInfo thisTestInfo;

        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return this.testContextInstance; }
            set { this.testContextInstance = value; }
        }

        private IKdbxReader reader;

        [TestInitialize]
        public async Task PrepareForTest()
        {
            this.thisTestInfo = await Utils.GetDatabaseInfoForTest(TestContext);

            this.reader = new KdbxReader();
            Assert.AreEqual(ReaderResult.Success, await this.reader.ReadHeaderAsync(await this.thisTestInfo.Database.AsIStorageFile.OpenReadAsync(), new CancellationTokenSource().Token));
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        private async Task ShouldUnlock(bool expectIdentical = true)
        {
            await ExpectUnlockError(KdbxParserCode.Success, expectIdentical);
        }

        private async Task ExpectUnlockError(KdbxParserCode error, bool expectIdentical = true)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            KdbxDecryptionResult result = await this.reader.DecryptFileAsync(await this.thisTestInfo.Database.AsIStorageFile.OpenReadAsync(), this.thisTestInfo.Password, this.thisTestInfo.Keyfile, cts.Token);
            
            if (result.Result == ReaderResult.Success)
            {
                KdbxDocument oldDocument = result.GetDocument();
                XElement newXml = oldDocument.ToXml(this.reader.HeaderData.GenerateRng(), result.Parameters);
                KdbxDocument newDocument = new KdbxDocument(
                    newXml,
                    this.reader.HeaderData.ProtectedBinaries,
                    this.reader.HeaderData.GenerateRng(),
                    result.Parameters
                );

                Assert.AreEqual(oldDocument, newDocument);
            }

            Assert.AreEqual(error, result.Result.Code);
        }

        [TestMethod]
        public async Task CustomKeyFile()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task SampleKeyFile()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_BadBase64()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_EmptyData()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_NoData()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_NoKey()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_WrongRoot()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KeyFile_32bytes()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KeyFile_64bytes_hex()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KeyFile_64bytes_not_hex()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KeyFile_64bytes_binary()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task Degenerate()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task BadPassword()
        {
            await ExpectUnlockError(KdbxParserCode.CouldNotDecrypt);
        }

        [TestMethod]
        public async Task Password_SampleKeyFile()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_09_Password()
        {
            await ShouldUnlock(false);
        }

        [TestMethod]
        public async Task KP2_14_Password()
        {
            await ShouldUnlock(false);
        }

        [TestMethod]
        public async Task KP2_20_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_24_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_25_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_26_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_27_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_29_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_31_Password_CustomKeyFile()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task ReadOnly_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task NotCompressed_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task ManyRounds_SampleKeyFile()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task ManyMoreRounds_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task TooManyRounds_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_08_MiniKeePass()
        {
            // Regression test case for a bug found in MiniKeePass (iOS) interop
            // for older databases.
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_35_Kdbx4_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_35_Kdbx3_Binaries_Password()
        {
            await ShouldUnlock();
        }

        [TestMethod]
        public async Task KP2_35_Kdbx4_Binaries_Password()
        {
            await ShouldUnlock();
        }
    }
}
