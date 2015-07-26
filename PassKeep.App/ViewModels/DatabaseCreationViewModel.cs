using System;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using Windows.Storage;
using System.Windows.Input;
using SariphLib.Mvvm;
using PassKeep.Lib.EventArgClasses;
using System.Threading;
using Windows.ApplicationModel;
using SariphLib.Infrastructure;
using PassKeep.Lib.Contracts.KeePass;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Models;
using System.Collections.Generic;
using PassKeep.Contracts.Models;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// ViewModel used to create a new database.
    /// </summary>
    public class DatabaseCreationViewModel : AbstractViewModel, IDatabaseCreationViewModel
    {
        private string _masterPassword, _confirmedPassword;
        private bool _rememberDatabase, _useEmpty;
        private StorageFile _keyFile;
        private IKdbxWriterFactory writerFactory;
        private IDatabaseAccessList futureAccessList;

        public DatabaseCreationViewModel(
            IStorageFile file,
            IKdbxWriterFactory writerFactory,
            IDatabaseAccessList futureAccessList
        )
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (writerFactory == null)
            {
                throw new ArgumentNullException(nameof(writerFactory));
            }

            if (futureAccessList == null)
            {
                throw new ArgumentNullException(nameof(futureAccessList));
            }

            this.File = file;
            this.writerFactory = writerFactory;
            this.futureAccessList = futureAccessList;

            this.CreateCommand = new ActionCommand(
                () => this.ConfirmedPassword == this.MasterPassword,
                GenerateDatabase
            );

            this.MasterPassword = String.Empty;
            this.ConfirmedPassword = String.Empty;
            this.CreateEmpty = true;
            this.Remember = true;
        }

        /// <summary>
        /// Invoked when the ViewModel begins generating a database file.
        /// </summary>
        public event EventHandler<CancellableEventArgs> StartedGeneration;

        /// <summary>
        /// Invoked when the ViewModel stops generating a database file.
        /// </summary>
        public event EventHandler StoppedGeneration;

        /// <summary>
        /// Invoked when the document has been successfully created.
        /// </summary>
        public event EventHandler<DocumentReadyEventArgs> DocumentReady;

        /// <summary>
        /// The new file being used for the database.
        /// </summary>
        public IStorageFile File
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
                    ((ActionCommand)this.CreateCommand).RaiseCanExecuteChanged();
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
                    ((ActionCommand)this.CreateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// The keyfile to use.
        /// </summary>
        public StorageFile KeyFile
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
            StartedGeneration?.Invoke(this, new CancellableEventArgs(cts));

            IKdbxWriter writer = this.writerFactory.Assemble(this.MasterPassword, this.KeyFile);
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

            using (IRandomAccessStream stream = await this.File.OpenAsync(FileAccessMode.ReadWrite))
            {
                if (await writer.Write(stream, newDocument, cts.Token))
                {
                    this.futureAccessList.Add(this.File, this.File.Name);
                    StoppedGeneration?.Invoke(this, EventArgs.Empty);
                    DocumentReady?.Invoke(this, new DocumentReadyEventArgs(newDocument, writer, writer.HeaderData.GenerateRng()));
                }
            }
        }
    }
}
