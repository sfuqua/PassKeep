using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Kdf;
using SariphLib.Files;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// ViewModel used to create a new database.
    /// </summary>
    public class DatabaseCreationViewModel : AbstractViewModel, IDatabaseCreationViewModel
    {
        private string _masterPassword, _confirmedPassword;
        private bool _rememberDatabase, _useEmpty;
        private int _encryptionRounds;
        private ITestableFile _keyFile;
        private readonly IKdbxWriterFactory writerFactory;
        private readonly IDatabaseAccessList futureAccessList;
        private readonly ITaskNotificationService taskNotificationService;
        private readonly IDatabaseCandidateFactory candidateFactory;

        public DatabaseCreationViewModel(
            ITestableFile file,
            IKdbxWriterFactory writerFactory,
            IDatabaseAccessList futureAccessList,
            ITaskNotificationService taskNotificationService,
            IDatabaseCandidateFactory candidateFactory
        )
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            this.writerFactory = writerFactory ?? throw new ArgumentNullException(nameof(writerFactory));
            this.futureAccessList = futureAccessList ?? throw new ArgumentNullException(nameof(futureAccessList));
            this.taskNotificationService = taskNotificationService ?? throw new ArgumentNullException(nameof(taskNotificationService));
            this.candidateFactory = candidateFactory ?? throw new ArgumentNullException(nameof(candidateFactory));

            CreateCommand = new ActionCommand(
                () => ConfirmedPassword == MasterPassword,
                GenerateDatabase
            );

            MasterPassword = String.Empty;
            ConfirmedPassword = String.Empty;
            EncryptionRounds = 6000;
            CreateEmpty = true;
            Remember = true;
        }

        /// <summary>
        /// Invoked when the document has been successfully created.
        /// </summary>
        public event EventHandler<DocumentReadyEventArgs> DocumentReady;

        /// <summary>
        /// The new file being used for the database.
        /// </summary>
        public ITestableFile File
        {
            get;
            private set;
        }

        /// <summary>
        /// Provides settings used by the database (key derivation, etc.)
        /// </summary>
        public IDatabaseSettingsViewModel Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// The password to use.
        /// </summary>
        public string MasterPassword
        {
            get { return this._masterPassword; }
            set
            {
                if (TrySetProperty(ref this._masterPassword, value))
                {
                    ((ActionCommand)CreateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Confirmation of password.
        /// </summary>
        public string ConfirmedPassword
        {
            get { return this._confirmedPassword; }
            set
            {
                if (TrySetProperty(ref this._confirmedPassword, value))
                {
                    ((ActionCommand)CreateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// The keyfile to use.
        /// </summary>
        public ITestableFile KeyFile
        {
            get { return this._keyFile; }
            set
            {
                SetProperty(ref this._keyFile, value);
            }
        }

        /// <summary>
        /// Whether to remember this database in the future.
        /// </summary>
        public bool Remember
        {
            get { return this._rememberDatabase; }
            set { TrySetProperty(ref this._rememberDatabase, value); }
        }

        /// <summary>
        /// The number of encryption rounds to use for the database.
        /// </summary>
        public int EncryptionRounds
        {
            get { return this._encryptionRounds; }
            set { TrySetProperty(ref this._encryptionRounds, value); }
        }

        /// <summary>
        /// Whether to use an empty database instead of using the sample as a basis.
        /// </summary>
        public bool CreateEmpty
        {
            get { return this._useEmpty; }
            set { TrySetProperty(ref this._useEmpty, value); }
        }

        /// <summary>
        /// Command used to lock in settings and create the database.
        /// </summary>
        public ICommand CreateCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Uses provided options to generate a database file.
        /// </summary>
        private async void GenerateDatabase()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            IKdbxWriter writer = this.writerFactory.Assemble(
                MasterPassword,
                KeyFile,
                EncryptionAlgorithm.Aes,
                new AesParameters((ulong)EncryptionRounds)
            );
            IRandomNumberGenerator rng = writer.HeaderData.GenerateRng();

            KdbxDocument newDocument = new KdbxDocument(new KdbxMetadata("PassKeep Database"));

            if (!CreateEmpty)
            {
                IList<IKeePassGroup> groups = new List<IKeePassGroup>
                {
                    new KdbxGroup(newDocument.Root.DatabaseGroup),
                    new KdbxGroup(newDocument.Root.DatabaseGroup),
                    new KdbxGroup(newDocument.Root.DatabaseGroup),
                    new KdbxGroup(newDocument.Root.DatabaseGroup),
                    new KdbxGroup(newDocument.Root.DatabaseGroup),
                    new KdbxGroup(newDocument.Root.DatabaseGroup)
                };

                groups[0].Title.ClearValue = "General";
                groups[1].Title.ClearValue = "Windows";
                groups[2].Title.ClearValue = "Network";
                groups[3].Title.ClearValue = "Internet";
                groups[4].Title.ClearValue = "eMail";
                groups[5].Title.ClearValue = "Homebanking";

                groups[0].IconID = 48;
                groups[1].IconID = 38;
                groups[2].IconID = 3;
                groups[3].IconID = 1;
                groups[4].IconID = 19;
                groups[5].IconID = 37;

                foreach(IKeePassGroup group in groups)
                {
                    newDocument.Root.DatabaseGroup.Children.Add(group);
                }

                IList<IKeePassEntry> entries = new List<IKeePassEntry>
                {
                    new KdbxEntry(newDocument.Root.DatabaseGroup, rng, newDocument.Metadata),
                    new KdbxEntry(newDocument.Root.DatabaseGroup, rng, newDocument.Metadata)
                };

                entries[0].Title.ClearValue = "Sample Entry";
                entries[1].Title.ClearValue = "Sample Entry #2";

                entries[0].UserName.ClearValue = "User Name";
                entries[1].UserName.ClearValue = "Michael321";

                entries[0].Password.ClearValue = "Password";
                entries[1].Password.ClearValue = "12345";

                entries[0].Url.ClearValue = "http://keepass.info/";
                entries[1].Url.ClearValue = "http://keepass.info/help/kb/testform.html";

                entries[0].Notes.ClearValue = "Notes";

                foreach(IKeePassEntry entry in entries)
                {
                    newDocument.Root.DatabaseGroup.Children.Add(entry);
                }
            }

            using (IRandomAccessStream stream = await File.AsIStorageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                Task<bool> writeTask = writer.WriteAsync(stream, newDocument, cts.Token);
                this.taskNotificationService.PushOperation(writeTask, cts, AsyncOperationType.DatabaseEncryption);

                if (await writeTask)
                {
                    this.futureAccessList.Add(File, File.AsIStorageItem.Name);
                    DocumentReady?.Invoke(
                        this,
                        new DocumentReadyEventArgs(
                            newDocument,
                            await this.candidateFactory.AssembleAsync(File),
                            writer,
                            writer.HeaderData.GenerateRng()
                        )
                    );
                }
            }
        }
    }
}
