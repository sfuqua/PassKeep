using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Models.DesignTime;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;
using Windows.Storage;

namespace PassKeep.Lib.ViewModels.DesignTime
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

            this.UnlockCommand = new AsyncActionCommand(
                () => this.HasGoodHeader,
                () => Task.CompletedTask
            );

            this.UseSavedCredentialsCommand = new AsyncActionCommand(
                () => true,
                () => Task.CompletedTask
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

        public ITestableFile KeyFile
        {
            get;
            set;
        }

        public AsyncActionCommand UnlockCommand
        {
            get;
            private set;
        }

        public event EventHandler HeaderValidated;
        
        public event EventHandler<DocumentReadyEventArgs> DocumentReady;

        public event EventHandler<CredentialStorageFailureEventArgs> CredentialStorageFailed;

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

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool ForbidRememberingDatabase
        {
            get
            {
                return false;
            }
        }

        public AsyncActionCommand UseSavedCredentialsCommand
        {
            get;
            private set;
        }

        public bool HasSavedCredentials
        {
            get
            {
                return true;
            }
        }

        public bool SaveCredentials
        {
            get;
            set;
        }
        
        public UserConsentVerifierAvailability IdentityVerifiability
        {
            get { return UserConsentVerifierAvailability.Available; }
        }

        public Task UpdateCandidateFileAsync(IDatabaseCandidate newCandidate)
        {
            throw new NotImplementedException();
        }
    }
}
