using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Contracts.Models;
using PassKeep.KeePassTests.Attributes;
using PassKeep.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace PassKeep.KeePassTests
{
    /// <summary>
    /// Common utilities used by tests.
    /// </summary>
    public static class Utils
    {
        // A map of known document files and their credentials.
        private static readonly Dictionary<string, Task<DatabaseInfo>> databaseMap = new Dictionary<string, Task<DatabaseInfo>>()
        {
            {
                "CustomKeyFile",
                DatabaseInfo.Create("CustomKeyFile.kdbx", keyfileName: "CustomKeyFile.key")
            },
            {
                "SampleKeyFile",
                DatabaseInfo.Create("SampleKeyFile.kdbx", keyfileName: "SampleKeyFile.xml")
            },
            {
                "ValidXml_BadBase64",
                DatabaseInfo.Create("ValidXml_BadBase64.kdbx", keyfileName: "ValidXml_BadBase64.xml")
            },
            {
                "ValidXml_EmptyData",
                DatabaseInfo.Create("ValidXml_EmptyData.kdbx", keyfileName: "ValidXml_EmptyData.xml")
            },
            {
                "ValidXml_NoData",
                DatabaseInfo.Create("ValidXml_NoData.kdbx", keyfileName: "ValidXml_NoData.xml")
            },
            {
                "ValidXml_NoKey",
                DatabaseInfo.Create("ValidXml_NoKey.kdbx", keyfileName: "ValidXml_NoKey.xml")
            },
            {
                "ValidXml_WrongRoot",
                DatabaseInfo.Create("ValidXml_WrongRoot.kdbx", keyfileName: "ValidXml_WrongRoot.xml")
            },
            {
                "KeyFile_32bytes",
                DatabaseInfo.Create("KeyFile_32bytes.kdbx", keyfileName: "32bytes.txt")
            },
            {
                "KeyFile_64bytes_hex",
                DatabaseInfo.Create("KeyFile_64bytes_hex.kdbx", keyfileName: "64bytes_hex.txt")
            },
            {
                "KeyFile_64bytes_not_hex",
                DatabaseInfo.Create("KeyFile_64bytes_not_hex.kdbx", keyfileName: "64bytes_not_hex.txt")
            },
            {
                "Degenerate",
                DatabaseInfo.Create("Degenerate.kdbx", "degenerate")
            },
            {
                "BadPassword",
                DatabaseInfo.Create("Degenerate.kdbx", "xxx")
            },
            {
                "Password_SampleKeyFile",
                DatabaseInfo.Create("Password_SampleKeyFile.kdbx", "password", "SampleKeyFile.xml")
            },
            {
                "KP2_09_Password",
                DatabaseInfo.Create("KP2_09_Password.kdbx", "password")
            },
            {
                "KP2_14_Password",
                DatabaseInfo.Create("KP2_14_Password.kdbx", "password")
            },
            {
                "KP2_20_Password",
                DatabaseInfo.Create("KP2_20_Password.kdbx", "password")
            },
            {
                "KP2_24_Password",
                DatabaseInfo.Create("KP2_24_Password.kdbx", "password")
            },
            {
                "KP2_25_Password",
                DatabaseInfo.Create("KP2_25_Password.kdbx", "password")
            },
            {
                "KP2_26_Password",
                DatabaseInfo.Create("KP2_26_Password.kdbx", "password")
            },
            {
                "KP2_27_Password",
                DatabaseInfo.Create("KP2_27_Password.kdbx", "password")
            },
            {
                "NotCompressed_Password",
                DatabaseInfo.Create("NotCompressed_Password.kdbx", "password")
            },
            {
                "ManyRounds_SampleKeyFile",
                DatabaseInfo.Create("12000Rounds_SampleKeyFile.kdbx", keyfileName: "SampleKeyFile.xml")
            },
            {
                "ManyMoreRounds_Password",
                DatabaseInfo.Create("120000Rounds_Password.kdbx", "password")
            },
            {
                "TooManyRounds_Password",
                DatabaseInfo.Create("1sDelay_Password.kdbx", "password")
            },
            {
                "StructureTesting",
                DatabaseInfo.Create("StructureTesting.kdbx", "password")
            },
            {
                "Unsearchable",
                DatabaseInfo.Create("Unsearchable.kdbx", "password")
            },
            {
                "KP2_08_MiniKeePass",
                DatabaseInfo.Create("KP2_08_MiniKeePass.kdbx", "password")
            }
        };

        /// <summary>
        /// Retrieves a StorageFile from the specified folder with the specified file name.
        /// </summary>
        /// <param name="folder">Where to open the StorageFile from.</param>
        /// <param name="file">The name of the file to open.</param>
        /// <returns>A Task representing a StorageFile.</returns>
        public static async Task<StorageFile> GetPackagedFile(string folder, string file)
        {
            StorageFolder installFolder = Package.Current.InstalledLocation;

            if (folder != null)
            {
                StorageFolder subFolder = await installFolder.GetFolderAsync(folder);
                return await subFolder.GetFileAsync(file);
            }
            else
            {
                return await installFolder.GetFileAsync(file);
            }
        }

        /// <summary>
        /// Shorthand for GetPackagedFile to locate a specific document by filename.
        /// </summary>
        /// <param name="fileName">The document to look up.</param>
        /// <returns>A Task representing a StorageFile for the desired document.</returns>
        public static async Task<IStorageFile> GetDatabaseByName(string fileName)
        {
            return await Utils.GetPackagedFile("Databases", fileName);
        }

        /// <summary>
        /// Gets a "work" folder useful for scratch operations.
        /// </summary>
        /// <returns>A Task representing a scratch StorageFolder.</returns>
        public static async Task<StorageFolder> GetWorkFolder()
        {
            StorageFolder workFolder = await
                ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Work", CreationCollisionOption.ReplaceExisting);
            return workFolder;
        }

        public static async Task<DatabaseInfo> GetDatabaseInfoForTest(TestContext testContext)
        {
            Type testClass = Type.GetType(testContext.FullyQualifiedTestClassName, true);
            MethodInfo testMethod = testClass.GetRuntimeMethod(testContext.TestName, new Type[0]);
            DatabaseInfoAttribute infoAttribute = testMethod.GetCustomAttribute<DatabaseInfoAttribute>();

            // Use the attribute if it exists
            if (infoAttribute != null)
            {
                string databaseName = infoAttribute.DatabaseName;
                string password = infoAttribute.Password;
                string keyfileName = infoAttribute.KeyFileName;

                string databaseKey = databaseName;
                int dotIndex = databaseName.LastIndexOf('.');
                if (dotIndex >= 0)
                {
                    databaseKey = databaseKey.Substring(0, dotIndex);
                }

                if (infoAttribute.UseDefaultCredentials)
                {
                    if (!Utils.databaseMap.ContainsKey(databaseKey))
                    {
                        throw new InvalidOperationException("Cannot use default credentials of undefined database: " + databaseName);
                    }

                    return await Utils.databaseMap[databaseKey];
                }

                return await DatabaseInfo.Create(databaseName, password, keyfileName, infoAttribute.IsSample);
            }

            // Otherwise try to glean something from the method name (legacy approach)
            string key = testContext.TestName;
            if (key.StartsWith("RoundTrip_"))
            {
                key = key.Substring("RoundTrip_".Length);
            }

            if (!databaseMap.ContainsKey(key))
            {
                throw new InvalidOperationException("Unable to divine what database info to use...");
            }

            return await databaseMap[key];
        }

        /// <summary>
        /// Represents a document file and credentials to attempt unlocking it.
        /// </summary>
        public class DatabaseInfo
        {
            /// <summary>
            /// The document file.
            /// </summary>
            public IStorageFile Database { get; private set; }
            
            /// <summary>
            /// Key file to use for the document.
            /// </summary>
            public StorageFile Keyfile { get; private set; }

            /// <summary>
            /// Password to use for the document.
            /// </summary>
            public string Password { get; private set; }

            /// <summary>
            /// Whether this represents a PassKeep sample document.
            /// </summary>
            public bool IsSample { get; private set; }

            /// <summary>
            /// Initializes the class.
            /// </summary>
            /// <param name="databaseName">Name of the document file this instance represents.</param>
            /// <param name="password">Password ot use for document decryption.</param>
            /// <param name="keyfileName">File name of the keyfile to use for decryption.</param>
            /// <param name="isSample">Whether this represents a PassKeep sample document.</param>
            public static async Task<DatabaseInfo> Create(string databaseName, string password = "", string keyfileName = null, bool isSample = false)
            {
                if (String.IsNullOrEmpty(databaseName))
                {
                    throw new ArgumentNullException("databaseName");
                }
                
                IStorageFile database = await GetDatabaseByName(databaseName);

                StorageFile keyfile =  null;
                if (!String.IsNullOrEmpty(keyfileName))
                {
                    keyfile = await Utils.GetPackagedFile("Keys", keyfileName);
                }

                return new DatabaseInfo
                {
                    Database = database,
                    Password = password,
                    Keyfile = keyfile,
                    IsSample = isSample
                };
            }
        }
    }
}
