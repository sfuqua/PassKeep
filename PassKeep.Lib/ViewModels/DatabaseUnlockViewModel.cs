// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Files;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;
using Windows.Storage.Streams;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents a View over a locked document, with the potential for unlocking.
    /// </summary>
    public sealed class DatabaseUnlockViewModel : AbstractViewModel, IDatabaseUnlockViewModel
    {
        private readonly object syncRoot = new object();

        // This is a task that completes when asynchronous activity started in the 
        // constructor has completed.
        private readonly Task initialConstruction;

        private readonly IDatabaseAccessList futureAccessList;
        private readonly IKdbxReader kdbxReader;
        private readonly IFileProxyProvider proxyProvider;
        private readonly IDatabaseCandidateFactory candidateFactory;
        private readonly ITaskNotificationService taskNotificationService;
        private readonly IIdentityVerificationService identityService;
        private readonly ICredentialStorageProvider credentialProvider;
        private readonly ISavedCredentialsViewModelFactory credentialViewModelFactory;

        // Backing fields
        private IDatabaseCandidate _candidateFile;
        private bool _isSampleFile;
        private bool _isReadOnly;
        private bool _cacheDatabase;
        private bool _rememberDatabase;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="file">The candidate document file.</param>
        /// <param name="isSampleFile">Whether the file is a PassKeep sample.</param>
        /// <param name="futureAccessList">A database access list for persisting permission to the database.</param>
        /// <param name="reader">The IKdbxReader implementation used for parsing document files.</param>
        /// <param name="proxyProvider">Generates file proxies that the app controls.</param>
        /// <param name="candidateFactory">Factory used to generate new candidate files as needed.</param>
        /// <param name="taskNotificationService">A service used to notify the UI of blocking operations.</param>
        /// <param name="identityService">The service used to verify the user's consent for saving credentials.</param>
        /// <param name="credentialProvider">The provider used to store/load saved credentials.</param>
        /// <param name="credentialViewModelFactory">A factory used to generate <see cref="ISavedCredentialsViewModel"/> instances.</param>
        public DatabaseUnlockViewModel(
            IDatabaseCandidate file,
            bool isSampleFile,
            IDatabaseAccessList futureAccessList,
            IKdbxReader reader,
            IFileProxyProvider proxyProvider,
            IDatabaseCandidateFactory candidateFactory,
            ITaskNotificationService taskNotificationService,
            IIdentityVerificationService identityService,
            ICredentialStorageProvider credentialProvider,
            ISavedCredentialsViewModelFactory credentialViewModelFactory
        )
        {
            this.futureAccessList = futureAccessList;
            this.kdbxReader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.proxyProvider = proxyProvider ?? throw new ArgumentNullException(nameof(proxyProvider));
            this.candidateFactory = candidateFactory ?? throw new ArgumentNullException(nameof(candidateFactory));
            this.taskNotificationService = taskNotificationService ?? throw new ArgumentNullException(nameof(taskNotificationService));
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            this.credentialViewModelFactory = credentialViewModelFactory ?? throw new ArgumentNullException(nameof(credentialViewModelFactory));

            SaveCredentials = false;
            IdentityVerifiability = UserConsentVerifierAvailability.Available;
            UnlockCommand = new AsyncActionCommand(CanUnlock, DoUnlockAsync);
            UseSavedCredentialsCommand = new AsyncActionCommand(
                () => UnlockCommand.CanExecute(null) && HasSavedCredentials,
                DoUnlockWithSavedCredentials
            );
            IsSampleFile = isSampleFile;
            RememberDatabase = true;
            
            this.initialConstruction = UpdateCandidateFileAsync(file);
        }

        /// <summary>
        /// Event that indicates an attempt to read the header has finished with either a positive or negative result.
        /// </summary>
        public event EventHandler HeaderValidated;
        private void RaiseHeaderValidated()
        {
            HeaderValidated?.Invoke(this, new EventArgs());
        }
        
        /// <summary>
        /// Event that indicates an unlock attempt has stopped (successfully or unsuccessfully).
        /// </summary>
        public event EventHandler StoppedUnlocking;
        private void RaiseStoppedUnlocking()
        {
            StoppedUnlocking?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event that indicates a decrypted document is ready for consumtpion.
        /// </summary>
        public event EventHandler<DocumentReadyEventArgs> DocumentReady;
        private void RaiseDocumentReady(KdbxDocument document, IDatabaseCandidate candidate)
        {
            DebugHelper.Assert(HasGoodHeader);
            if (!HasGoodHeader)
            {
                throw new InvalidOperationException("Document cannot be ready, because the KdbxReader does not have good HeaderData.");
            }

            DocumentReady?.Invoke(this, new DocumentReadyEventArgs(document, candidate, this.kdbxReader.GetWriter(), this.kdbxReader.HeaderData.GenerateRng()));
        }

        /// <summary>
        /// Event that indicates a stored credential could not be added because the provider was full.
        /// </summary>
        public event EventHandler<CredentialStorageFailureEventArgs> CredentialStorageFailed;

        /// <summary>
        /// A lockable object for thread synchronization.
        /// </summary>
        public object SyncRoot
        {
            get { return this.syncRoot;  }
        }

        /// <summary>
        /// The candidate potentially representing the locked document.
        /// </summary>
        public IDatabaseCandidate CandidateFile
        {
            get
            {
                return this._candidateFile;
            }
        }

        /// <summary>
        /// Whether the storage item represented by <see cref="CandidateFile"/>
        /// is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return this._isReadOnly;
            }
            private set
            {
                TrySetProperty(ref this._isReadOnly, value);
            }
        }

        /// <summary>
        /// Whether or not this document is the PassKeep sample document.
        /// </summary>
        public bool IsSampleFile
        {
            get { return this._isSampleFile; }
            private set
            {
                if (TrySetProperty(ref this._isSampleFile, value))
                {
                    OnPropertyChanged(nameof(ForbidTogglingRememberDatabase));
                    OnPropertyChanged(nameof(EligibleForAppControl));
                }
            }
        }

        /// <summary>
        /// Whether this document is eligible to prompt the user for a local
        /// writable copy.
        /// </summary>
        public bool EligibleForAppControl
        {
            get
            {
                return !IsSampleFile && CandidateFile?.IsAppOwned != true && HasGoodHeader; 
            }
        }

        private string _password;
        /// <summary>
        /// The password used to unlock the document. Nulls are converted to the empty string.
        /// </summary>
        public string Password
        {
            get
            {
                return this._password ?? String.Empty;
            }
            set
            {
                TrySetProperty(ref this._password, value ?? String.Empty);
            }
        }

        private ITestableFile _keyFile;
        /// <summary>
        /// The key file used to unlock the document.
        /// </summary>
        public ITestableFile KeyFile
        {
            get
            {
                return this._keyFile;
            }
            set
            {
                TrySetProperty(ref this._keyFile, value);
            }
        }

        /// <summary>
        /// Whether to not allow changing the value of <see cref="RememberDatabase"/>.
        /// </summary>
        public bool ForbidTogglingRememberDatabase
        {
            get
            {
                return IsSampleFile || CandidateFile?.CannotRememberText != null || CacheDatabase;
            }
        }

        /// <summary>
        /// Whether to cache a copy of the database upon unlocking.
        /// </summary>
        public bool CacheDatabase
        {
            get
            {
                return this._cacheDatabase;
            }
            set
            {
                if (TrySetProperty(ref this._cacheDatabase, value))
                {
                    OnPropertyChanged(nameof(ForbidTogglingRememberDatabase));

                    // When caching is chosen, the database is always remembered
                    if (value)
                    {
                        RememberDatabase = true;
                    }
                }
            }
        }

        /// <summary>
        /// Whether to remember the database on successful unlock for future access.
        /// Always false for a sample database.
        /// </summary>
        public bool RememberDatabase
        {
            get
            {
                return (!CacheDatabase && ForbidTogglingRememberDatabase) ? false : this._rememberDatabase;
            }
            set
            {
                TrySetProperty(ref this._rememberDatabase, value);
            }
        }

        /// <summary>
        /// An access list for remembering permission to databases for the future.
        /// </summary>
        public IDatabaseAccessList FutureAccessList
        {
            get
            {
                return this.futureAccessList;
            }
        }

        private AsyncActionCommand _unlockCommand;
        /// <summary>
        /// ActionCommand used to attempt a document unlock using the provided credentials.
        /// </summary>
        public AsyncActionCommand UnlockCommand
        {
            get
            {
                return this._unlockCommand;
            }
            private set
            {
                TrySetProperty(ref this._unlockCommand, value);
            }
        }

        private AsyncActionCommand _useSavedCredentialsCommand;
        /// <summary>
        /// Loads saved credentials from storage and then performs the same work as
        /// <see cref="UnlockCommand"/>.
        /// </summary>
        public AsyncActionCommand UseSavedCredentialsCommand
        {
            get
            {
                return this._useSavedCredentialsCommand;
            }
            private set
            {
                TrySetProperty(ref this._useSavedCredentialsCommand, value);
            }
        }

        /// <summary>
        /// Whether the cleartext header of the candidate file is valid.
        /// </summary>
        public bool HasGoodHeader
        {
            get
            {
                return this.kdbxReader?.HeaderData != null;
            }
        }

        private ReaderResult _parseResult;
        /// <summary>
        /// The result of the last parse operation (either header validation or decryption).
        /// </summary>
        public ReaderResult ParseResult
        {
            get
            {
                return this._parseResult;
            }
            private set
            {
                lock (SyncRoot)
                {
                    if (TrySetProperty(ref this._parseResult, value))
                    {
                        OnPropertyChanged(nameof(HasGoodHeader));
                        OnPropertyChanged(nameof(EligibleForAppControl));
                    }
                }
            }
        }

        private bool _hasSavedCredentials;
        /// <summary>
        /// Whether this database has saved credentials that can be auto-populated.
        /// </summary>
        public bool HasSavedCredentials
        {
            get
            {
                return this._hasSavedCredentials;
            }
            private set
            {
                if (TrySetProperty(ref this._hasSavedCredentials, value))
                {
                    UseSavedCredentialsCommand.RaiseCanExecuteChanged();
                    if (value && IdentityVerifiability == UserConsentVerifierAvailability.Available)
                    {
                        // If we have saved credentials and the identity verifier is available,
                        // default SaveCredentials to true
                        SaveCredentials = true;
                    }
                }
            }
        }

        private bool _saveCredentials;
        /// <summary>
        /// Whether to save this database's credentials on a successful decryption.
        /// </summary>
        public bool SaveCredentials
        {
            get
            {
                return this._saveCredentials;
            }
            set
            {
                TrySetProperty(ref this._saveCredentials, value);
            }
        }

        private UserConsentVerifierAvailability _identityVerifiability;
        /// <summary>
        /// The status of the user identity verification service. If the
        /// service is unavailable, <see cref="SaveCredentials"/> should be false.
        /// </summary>
        public UserConsentVerifierAvailability IdentityVerifiability
        {
            get
            {
                return this._identityVerifiability;
            }
            private set
            {
                if (TrySetProperty(ref this._identityVerifiability, value))
                {
                    if (value == UserConsentVerifierAvailability.Available && HasSavedCredentials)
                    {
                        // If we determine the consent verifier is available and we have
                        // credentials for this database, default SaveCredentials to true.
                        SaveCredentials = true;
                    }
                    else if (value != UserConsentVerifierAvailability.Available)
                    {
                        SaveCredentials = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the initial value of <see cref="IdentityVerifiability"/>.
        /// </summary>
        /// <returns></returns>
        public override async Task ActivateAsync()
        {
            await this.initialConstruction;
            await base.ActivateAsync();
            IdentityVerifiability = await this.identityService.CheckVerifierAvailabilityAsync();
        }

        /// <summary>
        /// Generates a new copy of <see cref="CandidateFile"/> that exists in a path controlled
        /// by the app. 
        /// </summary>
        /// <returns>A task that completes when the candidate swp is completed.</returns>
        public async Task UseAppControlledDatabaseAsync()
        {
            if (!EligibleForAppControl)
            {
                throw new InvalidOperationException("Cannot generate an app-controlled database if not eligible");
            }

            IDatabaseCandidate newCandidate = await GetCachedCandidateAsync();
            DebugHelper.Assert(newCandidate.File != CandidateFile);

            await UpdateCandidateFileAsync(newCandidate);
        }

        /// <summary>
        /// Updates the ViewModel with a new candidate file, which kicks off
        /// a new header validation and stored credential check.
        /// </summary>
        /// <param name="newCandidate">The new database candidate.</param>
        /// <returns>A task that completes when the candidate is updated.</returns>
        public async Task UpdateCandidateFileAsync(IDatabaseCandidate newCandidate)
        {
            IDatabaseCandidate oldCandidate = this._candidateFile;
            if (newCandidate != oldCandidate)
            {
                this._candidateFile = newCandidate;
                OnPropertyChanged(nameof(CandidateFile));
                if (newCandidate == null)
                {
                    CacheDatabase = false;
                }
                else if (newCandidate.IsAppOwned)
                {
                    CacheDatabase = true;
                }

                OnPropertyChanged(nameof(EligibleForAppControl));

                // Clear the keyfile for the old selection
                KeyFile = null;

                if (newCandidate != null)
                {
                    // Evaluate whether the new candidate is read-only
                    Task<bool> checkWritable = newCandidate.File?.AsIStorageFile.CheckWritableAsync();
                    checkWritable = checkWritable ?? Task.FromResult(false);

                    TaskScheduler syncContextScheduler;
                    if (SynchronizationContext.Current != null)
                    {
                        syncContextScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                    }
                    else
                    {
                        // If there is no SyncContext for this thread (e.g. we are in a unit test
                        // or console scenario instead of running in an app), then just use the
                        // default scheduler because there is no UI thread to sync with.
                        syncContextScheduler = TaskScheduler.Current;
                    }

                    Task fileAccessUpdate = checkWritable.ContinueWith(
                        async (task) =>
                        {
                            IsReadOnly = !task.Result;
                            await ValidateHeader();
                        },
                        syncContextScheduler
                    );

                    // Evaluate whether we have saved credentials for this database
                    Task hasCredentialsUpdate = this.credentialProvider.GetRawKeyAsync(newCandidate.File)
                        .ContinueWith(
                            (task) =>
                            {
                                HasSavedCredentials = task.Result != null;
                            },
                            syncContextScheduler
                        );


                    await Task.WhenAll(fileAccessUpdate, hasCredentialsUpdate);
                }
                else
                {
                    IsReadOnly = false;
                    HasSavedCredentials = false;
                }

                ParseResult = null;
                OnPropertyChanged(nameof(ForbidTogglingRememberDatabase));
            }
        }

        /// <summary>
        /// Parses the header of the CandidateFile and handles updating status on the View.
        /// </summary>
        private async Task ValidateHeader()
        {
            DebugHelper.Assert(this.kdbxReader != null);
            if (this.kdbxReader == null)
            {
                throw new InvalidOperationException("Cannot validate KDBX header if there is no reader instance");
            }

            try
            {
                if (CandidateFile == null)
                {
                    ParseResult = null;
                }
                else
                {
                    using (IRandomAccessStream fileStream = await CandidateFile.GetRandomReadAccessStreamAsync())
                    {
                        CancellationTokenSource cts = new CancellationTokenSource(5000);
                        ParseResult = await this.kdbxReader.ReadHeaderAsync(fileStream, cts.Token);
                    }
                }
            }
            catch(COMException)
            {
                // In the Windows 8.1 preview, opening a stream to a SkyDrive file can fail with no workaround.
                ParseResult = new ReaderResult(KdbxParserCode.UnableToReadFile);
            }
            finally
            {
                RaiseHeaderValidated();
                UnlockCommand.RaiseCanExecuteChanged();
                UseSavedCredentialsCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// CanExecute callback for the UnlockCommand - determines whether the document file is unlockable.
        /// </summary>
        /// <returns>Whether the document file can be unlocked in the current state.</returns>
        private bool CanUnlock()
        {
            // Verify all the appropriate data exists and the last parse event was successful.
            return CandidateFile != null && HasGoodHeader;
        }

        /// <summary>
        /// Execution action for the UnlockCommand - attempts to unlock the document file
        /// with the ViewModel's credentials.
        /// </summary>
        private Task DoUnlockAsync()
        {
            return DoUnlockAsync(null);
        }

        /// <summary>
        /// Attempts to unlock the document file.
        /// </summary>
        /// <param name="storedCredential">The key to use for decryption - if null, the ViewModel's
        /// credentials are used instead.</param>
        private async Task DoUnlockAsync(IBuffer storedCredential)
        {
            DebugHelper.Assert(CanUnlock());
            if (!CanUnlock())
            {
                throw new InvalidOperationException("The ViewModel is not in a state that can unlock the database!");
            }

            CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                using (IRandomAccessStream stream = await CandidateFile.GetRandomReadAccessStreamAsync())
                {
                    Task<KdbxDecryptionResult> decryptionTask;
                    if (storedCredential != null)
                    {
                        decryptionTask = this.kdbxReader.DecryptFile(stream, storedCredential, cts.Token);
                    }
                    else
                    {
                        decryptionTask = this.kdbxReader.DecryptFileAsync(stream, Password, KeyFile, cts.Token);
                    }

                    if (this.taskNotificationService.CurrentTask == null || this.taskNotificationService.CurrentTask.IsCompleted)
                    {
                        this.taskNotificationService.PushOperation(decryptionTask, cts, AsyncOperationType.DatabaseDecryption);
                    }
                    KdbxDecryptionResult result = await decryptionTask;

                    ParseResult = result.Result;

                    DebugHelper.Trace($"Got ParseResult from database unlock attempt: {ParseResult}");
                    if (!ParseResult.IsError)
                    {
                        // The database candidate to proceed into the next stage with
                        IDatabaseCandidate candidateToUse = CandidateFile;

                        if (CacheDatabase)
                        {
                            // We do not use UseAppControlledDatabaseAsync here because it has extra baggage.
                            // We don't need to refresh the view at this stage, just fire an event using
                            // the cached file.
                            candidateToUse = await GetCachedCandidateAsync();
                        }
                        if (RememberDatabase)
                        {
                            string accessToken = this.futureAccessList.Add(candidateToUse.File, candidateToUse.FileName);
                            DebugHelper.Trace($"Unlock was successful and database was remembered with token: {accessToken}");
                        }
                        else
                        {
                            DebugHelper.Trace("Unlock was successful but user opted not to remember the database.");
                        }

                        if (SaveCredentials)
                        {
                            bool storeCredential = false;

                            // If we were not already using a stored credential, we need user
                            // consent to continue.
                            if (storedCredential == null)
                            {
                                Task<bool> identityTask = this.identityService.VerifyIdentityAsync();
                                if (this.taskNotificationService.CurrentTask == null || this.taskNotificationService.CurrentTask.IsCompleted)
                                {
                                    this.taskNotificationService.PushOperation(identityTask, AsyncOperationType.IdentityVerification);
                                }

                                storeCredential = await identityTask;
                                storedCredential = result.GetRawKey();
                            }
                            else
                            {
                                // If we have a stored credential, we already got consent.
                                storeCredential = true;
                            }

                            if (storeCredential)
                            {
                                if (!await this.credentialProvider.TryStoreRawKeyAsync(candidateToUse.File, storedCredential))
                                {
                                    EventHandler<CredentialStorageFailureEventArgs> handler = CredentialStorageFailed;
                                    if (handler != null)
                                    {
                                        // If we could not store a credential, give the View a chance to try again.
                                        CredentialStorageFailureEventArgs eventArgs =
                                            new CredentialStorageFailureEventArgs(
                                                this.credentialProvider,
                                                this.credentialViewModelFactory,
                                                candidateToUse,
                                                storedCredential
                                            );

                                        handler(this, eventArgs);
                                        await eventArgs.DeferAsync();
                                    }
                                }
                            }
                        }

                        RaiseDocumentReady(result.GetDocument(), candidateToUse);
                    }
                }
            }
            catch (COMException)
            {
                // In the Windows 8.1 preview, opening a stream to a SkyDrive file can fail with no workaround.
                ParseResult = new ReaderResult(KdbxParserCode.UnableToReadFile);
            }
        }

        /// <summary>
        /// Execution action for the UseSavedCredentials - attempts to unlock the document file
        /// using stored credentials after verifying the user's identity.
        /// </summary>
        private async Task DoUnlockWithSavedCredentials()
        {
            Task<bool> verificationTask = this.identityService.VerifyIdentityAsync();
            this.taskNotificationService.PushOperation(verificationTask, AsyncOperationType.IdentityVerification);

            if (!await verificationTask)
            {
                ParseResult = new ReaderResult(KdbxParserCode.CouldNotVerifyIdentity);
                return;
            }

            Task<IBuffer> credentialTask = this.credentialProvider.GetRawKeyAsync(CandidateFile.File);
            this.taskNotificationService.PushOperation(credentialTask, AsyncOperationType.CredentialVaultAccess);

            IBuffer storedCredential = await credentialTask;
            if (storedCredential == null)
            {
                ParseResult = new ReaderResult(KdbxParserCode.CouldNotRetrieveCredentials);
                return;
            }
            
            Task unlockTask = DoUnlockAsync(storedCredential);
            this.taskNotificationService.PushOperation(unlockTask, AsyncOperationType.DatabaseDecryption);

            await unlockTask;
        }

        /// <summary>
        /// Helper to generate a new, cached candidate file using the underlying proxy provider.
        /// </summary>
        /// <returns>A cached, local database candidate.</returns>
        private async Task<IDatabaseCandidate> GetCachedCandidateAsync()
        {
            ITestableFile newCandidateFile = await this.proxyProvider.CreateWritableProxyAsync(CandidateFile.File);
            IDatabaseCandidate newCandidate = await this.candidateFactory.AssembleAsync(newCandidateFile);
            DebugHelper.Assert(newCandidateFile != CandidateFile);

            return newCandidate;
        }
    }
}
