using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Files;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents a View over a locked document, with the potential for unlocking.
    /// </summary>
    public sealed class DatabaseUnlockViewModel : AbstractViewModel, IDatabaseUnlockViewModel
    {
        private readonly object syncRoot = new object();
        private IDatabaseAccessList futureAccessList;
        private IKdbxReader kdbxReader;
        private IIdentityVerificationService identityService;
        private ICredentialStorageProvider credentialProvider;
        private ISyncContext syncContext;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="file">The candidate document file.</param>
        /// <param name="isSampleFile">Whether the file is a PassKeep sample.</param>
        /// <param name="futureAccessList">A database access list for persisting permission to the database.</param>
        /// <param name="reader">The IKdbxReader implementation used for parsing document files.</param>
        /// <param name="identityService">The service used to verify the user's consent for saving credentials.</param>
        /// <param name="credentialProvider">The provider used to store/load saved credentials.</param>
        /// <param name="syncContext">A context used to synchronize multi-threaded operations with the view.</param>
        public DatabaseUnlockViewModel(
            IDatabaseCandidate file,
            bool isSampleFile,
            IDatabaseAccessList futureAccessList,
            IKdbxReader reader,
            IIdentityVerificationService identityService,
            ICredentialStorageProvider credentialProvider,
            ISyncContext syncContext
        )
        {
            Dbg.Assert(reader != null);
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (identityService == null)
            {
                throw new ArgumentNullException(nameof(identityService));
            }
            
            if (credentialProvider == null)
            {
                throw new ArgumentNullException(nameof(credentialProvider));
            }

            if (syncContext == null)
            {
                throw new ArgumentNullException(nameof(syncContext));
            }

            this.futureAccessList = futureAccessList;
            this.kdbxReader = reader;
            this.identityService = identityService;
            this.credentialProvider = credentialProvider;
            this.syncContext = syncContext;
            this.UnlockCommand = new ActionCommand(this.CanUnlock, this.DoUnlock);
            this.UseSavedCredentialsCommand = new ActionCommand(
                () => this.UnlockCommand.CanExecute(null) && this.HasSavedCredentials,
                this.DoUnlockWithSavedCredentials
            );
            this.IsSampleFile = isSampleFile;
            this.RememberDatabase = true;

            this.CandidateFile = file;
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
        /// Event that indicates an unlock attempt has begun.
        /// </summary>
        public event EventHandler<CancellableEventArgs> StartedUnlocking;
        private void RaiseStartedUnlocking(CancellationTokenSource cts)
        {
            CancellableEventArgs eventArgs = new CancellableEventArgs(cts);
            StartedUnlocking?.Invoke(this, eventArgs);
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
        private void RaiseDocumentReady(KdbxDocument document)
        {
            Dbg.Assert(this.HasGoodHeader);
            if (!this.HasGoodHeader)
            {
                throw new InvalidOperationException("Document cannot be ready, because the KdbxReader does not have good HeaderData.");
            }

            DocumentReady?.Invoke(this, new DocumentReadyEventArgs(document, this.kdbxReader.GetWriter(), this.kdbxReader.HeaderData.GenerateRng()));
        }

        /// <summary>
        /// A lockable object for thread synchronization.
        /// </summary>
        public object SyncRoot
        {
            get { return this.syncRoot;  }
        }

        private IDatabaseCandidate _candidateFile;
        /// <summary>
        /// The candidate potentially representing the locked document.
        /// </summary>
        public IDatabaseCandidate CandidateFile
        {
            get
            {
                return this._candidateFile;
            }
            set
            {
                if (TrySetProperty(ref _candidateFile, value))
                {
                    // Clear the keyfile for the old selection
                    this.KeyFile = null;

                    if (value != null)
                    {
                        // Evaluate whether the new candidate is read-only
                        value.StorageItem?.CheckWritableAsync().ContinueWith(
                            (task) =>
                                this.syncContext.Post(() =>
                                {
                                    this.IsReadOnly = !task.Result;
                                    this.ValidateHeader();
                                })
                        );

                        // Evaluate whether we have saved credentials for this database
                        this.credentialProvider.GetRawKeyAsync(value)
                            .ContinueWith(
                                (task) =>
                                    this.syncContext.Post(() =>
                                    {
                                        this.HasSavedCredentials = task.Result != null;
                                    })
                            );
                    }
                    else
                    {
                        this.IsReadOnly = false;
                        this.HasSavedCredentials = false;
                    }

                    this.ParseResult = null;
                    OnPropertyChanged(nameof(ForbidRememberingDatabase));
                }
            }
        }

        private bool _isReadOnly;
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

        private bool _isSampleFile;
        /// <summary>
        /// Whether or not this document is the PassKeep sample document.
        /// </summary>
        public bool IsSampleFile
        {
            get { return this._isSampleFile; }
            private set
            {
                if(TrySetProperty(ref this._isSampleFile, value))
                {
                    OnPropertyChanged(nameof(ForbidRememberingDatabase));
                }
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

        private StorageFile _keyFile;
        /// <summary>
        /// The key file used to unlock the document.
        /// </summary>
        public StorageFile KeyFile
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
        /// Whether to not allow remembering the current database.
        /// </summary>
        public bool ForbidRememberingDatabase
        {
            get
            {
                return this.IsSampleFile || this.CandidateFile?.CannotRememberText != null;
            }
        }

        private bool _rememberDatabase;
        /// <summary>
        /// Whether to remember the database on successful unlock for future access.
        /// Always false for a sample database.
        /// </summary>
        public bool RememberDatabase
        {
            get
            {
                return this.ForbidRememberingDatabase ? false : this._rememberDatabase;
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

        private ActionCommand _unlockCommand;
        /// <summary>
        /// ActionCommand used to attempt a document unlock using the provided credentials.
        /// </summary>
        public ActionCommand UnlockCommand
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

        private ActionCommand _useSavedCredentialsCommand;
        /// <summary>
        /// Loads saved credentials from storage and then performs the same work as
        /// <see cref="UnlockCommand"/>.
        /// </summary>
        public ActionCommand UseSavedCredentialsCommand
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
                lock (this.SyncRoot)
                {
                    if (TrySetProperty(ref this._parseResult, value))
                    {
                        OnPropertyChanged(nameof(HasGoodHeader));
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
                    this.UseSavedCredentialsCommand.RaiseCanExecuteChanged();
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

        /// <summary>
        /// Parses the header of the CandidateFile and handles updating status on the View.
        /// </summary>
        private async Task ValidateHeader()
        {
            Dbg.Assert(this.kdbxReader != null);
            if (this.kdbxReader == null)
            {
                throw new InvalidOperationException("Cannot validate KDBX header if there is no reader instance");
            }

            try
            {
                if (this.CandidateFile == null)
                {
                    this.ParseResult = null;
                }
                else
                {
                    using (IRandomAccessStream fileStream = await this.CandidateFile.GetRandomReadAccessStreamAsync())
                    {
                        CancellationTokenSource cts = new CancellationTokenSource(5000);
                        this.ParseResult = await this.kdbxReader.ReadHeader(fileStream, cts.Token);
                    }
                }
            }
            catch(COMException)
            {
                // In the Windows 8.1 preview, opening a stream to a SkyDrive file can fail with no workaround.
                this.ParseResult = new ReaderResult(KdbxParserCode.UnableToReadFile);
            }
            finally
            {
                this.RaiseHeaderValidated();
                this.UnlockCommand.RaiseCanExecuteChanged();
                this.UseSavedCredentialsCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// CanExecute callback for the UnlockCommand - determines whether the document file is unlockable.
        /// </summary>
        /// <returns>Whether the document file can be unlocked in the current state.</returns>
        private bool CanUnlock()
        {
            // Verify all the appropriate data exists and the last parse event was successful.
            return this.CandidateFile != null && this.HasGoodHeader;
        }

        /// <summary>
        /// Execution action for the UnlockCommand - attempts to unlock the document file
        /// with the ViewModel's credentials.
        /// </summary>
        private void DoUnlock()
        {
            DoUnlock(null);
        }

        /// <summary>
        /// Attempts to unlock the document file.
        /// </summary>
        /// <param name="storedCredential">The key to use for decryption - if null, the ViewModel's
        /// credentials are used instead.</param>
        private async void DoUnlock(IBuffer storedCredential)
        {
            Dbg.Assert(this.CanUnlock());
            if (!this.CanUnlock())
            {
                throw new InvalidOperationException("The ViewModel is not in a state that can unlock the database!");
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            this.RaiseStartedUnlocking(cts);

            try
            {
                using (IRandomAccessStream stream = await this.CandidateFile.GetRandomReadAccessStreamAsync())
                {
                    KdbxDecryptionResult result;
                    if (storedCredential != null)
                    {
                        result = await this.kdbxReader.DecryptFile(stream, storedCredential, cts.Token);
                    }
                    else
                    {
                        result = await this.kdbxReader.DecryptFile(stream, this.Password, this.KeyFile, cts.Token);
                    }

                    this.ParseResult = result.Result;
                    this.RaiseStoppedUnlocking();

                    Dbg.Trace($"Got ParseResult from database unlock attempt: {this.ParseResult}");
                    if (!this.ParseResult.IsError)
                    {
                        if (this.RememberDatabase)
                        {
                            string accessToken = this.futureAccessList.Add(this.CandidateFile.StorageItem, this.CandidateFile.FileName);
                            Dbg.Trace($"Unlock was successful and database was remembered with token: {accessToken}");
                        }
                        else
                        {
                            Dbg.Trace("Unlock was successful but user opted not to remember the database.");
                        }

                        if (this.SaveCredentials)
                        {
                            bool storeCredential = false;

                            // If we were not already using a stored credential, we need user
                            // consent to continue.
                            if (storedCredential == null)
                            {
                                storeCredential = await this.identityService.VerifyIdentityAsync();
                                storedCredential = result.GetRawKey();
                            }
                            else
                            {
                                // If we have a stored credential, we already got consent.
                                storeCredential = true;
                            }

                            if (storeCredential)
                            {
                                // XXX - Handle the failure case
                                await this.credentialProvider.TryStoreRawKeyAsync(this.CandidateFile, storedCredential);
                            }
                        }

                        RaiseDocumentReady(result.GetDocument());
                    }
                }
            }
            catch (COMException)
            {
                // In the Windows 8.1 preview, opening a stream to a SkyDrive file can fail with no workaround.
                this.ParseResult = new ReaderResult(KdbxParserCode.UnableToReadFile);
                this.RaiseStoppedUnlocking();
            }
        }

        /// <summary>
        /// Execution action for the UseSavedCredentials - attempts to unlock the document file
        /// using stored credentials after verifying the user's identity.
        /// </summary>
        private async void DoUnlockWithSavedCredentials()
        {
            if (!await this.identityService.VerifyIdentityAsync())
            {
                return;
            }

            IBuffer storedCredential = await this.credentialProvider.GetRawKeyAsync(this.CandidateFile);
            if (storedCredential == null)
            {
                return;
            }

            DoUnlock(storedCredential);
        }
    }
}
