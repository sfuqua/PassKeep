using Microsoft.VisualStudio.TestTools.UnitTesting;
using PassKeep.Contracts.Models;
using PassKeep.Lib.Services;
using PassKeep.Tests.Mocks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace PassKeep.Tests
{
    /// <summary>
    /// Tests for various aspects of storing credentials, including:
    ///  * <see cref="PasswordVaultCredentialProvider"/>
    /// </summary>
    [TestClass]
    public class PasswordVaultTests : TestClassBase
    {
        // Tested value for how many values we can store in the PasswordVault
        private const int MaxStores = 20;

        private PasswordVaultCredentialProvider credentialProvider;

        // A candidate that intentionally has NOT been saved
        private readonly IDatabaseCandidate notStoredCandidate =
            new MockDatabaseCandidate
            {
                FileName = "not-stored.kdbx"
            };

        // A candidate to use when saving data
        private readonly IDatabaseCandidate storedCandidate =
            new MockDatabaseCandidate
            {
                FileName = "stored.kdbx"
            };

        // Data to use for storedCandidate above
        private readonly byte[] mockPasswordData;
        private readonly IBuffer mockPasswordBuffer;

        public override TestContext TestContext { get; set; }

        public PasswordVaultTests()
        {
            this.mockPasswordData = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            this.mockPasswordBuffer = CryptographicBuffer.CreateFromByteArray(this.mockPasswordData);
        }

        /// <summary>
        /// Reset the password vault before each test.
        /// </summary>
        [TestInitialize]
        public async Task TestInit()
        {
            this.credentialProvider = new PasswordVaultCredentialProvider();
            await this.credentialProvider.ClearAsync();
            IReadOnlyCollection<string> allCreds = await this.credentialProvider.GetAllEntriesAsync();
            Assert.AreEqual(0, allCreds.Count, "Test should always start with an empty vault");
        }

        [TestMethod]
        public async Task RetrievingNothingReturnsNull()
        {
            Assert.IsNull(
                await this.credentialProvider.GetRawKeyAsync(this.notStoredCandidate.FileName),
                "CredentialProvider.GetRawKey should return null when the database is not stored"
            );
        }

        [TestMethod]
        public async Task DeletingNothingDoesNotThrow()
        {
            await this.credentialProvider.DeleteAsync(this.notStoredCandidate.FileName);
        }

        [TestMethod]
        public async Task StoredDataIsRetrievable()
        {
            Assert.IsNull(
                await this.credentialProvider.GetRawKeyAsync(this.storedCandidate.FileName),
                "StoredCandidate should not be stored yet"
            );

            Assert.IsTrue(
                await this.credentialProvider.TryStoreRawKeyAsync(this.storedCandidate.FileName, this.mockPasswordBuffer),
                "Data should be storable"
            );

            IBuffer retrievedData = await this.credentialProvider.GetRawKeyAsync(this.storedCandidate.FileName);
            Assert.IsNotNull(retrievedData, "Data should been retrieved after storing");

            Assert.IsTrue(
                CryptographicBuffer.Compare(this.mockPasswordBuffer, retrievedData),
                "Data should roundtrip properly"
            );
        }

        [TestMethod]
        public async Task StoredDataIsDeletable()
        {
            await StoredDataIsRetrievable();
            await this.credentialProvider.DeleteAsync(this.storedCandidate.FileName);
            Assert.IsNull(
                await this.credentialProvider.GetRawKeyAsync(this.storedCandidate.FileName),
                "StoredCandidate should get properly deleted"
            );
        }

        [TestMethod]
        public async Task StoredDataIsOverwritable()
        {
            Assert.IsNull(
                await this.credentialProvider.GetRawKeyAsync(this.storedCandidate.FileName),
                "StoredCandidate should not be stored yet"
            );

            byte[] overwriteData = Enumerable.Range(0, 256).Reverse().Select(i => (byte)i).ToArray();
            IBuffer overwriteBuffer = CryptographicBuffer.CreateFromByteArray(overwriteData);

            Assert.IsTrue(await this.credentialProvider.TryStoreRawKeyAsync(this.storedCandidate.FileName, this.mockPasswordBuffer));
            Assert.IsTrue(
                await this.credentialProvider.TryStoreRawKeyAsync(this.storedCandidate.FileName, overwriteBuffer),
                "Storing data twice for a database should be fine"
            );

            IBuffer retrievedData = await this.credentialProvider.GetRawKeyAsync(this.storedCandidate.FileName);
            Assert.IsNotNull(retrievedData);
            Assert.IsTrue(
                CryptographicBuffer.Compare(overwriteBuffer, retrievedData),
                "The retrieved data should be what was stored most recently for the database"
            );
        }

        [TestMethod]
        public async Task ToTheLimit()
        {
            for (int i = 0; i < MaxStores; i++)
            {
                IDatabaseCandidate candidate = new MockDatabaseCandidate
                {
                    FileName = $"{i,10:D4}.kdbx"
                };

                Assert.IsTrue(
                    await this.credentialProvider.TryStoreRawKeyAsync(candidate.FileName, this.mockPasswordBuffer),
                    "Storing up to the credential limit should be fine"
                );
            }

            IReadOnlyCollection<string> allCredsEnum = await this.credentialProvider.GetAllEntriesAsync();
            IList<string> allCreds = allCredsEnum.OrderBy(s => s).ToList();
            Assert.AreEqual(MaxStores, allCreds.Count, $"{MaxStores} credentials should have been stored");
            for (int i = 0; i < allCreds.Count; i++)
            {
                Assert.AreEqual($"{i,10:D4}.kdbx", allCreds[i]);
            }

            IDatabaseCandidate finalCandidate = new MockDatabaseCandidate
            {
                FileName = $"{MaxStores}.kdbx"
            };
            Assert.IsFalse(
                await this.credentialProvider.TryStoreRawKeyAsync(finalCandidate.FileName, this.mockPasswordBuffer),
                "Storing past the credential limit should fail"
            );
        }
    }
}
