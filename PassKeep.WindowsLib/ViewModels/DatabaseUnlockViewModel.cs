using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.IO;
using SariphLib.Mvvm;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Represents a View over a locked database, with the potential for unlocking.
    /// </summary>
    public sealed class DatabaseUnlockViewModel : BindableBase, IDatabaseUnlockViewModel
    {
        private IKdbxReader kdbxReader;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="file">The candidate database file.</param>
        /// <param name="isSampleFile">Whether the file is a PassKeep sample.</param>
        public DatabaseUnlockViewModel(StorageFile file, bool isSampleFile)
        {
            this.CandidateFile = file;
            this.IsSampleFile = isSampleFile;
        }

        /// <summary>
        /// Event that indicates an unlock attempt has begun.
        /// </summary>
        public event EventHandler<CancelableEventArgs> StartedUnlocking;
        private void raiseStartedUnlocking(IKdbxReader reader)
        {
            if (StartedUnlocking != null)
            {
                CancelableEventArgs eventArgs = new CancelableEventArgs(reader.Cancel);
                StartedUnlocking(this, eventArgs);
            }
        }

        /// <summary>
        /// Event that indicates an unlock attempt has stopped (successfully or unsuccessfully).
        /// </summary>
        public event EventHandler StoppedUnlocking;
        private void raiseStoppedUnlocking()
        {
            if (StoppedUnlocking != null)
            {
                StoppedUnlocking(this, new EventArgs());
            }
        }

        /// <summary>
        /// Event that indicates a decrypted document is ready for consumtpion.
        /// </summary>
        public event EventHandler<DocumentReadyEventArgs> DocumentReady;
        private void raiseDocumentReady(XDocument document, IRandomNumberGenerator rng)
        {
            if (DocumentReady != null)
            {
                DocumentReady(this, new DocumentReadyEventArgs(document, rng));
            }
        }

        private StorageFile _candidateFile;
        /// <summary>
        /// The StorageFile representing the locked database.
        /// </summary>
        public StorageFile CandidateFile
        {
            get
            {
                return _candidateFile;
            }
            set
            {
                if (_candidateFile != null && SetProperty(ref _candidateFile, value))
                {
                    // If we have set a new file and it's not null, parse the header.
                    this.ValidateHeader();
                }
            }
        }

        private bool _isSampleFile;
        /// <summary>
        /// Whether or not this database is the PassKeep sample database.
        /// </summary>
        public bool IsSampleFile
        {
            get { return this._isSampleFile; }
            set { SetProperty(ref this._isSampleFile, value); }
        }

        private string _password;
        /// <summary>
        /// The password used to unlock the database.
        /// </summary>
        public string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                SetProperty(ref this._password, value);
            }
        }

        private StorageFile _keyFile;
        /// <summary>
        /// The key file used to unlock the database.
        /// </summary>
        public StorageFile KeyFile
        {
            get
            {
                return this._keyFile;
            }
            set
            {
                SetProperty(ref this._keyFile, value);
            }
        }

        private ActionCommand _unlockCommand;
        /// <summary>
        /// ActionCommand used to attempt a database unlock using the provided credentials.
        /// </summary>
        public ActionCommand UnlockCommand
        {
            get
            {
                return this._unlockCommand;
            }
            private set
            {
                SetProperty(ref this._unlockCommand, value);
            }
        }

        /// <summary>
        /// Whether the cleartext header of the candidate file is valid.
        /// </summary>
        public bool HasGoodHeader
        {
            get
            {
                if (this.kdbxReader == null || this.ParseResult == null)
                {
                    return false;
                }

                return !this.ParseResult.IsError;
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
            set
            {
                if (SetProperty(ref this._parseResult, value))
                {
                    OnPropertyChanged("HasGoodHeader");
                }
            }
        }

        private async void ValidateHeader()
        {
            Debug.Assert(this.CandidateFile != null);
            Debug.Assert(this.kdbxReader != null);

            if (this.CandidateFile == null)
            {
                throw new InvalidOperationException("Cannot validate KDBX header if there is no file to validate");
            }

            if (this.kdbxReader == null)
            {
                throw new InvalidOperationException("Cannot validate KDBX header if there is no reader instance");
            }

            try
            {
                using (IRandomAccessStream fileStream = await this.CandidateFile.OpenReadAsync())
                {
                    this.ParseResult = await this.kdbxReader.ReadHeader(fileStream);
                }
            }
            catch(COMException)
            {
                // In the Windows 8.1 preview, opening a stream to a SkyDrive file can fail with no workaround.
                this.ParseResult = new ReaderResult(KdbxParserCode.UnableToReadFile);
            }
        }
    }
}
