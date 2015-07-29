using PassKeep.Contracts.Models;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Models.DesignTime;
using SariphLib.Mvvm;
using System;
using Windows.Storage;

namespace PassKeep.ViewModels.DesignTime
{
    /// <summary>
    /// Serves as a bindable design-time ViewModel for the DatabaseUnlockView.
    /// </summary>
    public class DesignDatabaseUnlockViewModel : AbstractViewModel, IDatabaseUnlockViewModel
    {
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Initializes the design data.
        /// </summary>
        public DesignDatabaseUnlockViewModel()
        {
            this.CandidateFile = new MockDatabaseCandidate
            {
                FileName = "My Database.kdbx",
                LastModified = new DateTimeOffset(DateTime.UtcNow),
                Size = 12345,
            };

            this.Password = "some password";

            this.UnlockCommand = new ActionCommand(
                () => this.HasGoodHeader,
                () => { }
            );

            this.HasGoodHeader = true;
            this.ParseResult = new ReaderResult(KdbxParserCode.Success);
            this.RememberDatabase = true;
        }

        public object SyncRoot
        {
            get { return this._syncRoot; }
        }

        public IDatabaseCandidate CandidateFile
        {
            get;
            set;
        }

        public bool IsSampleFile
        {
            get;
            set;
        }

        public string Password
        {
            get;
            set;
        }

        public StorageFile KeyFile
        {
            get;
            set;
        }

        public ActionCommand UnlockCommand
        {
            get;
            private set;
        }

        public event EventHandler HeaderValidated;

        public event EventHandler<CancellableEventArgs> StartedUnlocking;

        public event EventHandler StoppedUnlocking;

        public event EventHandler<DocumentReadyEventArgs> DocumentReady;

        public bool HasGoodHeader
        {
            get;
            set;
        }

        public ReaderResult ParseResult
        {
            get;
            set;
        }


        public bool RememberDatabase
        {
            get;
            set;
        }

        public IDatabaseAccessList FutureAccessList
        {
            get;
            set;
        }
    }
}
