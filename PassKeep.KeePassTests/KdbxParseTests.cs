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

namespace PassKeep.KeePassTests
{
    [TestClass]
    public partial class KdbxParseTests
    {
        public class DatabaseInfo
        {
            public string Database { get; set; }
            public string Keyfile {get; set; }
            public string Password {get; set; }
            public bool UseAccount {get; set; }

            public DatabaseInfo(string database)
            {
                Database = database;
                Keyfile = string.Empty;
                Password = string.Empty;
                UseAccount = false;
            }
        }

        private static readonly Dictionary<string, DatabaseInfo> databaseMap = new Dictionary<string, DatabaseInfo>()
        {
            {
                "CustomKeyFile",
                new DatabaseInfo("CustomKeyFile.kdbx")
                {
                    Keyfile = "CustomKeyFile.key"
                }
            },
            {
                "SampleKeyFile",
                new DatabaseInfo("SampleKeyFile.kdbx")
                {
                    Keyfile = "SampleKeyFile.xml"
                }
            },
            {
                "ValidXml_BadBase64",
                new DatabaseInfo("ValidXml_BadBase64.kdbx")
                {
                    Keyfile = "ValidXml_BadBase64.xml"
                }
            },
            {
                "ValidXml_EmptyData",
                new DatabaseInfo("ValidXml_EmptyData.kdbx")
                {
                    Keyfile = "ValidXml_EmptyData.xml"
                }
            },
            {
                "ValidXml_NoData",
                new DatabaseInfo("ValidXml_NoData.kdbx")
                {
                    Keyfile = "ValidXml_NoData.xml"
                }
            },
            {
                "ValidXml_NoKey",
                new DatabaseInfo("ValidXml_NoKey.kdbx")
                {
                    Keyfile = "ValidXml_NoKey.xml"
                }
            },
            {
                "ValidXml_WrongRoot",
                new DatabaseInfo("ValidXml_WrongRoot.kdbx")
                {
                    Keyfile = "ValidXml_WrongRoot.xml"
                }
            },
            {
                "KeyFile_32bytes",
                new DatabaseInfo("KeyFile_32bytes.kdbx")
                {
                    Keyfile = "32bytes.txt"
                }
            },
            {
                "KeyFile_64bytes_hex",
                new DatabaseInfo("KeyFile_64bytes_hex.kdbx")
                {
                    Keyfile = "64bytes_hex.txt"
                }
            },
            {
                "KeyFile_64bytes_not_hex",
                new DatabaseInfo("KeyFile_64bytes_not_hex.kdbx")
                {
                    Keyfile = "64bytes_not_hex.txt"
                }
            },
            {
                "Degenerate",
                new DatabaseInfo("Degenerate.kdbx")
                {
                    Password = "degenerate"
                }
            },
            {
                "BadPassword",
                new DatabaseInfo("Degenerate.kdbx")
                {
                    Password = "xxx"
                }
            },
            {
                "Password_SampleKeyFile",
                new DatabaseInfo("Password_SampleKeyFile.kdbx")
                {
                    Password = "password",
                    Keyfile = "SampleKeyFile.xml"
                }
            },
            {
                "KP2_09_Password",
                new DatabaseInfo("KP2_09_Password.kdbx")
                {
                    Password = "password",
                }
            },
            {
                "KP2_14_Password",
                new DatabaseInfo("KP2_14_Password.kdbx")
                {
                    Password = "password",
                }
            },
            {
                "KP2_20_Password",
                new DatabaseInfo("KP2_20_Password.kdbx")
                {
                    Password = "password",
                }
            },
            {
                "NotCompressed_Password",
                new DatabaseInfo("NotCompressed_Password.kdbx")
                {
                    Password = "password"
                }
            },
            {
                "ManyRounds_SampleKeyFile",
                new DatabaseInfo("12000Rounds_SampleKeyFile.kdbx")
                {
                    Keyfile = "SampleKeyFile.xml"
                }
            },
            {
                "ManyMoreRounds_Password",
                new DatabaseInfo("120000Rounds_Password.kdbx")
                {
                    Password = "password"
                }
            },
            {
                "TooManyRounds_Password",
                new DatabaseInfo("1sDelay_Password.kdbx")
                {
                    Password = "password"
                }
            },
        };

        private DatabaseInfo thisTestInfo;

        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        private IKdbxReader reader;

        private async Task<StorageFile> getDatabaseFileForTest() {
            string key = TestContext.TestName;
            if (key.StartsWith("RoundTrip_"))
            {
                key = key.Substring("RoundTrip_".Length);
            }

            if (!databaseMap.ContainsKey(key))
            {
                return null;
            }
            thisTestInfo = databaseMap[key];

            return await Utils.GetPackagedFile("Databases", thisTestInfo.Database);
        }

        [TestInitialize]
        public async Task PrepareForTest()
        {
            StorageFile testFile = await getDatabaseFileForTest();
            if (testFile == null)
            {
                return;
            }

            reader = new KdbxReader();
            Assert.IsTrue(await reader.ReadHeader(await testFile.OpenReadAsync()) == KeePassError.None);
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        private async Task<StorageFile> getKeyFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return null;
            }

            StorageFile key = await Utils.GetPackagedFile("Keys", file);
            return key;
        }

        private async Task shouldUnlock(bool expectIdentical = true)
        {
            await expectUnlockError(KdbxParseError.None, expectIdentical);
        }

        private async Task expectUnlockError(KdbxParseError error, bool expectIdentical = true)
        {
            StorageFile keyfile = await getKeyFile(thisTestInfo.Keyfile);
            DecryptionResult result = await reader.DecryptFile(await (await getDatabaseFileForTest()).OpenReadAsync(), thisTestInfo.Password, keyfile);

            if (result.Error == KeePassError.None)
            {
                XElement oldXml = result.GetXmlDocument().Root;
                KdbxDocument oldDocument = new KdbxDocument(oldXml, result.GetDocumentRng());
                XElement newXml = oldDocument.ToXml(result.GetDocumentRng());
                KdbxDocument newDocument = new KdbxDocument(newXml, result.GetDocumentRng());

                Assert.AreEqual(oldDocument, newDocument);
            }

            Assert.AreEqual(result.Error.Code, error);
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
            await expectUnlockError(KdbxParseError.CouldNotDecrypt);
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
    }
}
