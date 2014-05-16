using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.Storage;
using Windows.Storage.Streams;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.KeePass.Dom;
using DatabaseInfo = PassKeep.KeePassTests.Utils.DatabaseInfo;
using System.Threading;

namespace PassKeep.KeePassTests
{
    [TestClass]
    public partial class KdbxParseTests
    {
        private DatabaseInfo thisTestInfo;

        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private IKdbxReader reader;

        [TestInitialize]
        public async Task PrepareForTest()
        {
            this.thisTestInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);

            this.reader = new KdbxReader();
            Assert.IsTrue(await reader.ReadHeader(await this.thisTestInfo.Database.OpenReadAsync(), new CancellationTokenSource().Token) == ReaderResult.Success);
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        private async Task shouldUnlock(bool expectIdentical = true)
        {
            await expectUnlockError(KdbxParserCode.Success, expectIdentical);
        }

        private async Task expectUnlockError(KdbxParserCode error, bool expectIdentical = true)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            KdbxDecryptionResult result = await reader.DecryptFile(await this.thisTestInfo.Database.OpenReadAsync(), thisTestInfo.Password, this.thisTestInfo.Keyfile, cts.Token);

            if (result.Result == ReaderResult.Success)
            {
                KdbxDocument oldDocument = result.GetDocument();
                XElement newXml = oldDocument.ToXml(reader.HeaderData.GenerateRng());
                KdbxDocument newDocument = new KdbxDocument(newXml, reader.HeaderData.GenerateRng());

                Assert.AreEqual(oldDocument, newDocument);
            }

            Assert.AreEqual(result.Result.Code, error);
        }

        [TestMethod]
        public async Task CustomKeyFile()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task SampleKeyFile()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_BadBase64()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_EmptyData()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_NoData()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_NoKey()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task ValidXml_WrongRoot()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task KeyFile_32bytes()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task KeyFile_64bytes_hex()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task KeyFile_64bytes_not_hex()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task Degenerate()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task BadPassword()
        {
            await expectUnlockError(KdbxParserCode.CouldNotDecrypt);
        }

        [TestMethod]
        public async Task Password_SampleKeyFile()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task KP2_09_Password()
        {
            await shouldUnlock(false);
        }

        [TestMethod]
        public async Task KP2_14_Password()
        {
            await shouldUnlock(false);
        }

        [TestMethod]
        public async Task KP2_20_Password()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task NotCompressed_Password()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task ManyRounds_SampleKeyFile()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task ManyMoreRounds_Password()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task TooManyRounds_Password()
        {
            await shouldUnlock();
        }

        [TestMethod]
        public async Task KP2_08_MiniKeePass()
        {
            // Regression test case for a bug found in MiniKeePass (iOS) interop
            // for older databases.
            await shouldUnlock();
        }
    }
}
