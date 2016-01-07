using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
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

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="file">The candidate document file.</param>
        /// <param name="isSampleFile">Whether the file is a PassKeep sample.</param>
        /// <param name="futureAccessList">A database access list for persisting permission to the database.</param>
        /// <param name="reader">The IKdbxReader implementation used for parsing document files.</param>
        public DatabaseUnlockViewModel(IDatabaseCandidate file, bool isSampleFile, IDatabaseAccessList futureAccessList, IKdbxReader reader)
        {
            Dbg.Assert(reader != null);
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this.futureAccessList = futureAccessList;
            this.kdbxReader = reader;
            this.UnlockCommand = new ActionCommand(this.CanUnlock, this.DoUnlock);
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
                    this.ParseResult = null;

                    #pragma warning disable 4014

                    // If we have set a new file, parse the header. 
                    // This is intentionally asynchronous.
                    this.ValidateHeader();

                    #pragma warning restore 4014
                }
            }
        }

        private bool _isSampleFile;
        /// <summary>
        /// Whether or not this document is the PassKeep sample document.
        /// </summary>
        public bool IsSampleFile
        {
            get { return this._isSampleFile; }
            private set { TrySetProperty(ref this._isSampleFile, value); }
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

        private bool _rememberDatabase;
        /// <summary>
        /// Whether to remember the database on successful unlock for future access.
        /// Always false for a sample database.
        /// </summary>
        public bool RememberDatabase
        {
            get
            {
                return (this.IsSampleFile ? false : this._rememberDatabase);
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

        /// <summary>
        /// Whether the cleartext header of the candidate file is valid.
        /// </summary>
        public bool HasGoodHeader
        {
            get
            {
                return this.kdbxReader != null && this.kdbxReader.HeaderData != null;
            }
        }

        /// <summary>
        /// The result of the last parse operation (either header validation or decryption).
        /// </summary>
        private ReaderResult _parseResult;
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
        /// Execution action for the UnlockCommand - attempts to unlock the document file.
        /// </summary>
        private async void DoUnlock()
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
                    KdbxDecryptionResult result = await this.kdbxReader.DecryptFile(stream, this.Password, this.KeyFile, cts.Token);
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
    }
}
