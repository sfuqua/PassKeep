using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Providers;
using PassKeep.Lib.Services;
using PassKeep.Lib.ViewModels;
using PassKeep.Tests.Attributes;
using PassKeep.Tests.Mocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace PassKeep.Tests
{
    [TestClass]
    public class DatabaseUnlockViewModelTests : TestClassBase
    {
        private const string KnownBadDatabase = "Bad.txt";
        private const string KnownGoodDatabase = "NotCompressed_Password.kdbx";

        private Utils.DatabaseInfo testDatabaseInfo;
        private IDatabaseCandidate alwaysStoredCandidate;
        private MockIdentityVerifier identityService;
        private ICredentialStorageProvider credentialProvider;
        private IDatabaseAccessList accessList;
        private IDatabaseUnlockViewModel viewModel;

        public override TestContext TestContext
        {
            get;
            set;
        }

        [TestInitialize]
        public async Task Initialize()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            
            var dataAttr = GetTestAttribute<TestDataAttribute>();
            if (dataAttr != null && dataAttr.SkipInitialization)
            {
                return;
            }

            this.testDatabaseInfo = null;
            IDatabaseCandidate databaseValue = null;
            bool sampleValue = false;

            try
            {
                this.testDatabaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
            }
            catch (InvalidOperationException) { }

            if (this.testDatabaseInfo != null)
            {
                databaseValue = await new StorageFileDatabaseCandidateFactory().AssembleAsync(this.testDatabaseInfo.Database);
                sampleValue = (dataAttr != null && dataAttr.InitSample);
            }

            if (dataAttr != null && !dataAttr.InitDatabase)
            {
                databaseValue = null;
            }

            this.accessList = new MockStorageItemAccessList();

            this.identityService = new MockIdentityVerifier();
            this.identityService.CanVerify = dataAttr?.IdentityVerifierAvailable ?? UserConsentVerifierAvailability.NotConfiguredForUser;
            this.identityService.Verified = dataAttr?.IdentityVerified ?? false;

            this.credentialProvider = new MockCredentialProvider();

            if (dataAttr?.StoredCredentials == true && databaseValue != null && this.testDatabaseInfo != null)
            {
                Assert.IsTrue(
                    await this.credentialProvider.TryStoreRawKeyAsync(
                        databaseValue,
                        this.testDatabaseInfo.RawKey
                    )
                );
            }

            Utils.DatabaseInfo backupDatabase = await Utils.DatabaseMap["StructureTesting"];
            this.alwaysStoredCandidate = await new StorageFileDatabaseCandidateFactory().AssembleAsync(
                backupDatabase.Database
            );
            Assert.IsTrue(
                await this.credentialProvider.TryStoreRawKeyAsync(
                    this.alwaysStoredCandidate,
                    backupDatabase.RawKey
                )
            );

            this.viewModel = new DatabaseUnlockViewModel(
                databaseValue,
                sampleValue,
                this.accessList,
                new KdbxReader(),
                new TaskNotificationService(),
                this.identityService,
                this.credentialProvider,
                new MockSyncContext()
            );

            await this.viewModel.ActivateAsync();

            // Set various ViewModel properties if desired
            if (this.testDatabaseInfo != null && dataAttr != null)
            {
                if (dataAttr.SetPassword)
                {
                    this.viewModel.Password = this.testDatabaseInfo.Password;
                }

                if (dataAttr.SetKeyFile)
                {
                    this.viewModel.KeyFile = this.testDatabaseInfo.Keyfile;
                }
            }

            if (databaseValue != null)
            {
                await this.ViewModelHeaderValidated();
            }
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await this.credentialProvider.ClearAsync();
        }

        [TestMethod, TestData(initDatabase: false)]
        public void DatabaseUnlockViewModel_AllowsNullFile()
        {
            Assert.IsNull(this.viewModel.CandidateFile, "ViewModel.CandidateFile should have been initialized to null");
            Assert.IsFalse(this.viewModel.HasGoodHeader, "ViewModel.HasGoodHeader should default to false");
            Assert.IsNull(this.viewModel.ParseResult, "ViewModel.CandidateFile should have no parse results yet");
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase)]
        public void DatabaseUnlockViewModel_SampleDefault()
        {
            Assert.IsFalse(this.viewModel.IsSampleFile, "ViewModel.IsSampleFile should default to false");
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(initSample: true)]
        public void DatabaseUnlockViewModel_SampleTrue()
        {
            Assert.IsTrue(this.viewModel.IsSampleFile, "ViewModel.IsSampleFile should initialize true when specified");
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase, false, Password = "temp"), TestData(setPassword: true)]
        public void DatabaseUnlockViewModel_ChangePassword()
        {
            Assert.AreEqual("temp", this.viewModel.Password, "ViewModel.Password should reflect string values");

            this.viewModel.Password = null;
            Assert.AreEqual(String.Empty, this.viewModel.Password, "A null password should be coerced into an empty string");
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase, false, KeyFileName = "CustomKeyFile.key"), TestData(setKeyFile: true)]
        public void DatabaseUnlockViewModel_ChangeKeyFile()
        {
            Assert.IsNotNull(this.viewModel.KeyFile, "ViewModel.KeyFile should be mutable");

            this.viewModel.KeyFile = null;
            Assert.IsNull(this.viewModel.KeyFile, "ViewModel.KeyFile should be nullable");
        }

        [TestMethod, DatabaseInfo(KnownBadDatabase, false)]
        public async Task DatabaseUnlockViewModel_BadHeader()
        {
            await ViewModelHeaderValidated();

            Assert.IsFalse(
                this.viewModel.HasGoodHeader,
                "Setting a CandidateFile with a known bad header should result in HasGoodHeader == false"
            );

            Assert.IsNotNull(
                this.viewModel.ParseResult,
                "Setting a CandidateFile with a known bad header should populate the ParseResult property"
            );

            Assert.AreEqual(
                KdbxParserCode.SignatureInvalid,
                this.viewModel.ParseResult.Code,
                "The ParseResult code should reflect the value returned by the KdbxReader"
            );

            Assert.IsFalse(this.viewModel.UnlockCommand.CanExecute(null), "The UnlockCommand should not be ready to execute");
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase)]
        public void DatabaseUnlockViewModel_GoodHeader()
        {
            Assert.IsTrue(
                this.viewModel.HasGoodHeader,
                "Setting a CandidateFile with a known good header should result in HasGoodHeader == true"
            );

            Assert.IsNotNull(
                this.viewModel.ParseResult,
                "Setting a CandidateFile with a known good header should populate the ParseResult property"
            );

            Assert.AreEqual(
                KdbxParserCode.Success,
                this.viewModel.ParseResult.Code,
                "The ParseResult code should reflect the value returned by the KdbxReader"
            );

            Assert.IsTrue(this.viewModel.UnlockCommand.CanExecute(null), "The UnlockCommand should be ready to execute");
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase)]
        public async Task DatabaseUnlockViewModel_ChangeFiles()
        {
            DatabaseUnlockViewModel_GoodHeader();

            StorageFileDatabaseCandidateFactory factory = new StorageFileDatabaseCandidateFactory();

            await this.viewModel.UpdateCandidateFileAsync(await factory.AssembleAsync(await Utils.GetDatabaseByName(KnownBadDatabase)));
            await ViewModelHeaderValidated();
            DatabaseUnlockViewModel_BadHeader();

            await this.viewModel.UpdateCandidateFileAsync(await factory.AssembleAsync(await Utils.GetDatabaseByName(KnownGoodDatabase)));
            await ViewModelHeaderValidated();
            DatabaseUnlockViewModel_GoodHeader();
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(setPassword: true)]
        public async Task DatabaseUnlockViewModel_UnlockSuccess()
        {
            List<string> expectedEvents = new List<string> { "StartedUnlocking", "StoppedUnlocking", "DocumentReady" };
            List<string> actualEvents = new List<string>();

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            Action<string, object, object> handler = (eventName, sender, eventArgs) =>
            {
                ICollection collection = actualEvents;
                lock (collection.SyncRoot)
                {
                    actualEvents.Add(eventName);
                    if (actualEvents.Count == 3)
                    {
                        tcs.SetResult(null);
                    }
                }
            };

            EventHandler<CancellableEventArgs> startedHandler = (sender, eventArgs) =>
            {
                handler("StartedUnlocking", sender, eventArgs);
            };

            EventHandler stoppedHandler = (sender, eventArgs) =>
            {
                handler("StoppedUnlocking", sender, eventArgs);
            };

            EventHandler<DocumentReadyEventArgs> readyHandler = (sender, eventArgs) =>
            {
                Assert.IsNotNull(eventArgs.Document, "XDocument from event should not be null");
                handler("DocumentReady", sender, eventArgs);
            };

            this.viewModel.StartedUnlocking += startedHandler;
            this.viewModel.StoppedUnlocking += stoppedHandler;
            this.viewModel.DocumentReady += readyHandler;

            this.viewModel.UnlockCommand.Execute(null);

            await tcs.Task;

            for (int i = 0; i < expectedEvents.Count; i++)
            {
                Assert.AreEqual(expectedEvents[i], actualEvents[i]);
            }

            Assert.IsFalse(this.viewModel.ParseResult.IsError, "ParseResult should not be an error");
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase, false, Password="bogus"), TestData(setPassword: true)]
        public async Task DatabaseUnlockViewModel_UnlockFailure()
        {
            List<string> expectedEvents = new List<string> { "StartedUnlocking", "StoppedUnlocking" };
            List<string> actualEvents = new List<string>();

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            Action<string, object, object> handler = (eventName, sender, eventArgs) =>
            {
                ICollection collection = actualEvents;
                lock (collection.SyncRoot)
                {
                    actualEvents.Add(eventName);
                    if (actualEvents.Count == 2)
                    {
                        tcs.SetResult(null);
                    }
                }
            };

            EventHandler<CancellableEventArgs> startedHandler = (sender, eventArgs) =>
            {
                handler("StartedUnlocking", sender, eventArgs);
            };

            EventHandler stoppedHandler = (sender, eventArgs) =>
            {
                handler("StoppedUnlocking", sender, eventArgs);
            };

            this.viewModel.StartedUnlocking += startedHandler;
            this.viewModel.StoppedUnlocking += stoppedHandler;

            this.viewModel.UnlockCommand.Execute(null);

            await tcs.Task;

            for (int i = 0; i < expectedEvents.Count; i++)
            {
                Assert.AreEqual(expectedEvents[i], actualEvents[i]);
            }

            Assert.AreEqual(
                KdbxParserCode.CouldNotDecrypt,
                this.viewModel.ParseResult.Code,
                "ParseResult should be correct on a failed decryption"
            );
            Assert.IsTrue(this.viewModel.HasGoodHeader, "Header should still be good after failure");
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(setPassword: true), Timeout(5000)]
        public async Task DatabaseUnlockViewModel_RememberOnSuccessIfDesired()
        {
            Assert.AreEqual(0, this.accessList.Entries.Count);
            this.viewModel.RememberDatabase = true;

            await ViewModelHeaderValidated();
            Assert.AreEqual(0, this.accessList.Entries.Count);
            await ViewModelDecrypted();

            Assert.AreEqual(1, this.accessList.Entries.Count);
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(setPassword: true), Timeout(5000)]
        public async Task DatabaseUnlockViewModel_DoNotRememberOnSuccessIfNotDesired()
        {
            Assert.AreEqual(0, this.accessList.Entries.Count);
            this.viewModel.RememberDatabase = false;

            await ViewModelHeaderValidated();
            await ViewModelDecrypted();

            Assert.AreEqual(0, this.accessList.Entries.Count);
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase, false, KeyFileName = "CustomKeyFile.key"), TestData(setKeyFile: true)]
        public async Task DatabaseUnlockViewModel_ChangeCandidate()
        {
            Assert.IsNotNull(this.viewModel.KeyFile);

            bool propChanged = false;
            this.viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(this.viewModel.KeyFile))
                {
                    propChanged = true;
                }
            };

            await this.viewModel.UpdateCandidateFileAsync(null);

            Assert.IsTrue(propChanged);
            Assert.IsNull(this.viewModel.KeyFile);
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(storedCredentials: true), Timeout(5000)]
        public async Task DatabaseUnlockViewModel_HasStoredCredential()
        {
            await ViewModelHeaderValidated();
            Assert.IsTrue(this.viewModel.HasSavedCredentials);
            Assert.IsTrue(this.viewModel.UseSavedCredentialsCommand.CanExecute(null));
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(storedCredentials: false), Timeout(5000)]
        public async Task DatabaseUnlockViewModel_NoStoredCredential()
        {
            await ViewModelHeaderValidated();
            Assert.IsFalse(this.viewModel.HasSavedCredentials);
            Assert.IsFalse(this.viewModel.UseSavedCredentialsCommand.CanExecute(null));
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(storedCredentials: false), Timeout(5000)]
        public async Task DatabaseUnlockViewModel_SwitchToStoredCredential()
        {
            await DatabaseUnlockViewModel_NoStoredCredential();

            IDatabaseCandidate originalCandidate = this.viewModel.CandidateFile;

            await this.viewModel.UpdateCandidateFileAsync(this.alwaysStoredCandidate);
            await DatabaseUnlockViewModel_HasStoredCredential();

            await this.viewModel.UpdateCandidateFileAsync(originalCandidate);
            await DatabaseUnlockViewModel_NoStoredCredential();
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(storedCredentials: true), Timeout(5000)]
        public async Task DatabaseUnlockViewModel_StoredCredentialNoConsent()
        {
            this.identityService.Verified = false;

            await ViewModelHeaderValidated();
            Assert.IsTrue(this.viewModel.HasSavedCredentials);
            Assert.IsTrue(this.viewModel.UseSavedCredentialsCommand.CanExecute(null));

            await this.viewModel.UseSavedCredentialsCommand.ExecuteAsync(null);
            Assert.AreEqual(KdbxParserCode.CouldNotVerifyIdentity, this.viewModel.ParseResult?.Code);
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(storedCredentials: false), Timeout(5000)]
        public async Task DatabaseUnlockViewModel_StoredCredentialMissing()
        {
            this.identityService.Verified = true;

            await ViewModelHeaderValidated();

            await this.viewModel.UseSavedCredentialsCommand.ExecuteAsync(null);
            Assert.AreEqual(KdbxParserCode.CouldNotRetrieveCredentials, this.viewModel.ParseResult?.Code);
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(storedCredentials: true), Timeout(5000)]
        public async Task DatabaseUnlockViewModel_StoredCredentials()
        {
            this.identityService.Verified = true;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            this.viewModel.DocumentReady += (s, e) =>
            {
                tcs.SetResult(true);
            };

            await ViewModelHeaderValidated();
            await this.viewModel.UseSavedCredentialsCommand.ExecuteAsync(null);
            
            await tcs.Task;
            Assert.AreEqual(KdbxParserCode.Success, this.viewModel.ParseResult?.Code);
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(setPassword: true, storedCredentials: false), Timeout(5000)]
        public async Task DatabaseUnlockViewModel_StoreCredentials()
        {
            Assert.IsNull(await this.credentialProvider.GetRawKeyAsync(this.viewModel.CandidateFile));

            this.identityService.Verified = true;
            this.viewModel.SaveCredentials = true;

            await ViewModelHeaderValidated();
            await ViewModelDecrypted();

            IBuffer storedKey = await this.credentialProvider.GetRawKeyAsync(this.viewModel.CandidateFile);
            Assert.IsNotNull(storedKey);
            Assert.IsTrue(
                CryptographicBuffer.Compare(
                    this.testDatabaseInfo.RawKey,
                    storedKey
                )
            );
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), TestData(setPassword: true, storedCredentials: false), Timeout(50000)]
        public async Task DatabaseUnlockViewModel_DontStoreCredentialsWithoutConsent()
        {
            Assert.IsNull(await this.credentialProvider.GetRawKeyAsync(this.viewModel.CandidateFile));

            this.identityService.Verified = false;
            this.viewModel.SaveCredentials = true;

            await ViewModelHeaderValidated();
            await ViewModelDecrypted();

            IBuffer storedKey = await this.credentialProvider.GetRawKeyAsync(this.viewModel.CandidateFile);
            Assert.IsNull(storedKey);
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), Timeout(5000)]
        [TestData(storedCredentials: true, identityVerifierAvailable: UserConsentVerifierAvailability.Available)]
        public async Task DatabaseUnlockViewModel_SaveCredentialsDefaultsTrueIfStored()
        {
            await ViewModelHeaderValidated();
            Assert.IsTrue(this.viewModel.SaveCredentials);
            Assert.AreEqual(UserConsentVerifierAvailability.Available, this.viewModel.IdentityVerifiability);
        }

        [TestMethod, DatabaseInfo(KnownGoodDatabase), Timeout(5000)]
        [TestData(identityVerifierAvailable: UserConsentVerifierAvailability.DeviceNotPresent)]
        public void DatabaseUnlockViewModel_VerifierNotAvailable()
        {
            Assert.AreEqual(UserConsentVerifierAvailability.DeviceNotPresent, this.viewModel.IdentityVerifiability);
        }

        /// <summary>
        /// An awaitable Task that doesn't complete until the ViewModel has finished initial header validation.
        /// </summary>
        /// <returns>A Task that completes when the ViewModel's Header is done validating.</returns>
        private Task ViewModelHeaderValidated()
        {
            lock (this.viewModel.SyncRoot)
            {
                var tcs = new TaskCompletionSource<object>();
                if (this.viewModel.ParseResult != null)
                {
                    tcs.SetResult(null);
                    return tcs.Task;
                }

                EventHandler eventHandler = null;
                eventHandler = (sender, eventArgs) =>
                {
                    this.viewModel.HeaderValidated -= eventHandler;
                    tcs.SetResult(null);
                };

                this.viewModel.HeaderValidated += eventHandler;
                return tcs.Task;
            }
        }

        private Task ViewModelDecrypted()
        {
            lock (this.viewModel.SyncRoot)
            {
                var tcs = new TaskCompletionSource<object>();

                EventHandler<DocumentReadyEventArgs> eventHandler = null;
                eventHandler = (sender, eventArgs) =>
                {
                    this.viewModel.DocumentReady -= eventHandler;
                    tcs.SetResult(null);
                };

                this.viewModel.DocumentReady += eventHandler;

                this.viewModel.UnlockCommand.Execute(null);

                return tcs.Task;
            }
        }
    }
}
