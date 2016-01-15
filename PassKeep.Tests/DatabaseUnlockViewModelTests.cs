using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PassKeep.Contracts.Models;
using PassKeep.Tests.Attributes;
using PassKeep.Tests.Mocks;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.ViewModels;
using PassKeep.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage;

namespace PassKeep.Tests
{
    [TestClass]
    public class DatabaseUnlockViewModelTests : TestClassBase
    {
        private const string KnownBadDatabase = "Bad.txt";
        private const string KnownGoodDatabase = "NotCompressed_Password.kdbx";

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
            MethodInfo testMethod = typeof(DatabaseUnlockViewModelTests).GetRuntimeMethod(
                this.TestContext.TestName, new Type[0]
            );
            var dataAttr = testMethod.GetCustomAttribute<TestDataAttribute>();
            if (dataAttr != null && dataAttr.SkipInitialization)
            {
                return;
            }

            Utils.DatabaseInfo databaseInfo = null;
            IDatabaseCandidate databaseValue = null;
            bool sampleValue = false;

            try
            {
                databaseInfo = await Utils.GetDatabaseInfoForTest(this.TestContext);
            }
            catch (InvalidOperationException) { }

            if (databaseInfo != null)
            {
                databaseValue = new StorageFileDatabaseCandidate(databaseInfo.Database);
                sampleValue = (dataAttr != null && dataAttr.InitSample);
            }

            if (dataAttr != null && !dataAttr.InitDatabase)
            {
                databaseValue = null;
            }

            this.accessList = new MockStorageItemAccessList();
            this.viewModel = new DatabaseUnlockViewModel(databaseValue, sampleValue, this.accessList, new KdbxReader());

            // Set various ViewModel properties if desired
            if (databaseInfo != null && dataAttr != null)
            {
                if (dataAttr.SetPassword)
                {
                    this.viewModel.Password = databaseInfo.Password;
                }

                if (dataAttr.SetKeyFile)
                {
                    this.viewModel.KeyFile = databaseInfo.Keyfile;
                }
            }

            if (databaseValue != null)
            {
                await this.ViewModelHeaderValidated();
            }
        }

        [TestMethod, TestData(initDatabase: false)]
        public void DatabaseUnlockViewModel_AllowsNullFile()
        {
            Assert.IsNull(this.viewModel.CandidateFile, "ViewModel.CandidateFile should have been initialized to null");
            Assert.IsFalse(this.viewModel.HasGoodHeader, "ViewModel.HasGoodHeader should default to false");
            Assert.IsNull(this.viewModel.ParseResult, "ViewModel.CandidateFile should have no parse results yet");
        }

        [TestMethod, TestData(skipInitialization: true)]
        public void DatabaseUnlockViewModel_ThrowsOnNullReader()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DatabaseUnlockViewModel(null, false, null, null));
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
        public void DatabaseUnlockViewModel_BadHeader()
        {
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

            this.viewModel.CandidateFile = new StorageFileDatabaseCandidate(await Utils.GetDatabaseByName(KnownBadDatabase));
            await ViewModelHeaderValidated();
            DatabaseUnlockViewModel_BadHeader();

            this.viewModel.CandidateFile =  new StorageFileDatabaseCandidate(await Utils.GetDatabaseByName(KnownGoodDatabase));
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
