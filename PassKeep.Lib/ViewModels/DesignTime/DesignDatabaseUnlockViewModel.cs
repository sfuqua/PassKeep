// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
            CandidateFile = new MockDatabaseCandidate
            {
                FileName = "My Database.kdbx",
                LastModified = new DateTimeOffset(DateTime.UtcNow),
                Size = 12345,
            };

            Password = "some password";

            UnlockCommand = new AsyncActionCommand(
                () => HasGoodHeader,
                () => Task.CompletedTask
            );

            UseSavedCredentialsCommand = new AsyncActionCommand(
                () => true,
                () => Task.CompletedTask
            );

            HasGoodHeader = true;
            ParseResult = new ReaderResult(KdbxParserCode.Success);
            RememberDatabase = true;
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

        public bool EligibleForAppControl
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

        public event EventHandler HeaderValidated
        {
            add { }
            remove { }
        }
        
        public event EventHandler<DocumentReadyEventArgs> DocumentReady
        {
            add { }
            remove { }
        }

        public event EventHandler<CredentialStorageFailureEventArgs> CredentialStorageFailed
        {
            add { }
            remove { }
        }

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

        public bool ForbidTogglingRememberDatabase
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

        public bool CacheDatabase
        {
            get;
            set;
        }

        public Task UpdateCandidateFileAsync(IDatabaseCandidate newCandidate)
        {
            throw new NotImplementedException();
        }

        public Task UseAppControlledDatabaseAsync()
        {
            throw new NotImplementedException();
        }
    }
}
