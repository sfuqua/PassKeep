using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using PassKeep.Common;
using PassKeep.Controls;
using PassKeep.KeePassLib;
using PassKeep.KeePassLib.Crypto;
using PassKeep.Models;
using PassKeep.Models.Abstraction;
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

        private DatabaseNavigationViewModel _breadcrumbViewModel;
        public DatabaseNavigationViewModel BreadcrumbViewModel
        {
            get { return _breadcrumbViewModel; }
            private set { SetProperty(ref _breadcrumbViewModel, value); }
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

        public EntryDetailsViewModel GetEntryDetailViewModel(IKeePassEntry entry, bool forEdit = false)
        {
            EntryDetailsViewModel viewModel = new EntryDetailsViewModel(entry.Clone(), this, Settings, !forEdit);
            return viewModel;
        }

        public GroupDetailsViewModel GetGroupDetailViewModel(IKeePassGroup group, bool forEdit = false)
        {
            GroupDetailsViewModel viewModel = new GroupDetailsViewModel(group.Clone(), this, Settings, !forEdit);
            return viewModel;
        }

        public KdbxMetadata GetDbMetadata()
        {
            return Document.Metadata;
        }

        public IRandomNumberGenerator GetRng()
        {
            return rng;
        }

        public event EventHandler<ActiveEntryChangedEventArgs> ActiveEntryChanged;

        private IKdbxWriter writer;
        private IRandomNumberGenerator rng;
        private XDocument backupDocument;
        public KdbxDocument Document { get; private set; }

        private bool _isSample;
        public DatabaseViewModel(ConfigurationViewModel appSettings, IKdbxWriter writer, StorageFile file, IRandomNumberGenerator rng, bool isSample = false)
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
                () => BreadcrumbViewModel.ActiveLeaf != null,
                () => { onDetailsRequested(); }
            );

            this.writer = writer;
            this.backupDocument = writer.Document;
            this.File = file;
            this.rng = rng;
            this._isSample = isSample;

            BreadcrumbViewModel = new DatabaseNavigationViewModel(appSettings);
            BreadcrumbViewModel.PropertyChanged += BreadcrumbViewModel_PropertyChanged;
        }

        private IKeePassEntry lastActiveEntry = null;
        private void BreadcrumbViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveLeaf")
            {
                IKeePassEntry newActiveEntry = BreadcrumbViewModel.ActiveLeaf;
                onActiveEntryChanged(new ActiveEntryChangedEventArgs(lastActiveEntry, newActiveEntry));
                lastActiveEntry = newActiveEntry;
                RequestDetailsCommand.RaiseCanExecuteChanged();

                if (newActiveEntry == null)
                {
                    _activeUri = null;
                    activeUriIsValid = false;
                }
                else
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(newActiveEntry.OverrideUrl))
                        {
                            _activeUri = new Uri(newActiveEntry.OverrideUrl, UriKind.Absolute);
                        }
                        else
                        {
                            _activeUri = new Uri(newActiveEntry.Url.ClearValue, UriKind.Absolute);
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

        /// <summary>
        /// Initializes the KdbxDocument that is the root of the database by parsing the XML.
        /// Sets the current root of the VM to the DatabaseGroup, navigates into the DatabaseGroup,
        /// and adds it to the Breadcrumb trail.
        /// </summary>
        /// <returns>True if the tree was built (first run), false if nothing happened.</returns>
        public async Task<bool> BuildTree()
        {
            Debug.WriteLine("Attempting to build tree within DatabaseViewModel...");
            if (Document != null)
            {
                Debug.WriteLine("...no build was necessary. Aborting.");
                return false;
            }

            Debug.WriteLine("...tree has not been built, starting async task to do so.");
            await Task.Run(() =>
                {
                    Document = new KdbxDocument(writer.Document.Root, rng);
                }
            );
            Debug.WriteLine("Tree has finished building. Setting up initial state of ViewModel.");

            BreadcrumbViewModel.SetGroup(Document.Root.DatabaseGroup);
            return true;
        }

        public bool GoUp()
        {
            Debug.WriteLine("Trying to navigate up one level.");
            if (BreadcrumbViewModel.ActiveGroup == null || BreadcrumbViewModel.ActiveGroup.Parent == null)
            {
                Debug.WriteLine("Cannot go up - returning false.");
                return false;
            }

            BreadcrumbViewModel.SetGroup(BreadcrumbViewModel.ActiveGroup.Parent);
            if (BreadcrumbViewModel.ActiveGroup == null || BreadcrumbViewModel.ActiveGroup.Parent == null)
            {
                Debug.WriteLine("We have reached the top of the database and can no longer navigate up.");
            }
            return true;
        }

        /// <summary>
        /// "Selects" the given Entry or Group.
        /// If an Entry, navigates to the parent Group and marks the Entry as active.
        /// If a Group, navigates to it and optionally resets ActiveEntry.
        /// </summary>
        /// <param name="node">The entity being selected</param>
        /// <param name="resetEntry">Whether to clear ActiveEntry on a node navigation</param>
        public void Select(IKeePassNode node, bool resetEntry = false)
        {
            Debug.Assert(node != null);
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            Debug.WriteLine("Selecting a new element for the DatabaseViewModel...");

            if (node is IKeePassGroup)
            {
                IKeePassGroup group = (IKeePassGroup)node;
                Debug.WriteLine("...element is a Group, and resetEntry is {0}.", resetEntry);

                IKeePassGroup activeGroup = BreadcrumbViewModel.ActiveGroup;
                BreadcrumbViewModel.SetGroup(group);
                if (group.Parent != null && group.Parent.Equals(activeGroup))
                {
                    Debug.WriteLine("Group was a child of the currently active Group - we navigated down one level.");
                }
                else
                {
                    Debug.WriteLine("Group was not a child of the current Group - we navigated arbitrarily.");
                }
            }
            else if (node is IKeePassEntry)
            {
                IKeePassEntry entry = (IKeePassEntry)node;
                Debug.WriteLine("...element is an Entry.");
                BreadcrumbViewModel.SetEntry(entry);
            }
            else
            {
                throw new ArgumentException("node is not a KeePass object", "node");
            }
        }

        private void onActiveEntryChanged(ActiveEntryChangedEventArgs e)
        {
            if (ActiveEntryChanged != null)
            {
                ActiveEntryChanged(this, e);
            }
        }

        private void addToSearch(IKeePassGroup root, ObservableCollection<IKeePassNode> soFar, bool addRoot = true)
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

        public ObservableCollection<IKeePassNode> GetAll()
        {
            ObservableCollection<IKeePassNode> items = new ObservableCollection<IKeePassNode>();
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
                return true;
            }
            
            onStartedUnlock();
            if (await writer.Write(File, Document))
            {
                backupDocument = writer.Document;
                onDoneWrite();
                return true;
            }

            onDoneWrite();
            return false;
        }
    }

    public class ActiveEntryChangedEventArgs : EventArgs
    {
        public IKeePassEntry OldEntry { get; set; }
        public IKeePassEntry NewEntry { get; set; }
        public ActiveEntryChangedEventArgs(IKeePassEntry oldEntry, IKeePassEntry newEntry)
        {
            OldEntry = oldEntry;
            NewEntry = newEntry;
        }
    }
}
