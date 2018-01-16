// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Files;
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
        private bool _rememberDatabase, _useEmpty;
        private readonly IMasterKeyViewModel masterKeyViewModel;
        private readonly IKdbxWriterFactory writerFactory;
        private readonly IDatabaseAccessList futureAccessList;
        private readonly ITaskNotificationService taskNotificationService;
        private readonly IDatabaseCandidateFactory candidateFactory;

        public DatabaseCreationViewModel(
            ITestableFile file,
            IMasterKeyViewModel masterKeyViewModel,
            IDatabaseSettingsViewModelFactory settingsVmFactory,
            IKdbxWriterFactory writerFactory,
            IDatabaseAccessList futureAccessList,
            ITaskNotificationService taskNotificationService,
            IDatabaseCandidateFactory candidateFactory
        )
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            this.masterKeyViewModel = masterKeyViewModel ?? throw new ArgumentNullException(nameof(masterKeyViewModel));
            Settings = settingsVmFactory?.Assemble() ?? throw new ArgumentNullException(nameof(settingsVmFactory));
            this.writerFactory = writerFactory ?? throw new ArgumentNullException(nameof(writerFactory));
            this.futureAccessList = futureAccessList ?? throw new ArgumentNullException(nameof(futureAccessList));
            this.taskNotificationService = taskNotificationService ?? throw new ArgumentNullException(nameof(taskNotificationService));
            this.candidateFactory = candidateFactory ?? throw new ArgumentNullException(nameof(candidateFactory));
            
            CreateEmpty = true;
            Remember = true;
        }

        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();
            this.masterKeyViewModel.Confirmed += MasterKeyViewModel_Confirmed;
        }

        public override async Task SuspendAsync()
        {
            await base.SuspendAsync();
            this.masterKeyViewModel.Confirmed -= MasterKeyViewModel_Confirmed;
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
        /// Provides the master key for the new database.
        /// </summary>
        public IMasterKeyViewModel MasterKeyViewModel => this.masterKeyViewModel;

        /// <summary>
        /// Whether to remember this database in the future.
        /// </summary>
        public bool Remember
        {
            get { return this._rememberDatabase; }
            set { TrySetProperty(ref this._rememberDatabase, value); }
        }

        /// <summary>
        /// Whether to use an empty database instead of using the sample as a basis.
        /// </summary>
        public bool CreateEmpty
        {
            get { return this._useEmpty; }
            set { TrySetProperty(ref this._useEmpty, value); }
        }

        public ICommand CreateCommand => this.masterKeyViewModel.ConfirmCommand;

        /// <summary>
        /// Event handler for the key ViewModel's confirmation event, indicating a user is locking in
        /// the specified configuration and credentials.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MasterKeyViewModel_Confirmed(object sender, EventArgs e)
        {
            GenerateDatabase();
        }

        /// <summary>
        /// Uses provided options to generate a database file.
        /// </summary>
        private async void GenerateDatabase()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            IKdbxWriter writer = this.writerFactory.Assemble(
                this.masterKeyViewModel.MasterPassword,
                this.masterKeyViewModel.KeyFile,
                Settings.Cipher,
                Settings.GetKdfParameters()
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
