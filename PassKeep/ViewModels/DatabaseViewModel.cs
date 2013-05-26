using PassKeep.Common;
using PassKeep.Controls;
using PassKeep.KeePassLib;
using PassKeep.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;
using Windows.System;

namespace PassKeep.ViewModels
{
    public class DatabaseViewModel : ViewModelBase
    {
        private StorageFile _file;
        public StorageFile File
        {
            get { return _file; }
            set { SetProperty(ref _file, value); }
        }

        private ObservableCollection<IGroup> _items;
        public ObservableCollection<IGroup> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        private ObservableCollection<KdbxEntry> _entries;
        public ObservableCollection<KdbxEntry> Entries
        {
            get { return _entries; }
            set
            {
                SetProperty(ref _entries, value);
                _entries.CollectionChanged += (s, e) =>
                    {
                        OnPropertyChanged("HasEntries");
                    };
                OnPropertyChanged("HasEntries");
            }
        }

        private ObservableCollection<KdbxGroup> _breadcrumbs;
        public ObservableCollection<KdbxGroup> Breadcrumbs
        {
            get { return _breadcrumbs; }
            set { SetProperty(ref _breadcrumbs, value); }
        }

        private IEntry _activeEntry;
        public IEntry ActiveEntry
        {
            get { return _activeEntry; }
            set
            {
                IEntry temp = _activeEntry;
                if (SetProperty(ref _activeEntry, value))
                {
                    onActiveEntryChanged(new ActiveEntryChangedEventArgs(temp, value));
                    RequestDetailsCommand.RaiseCanExecuteChanged();
                }

                if(value == null)
                {
                    _activeUri = null;
                    activeUriIsValid = false;
                }
                else
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(value.OverrideUrl))
                        {
                            _activeUri = new Uri(value.OverrideUrl, UriKind.Absolute);
                        }
                        else
                        {
                            _activeUri = new Uri(value.Url.ClearValue, UriKind.Absolute);
                        }
                        activeUriIsValid = true;
                    }
                    catch (Exception)
                    {
                        activeUriIsValid = false;
                    }
                }
            }
        }
        public DelegateCommand RequestDetailsCommand { get; set; }
        public event EventHandler DetailsRequested;
        private void onDetailsRequested()
        {
            if (DetailsRequested != null)
            {
                DetailsRequested(this, new EventArgs());
            }
        }

        public bool HasEntries
        {
            get { return _entries != null && _entries.Count > 0; }
        }

        private Uri _activeUri;
        private bool _activeUriIsValid;
        private bool activeUriIsValid
        {
            get { return _activeUriIsValid; }
            set
            {
                _activeUriIsValid = value;
                UrlLaunchCommand.RaiseCanExecuteChanged();
            }
        }
        public DelegateCommand UrlLaunchCommand { get; set; }

        public EntryDetailsViewModel GetEntryDetailViewModel(KdbxEntry entry, bool forEdit = false)
        {
            EntryDetailsViewModel viewModel = new EntryDetailsViewModel(entry.Clone(), this, Settings, !forEdit);
            return viewModel;
        }

        public GroupDetailsViewModel GetGroupDetailViewModel(KdbxGroup group, bool forEdit = false)
        {
            GroupDetailsViewModel viewModel = new GroupDetailsViewModel(group.Clone(), this, Settings, !forEdit);
            return viewModel;
        }

        public KdbxMetadata GetDbMetadata()
        {
            return Document.Metadata;
        }

        public KeePassRng GetRng()
        {
            return rng;
        }

        public event EventHandler DirectoryLevelIncreased;
        public event EventHandler DirectoryLevelReset;
        public event EventHandler<ActiveEntryChangedEventArgs> ActiveEntryChanged;

        private KdbxWriter writer;
        private KdbxGroup currentRoot;
        private KeePassRng rng;
        private XDocument backupDocument;
        public KdbxDocument Document { get; private set; }

        private bool _isSample;
        public DatabaseViewModel(ConfigurationViewModel appSettings, KdbxWriter writer, StorageFile file, KeePassRng rng, bool isSample = false)
            : base(appSettings)
        {
            UrlLaunchCommand = new DelegateCommand(
                () => activeUriIsValid,
                async () =>
                {
                    await Launcher.LaunchUriAsync(_activeUri);
                }
            );

            RequestDetailsCommand = new DelegateCommand(
                () => ActiveEntry != null,
                () => { onDetailsRequested(); }
            );

            this.writer = writer;
            this.backupDocument = writer.Document;
            this.File = file;
            this.rng = rng;
            this._isSample = isSample;

            Breadcrumbs = new ObservableCollection<KdbxGroup>();
            Entries = new ObservableCollection<KdbxEntry>();
        }

        public async Task<bool> BuildTree()
        {
            if (Document != null)
            {
                return false;
            }

            await Task.Run(() =>
                {
                    Document = new KdbxDocument(writer.Document.Root, rng);
                }
            );

            currentRoot = Document.Root.DatabaseGroup;
            Breadcrumbs.Add(currentRoot);
            Synchronize();

            if (Entries.Count == 0 && Items.Count == 1)
            {
                Select(Items[0]);
            }

            return true;
        }

        public void Synchronize()
        {
            var newItems = new ObservableCollection<IGroup>(
                currentRoot.Groups
            );

            Entries = new ObservableCollection<KdbxEntry>(
                currentRoot.Entries
            );

            foreach(KdbxEntry entry in Entries)
            {
                newItems.Add(entry);
            }

            Items = newItems;
        }

        public bool GoUp()
        {
            if (currentRoot.Parent == null || Breadcrumbs.Count == 1)
            {
                return false;
            }

            currentRoot = currentRoot.Parent;
            Breadcrumbs.RemoveAt(Breadcrumbs.Count - 1);
            Synchronize();
            if (currentRoot.Parent == null || Breadcrumbs.Count == 1)
            {
                onDirectoryLevelReset();
            }
            return true;
        }

        public void Select(object obj, bool resetEntry = false)
        {
            Debug.Assert(obj != null);
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            if (obj is KdbxGroup)
            {
                KdbxGroup directory = (KdbxGroup)obj;

                if (directory.Parent != null && directory.Parent.Equals(currentRoot))
                {
                    currentRoot = currentRoot.Groups.First(g => g.Uuid.Equals(directory.Uuid));
                    Breadcrumbs.Add(directory);
                    Synchronize();
                    onDirectoryLevelIncreased();
                    if (resetEntry)
                    {
                        ActiveEntry = null;
                    }
                }
                else
                {
                    ActiveEntry = null;
                    setArbitraryRoot(directory);
                }
            }
            else if (obj is KdbxEntry)
            {
                KdbxEntry newEntry = (KdbxEntry)obj;
                if (newEntry.Parent != currentRoot)
                {
                    setArbitraryRoot(newEntry.Parent);
                }

                ActiveEntry = newEntry;
            }
            else
            {
                throw new ArgumentException("obj is not a KeePass object", "obj");
            }
        }

        private void setArbitraryRoot(KdbxGroup newRoot)
        {
            // Handle directory level?
            currentRoot = newRoot;
            Synchronize();
            Breadcrumbs.Clear();

            KdbxGroup root = currentRoot;
            while (root != null)
            {
                Breadcrumbs.Insert(0, root);
                root = root.Parent;
            }
        }

        public void Navigate(int index)
        {
            Debug.Assert(index >= 0 && index < Breadcrumbs.Count);
            if (!(index >= 0 && index < Breadcrumbs.Count))
            {
                throw new ArgumentOutOfRangeException("index");
            }

            int n = Breadcrumbs.Count;
            for (int i = index + 1; i < n; i++)
            {
                Breadcrumbs.RemoveAt(index + 1);
                currentRoot = currentRoot.Parent;
            }

            Synchronize();

            if (index == 0)
            {
                onDirectoryLevelReset();
            }
        }

        private void onDirectoryLevelReset()
        {
            if (DirectoryLevelReset != null)
            {
                DirectoryLevelReset(this, new EventArgs());
            }
        }

        private void onDirectoryLevelIncreased()
        {
            if (DirectoryLevelIncreased != null)
            {
                DirectoryLevelIncreased(this, new EventArgs());
            }
        }

        private void onActiveEntryChanged(ActiveEntryChangedEventArgs e)
        {
            if (ActiveEntryChanged != null)
            {
                ActiveEntryChanged(this, e);
            }
        }

        private void addToSearch(KdbxGroup root, ObservableCollection<IGroup> soFar, bool addRoot = true)
        {
            if (root.EnableSearching.HasValue && !root.EnableSearching.Value)
            {
                return;
            }

            if (addRoot)
            {
                soFar.Add(root);
            }

            var groups = root.Groups;
            foreach (var group in groups)
            {
                addToSearch(group, soFar);
            }

            var entries = root.Entries;
            foreach (var entry in entries)
            {
                soFar.Add(entry);
            }
        }

        public ObservableCollection<IGroup> GetAll()
        {
            ObservableCollection<IGroup> items = new ObservableCollection<IGroup>();
            addToSearch(Document.Root.DatabaseGroup, items, false);
            return items;
        }

        public event EventHandler<CancelableEventArgs> StartedWrite;
        private void onStartedUnlock()
        {
            Action cancelWrite = () =>
            {
                writer.CancelWrite();
            };

            if (StartedWrite != null)
            {
                StartedWrite(this, new CancelableEventArgs(cancelWrite));
            }
        }

        public event EventHandler DoneWrite;
        private void onDoneWrite()
        {
            if (DoneWrite != null)
            {
                DoneWrite(this, new EventArgs());
            }
        }

        public async Task<bool> Commit()
        {
            if (_isSample)
            {
                Synchronize();
                return true;
            }
            
            onStartedUnlock();
            if (await writer.Write(File, Document))
            {
                backupDocument = writer.Document;
                Synchronize();
                onDoneWrite();
                return true;
            }

            onDoneWrite();
            return false;
        }
    }

    public class ActiveEntryChangedEventArgs : EventArgs
    {
        public IEntry OldEntry { get; set; }
        public IEntry NewEntry { get; set; }
        public ActiveEntryChangedEventArgs(IEntry oldEntry, IEntry newEntry)
        {
            OldEntry = oldEntry;
            NewEntry = newEntry;
        }
    }
}
