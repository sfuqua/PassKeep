using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Eventing;
using SariphLib.Infrastructure;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Backs a View over an unlocked document, allowing for navigation and manipulation of nodes.
    /// </summary>
    public class DatabaseViewModel : DatabasePersistenceViewModel, IDatabaseViewModel
    {
        private const string DatabaseOrderStringKey = "DatabaseOrder";
        private const string AlphabetOrderStringKey = "Alphabet";
        private const string AlphabetOrderReverseStringKey = "AlphabetReverse";

        /// <summary>
        /// A Comparer that sorts Groups before Entries.
        /// </summary>
        private static Comparer<IKeePassNode> NodeComparer;

        private IResourceProvider resourceProvider;
        private IRandomNumberGenerator rng;
        private IAppSettingsService settingsService;
        private ISensitiveClipboardService clipboardService;
        private IList<DatabaseSortMode> availableSortModes;
        private ObservableCollection<IDatabaseNodeViewModel> sortedChildren;
        private IKeePassGroup activeGroup;

        private DatabaseSortMode _sortMode;
        private string filter;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static DatabaseViewModel()
        {
            // We always want groups to show up before entries.
            // To accomplish this, we order first 
            DatabaseViewModel.NodeComparer = Comparer<IKeePassNode>.Create(
                (nodeX, nodeY) =>
                {
                    if (nodeX is IKeePassGroup)
                    {
                        if (nodeY is IKeePassGroup)
                        {
                            // Both are groups
                            return 0;
                        }

                        // X is a group and Y is an entry
                        return -1;
                    }
                    else
                    {
                        if (nodeY is IKeePassGroup)
                        {
                            // X is an entry and Y is a group
                            return 1;
                        }

                        // Both are entries
                        return 0;
                    }
                }
            );
        }

        /// <summary>
        /// Initializes the ViewModel with the provided parameters.
        /// </summary>
        /// <param name="document">The document this ViewModel will represent.</param>
        /// <param name="resourceProvider">The IResourceProvider used to load strings.</param>
        /// <param name="rng">The random number generator used to protected entry strings in memory.</param>
        /// <param name="navigationViewModel">A ViewModel representing navigation state.</param>
        /// <param name="persistenceViewModel">A ViewModel used for persisting the document.</param>
        /// <param name="settingsService">The service used to access app settings.</param>
        public DatabaseViewModel(
            KdbxDocument document,
            IResourceProvider resourceProvider,
            IRandomNumberGenerator rng,
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService databasePersistenceService,
            IAppSettingsService settingsService,
            ISensitiveClipboardService clipboardService
        ) : base(document, databasePersistenceService)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            if (navigationViewModel == null)
            {
                throw new ArgumentNullException(nameof(navigationViewModel));
            }

            if (settingsService == null)
            {
                throw new ArgumentNullException(nameof(settingsService));
            }

            this.resourceProvider = resourceProvider;
            this.Document = document;
            this.rng = rng;

            if (navigationViewModel.ActiveGroup == null)
            {
                navigationViewModel.SetGroup(this.Document.Root.DatabaseGroup);
            }
            this.NavigationViewModel = navigationViewModel;

            this.settingsService = settingsService;
            this.clipboardService = clipboardService;

            this.availableSortModes = new List<DatabaseSortMode>
            {
                new DatabaseSortMode(
                    DatabaseSortMode.Mode.DatabaseOrder,
                    resourceProvider.GetString(DatabaseViewModel.DatabaseOrderStringKey)
                ),
                new DatabaseSortMode(
                    DatabaseSortMode.Mode.AlphabetAscending,
                    resourceProvider.GetString(DatabaseViewModel.AlphabetOrderStringKey)
                ),
                new DatabaseSortMode(
                    DatabaseSortMode.Mode.AlphabetDescending,
                    resourceProvider.GetString(DatabaseViewModel.AlphabetOrderReverseStringKey)
                )
            };
            this.AvailableSortModes = new ReadOnlyCollection<DatabaseSortMode>(this.availableSortModes);

            // Default to DatabaseOrder.
            // Set the backing field directly since we don't want to trigger all the property logic
            // from the constructor.
            this._sortMode = this.availableSortModes[0];

            // Set up collections.
            this.sortedChildren = new ObservableCollection<IDatabaseNodeViewModel>();
            this.SortedChildren = new ReadOnlyObservableCollection<IDatabaseNodeViewModel>(this.sortedChildren);

            // Set up the copy commands.
            this.RequestCopyUsernameCommand = new TypedCommand<IKeePassEntry>(
                entry =>
                {
                    this.clipboardService.CopyCredential(entry.UserName.ClearValue, ClipboardOperationType.UserName);
                }
            );

            this.RequestCopyPasswordCommand = new TypedCommand<IKeePassEntry>(
                entry =>
                {
                    this.clipboardService.CopyCredential(entry.Password.ClearValue, ClipboardOperationType.Password);
                }
            );
        }

        public override void Activate()
        {
            base.Activate();
            this.NavigationViewModel.PropertyChanged += this.OnNavigationViewModelPropertyChanged;
            this.NavigationViewModel.LeavesChanged += this.OnNavigationViewModelLeavesChanged;

            UpdateActiveGroupView();
        }

        public override void Suspend()
        {
            base.Suspend();
            this.NavigationViewModel.PropertyChanged -= this.OnNavigationViewModelPropertyChanged;
            this.NavigationViewModel.LeavesChanged -= this.OnNavigationViewModelLeavesChanged;
        }

        /// <summary>
        /// Raised when we should prompt the user to rename a specific node.
        /// </summary>
        public event TypedEventHandler<IDatabaseViewModel, IDatabaseNodeViewModel> RequestRenameNode;

        private void FireRequestRenameNode(IDatabaseNodeViewModel node)
        {
            Dbg.Assert(node != null);
            RequestRenameNode?.Invoke(this, node);
        }

        /// <summary>
        /// Raised when we should prompt the user to delete a specific node.
        /// </summary>
        public event TypedEventHandler<IDatabaseViewModel, IDatabaseNodeViewModel> RequestDeleteNode;

        private void FireRequestDeleteNode(IDatabaseNodeViewModel node)
        {
            Dbg.Assert(node != null);
            RequestDeleteNode?.Invoke(this, node);
        }

        /// <summary>
        /// Raised when the user requests details for a specific node.
        /// </summary>
        public event TypedEventHandler<IDatabaseViewModel, IDatabaseNodeViewModel> RequestDetails;

        private void FireRequestDetails(IDatabaseNodeViewModel node)
        {
            Dbg.Assert(node != null);
            RequestDetails?.Invoke(this, node);
        }

        /// <summary>
        /// The navigation ViewModel for the document.
        /// </summary>
        /// <remarks>
        /// This is responsible for tracking "where" the user is in the document.
        /// </remarks>
        public IDatabaseNavigationViewModel NavigationViewModel
        {
            get;
            private set;
        }

        /// <summary>
        /// The actual KdbxDocument represented by the ViewModel.
        /// </summary>
        public KdbxDocument Document
        {
            get;
            private set;
        }

        /// <summary>
        /// A command that is activated when the user requests to copy
        /// an entry's username.
        /// </summary>
        public ICommand RequestCopyUsernameCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// A command that is activated when the user requests to copy
        /// an entry's password.
        /// </summary>
        public ICommand RequestCopyPasswordCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// A listing of all known, available sort modes.
        /// </summary>
        public IReadOnlyCollection<DatabaseSortMode> AvailableSortModes
        {
            get;
            private set;
        }

        /// <summary>
        /// The current DatabaseSortMode used by this ViewModel.
        /// </summary>
        public DatabaseSortMode SortMode
        {
            get { return this._sortMode; }
            set
            {
                if (!this.AvailableSortModes.Contains(value))
                {
                    throw new ArgumentException("Unknown sort mode!", nameof(value));
                }

                if (TrySetProperty(ref this._sortMode, value))
                {
                    this.settingsService.DatabaseSortMode = value.SortMode;

                    // Re-sort
                    UpdateActiveGroupView();
                }
            }
        }

        /// <summary>
        /// A filter used to change the displayed children.
        /// </summary>
        public string Filter
        {
            get { return this.filter; }
            set
            {
                if (TrySetProperty(ref this.filter, value))
                {
                    UpdateActiveGroupView();
                }
            }
        }

        /// <summary>
        /// Allows binding to a continually sorted list of children in the current document view.
        /// </summary>
        public ReadOnlyObservableCollection<IDatabaseNodeViewModel> SortedChildren
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of queryable IKeePassNodes for search purposes.
        /// </summary>
        /// <param name="query">An optional query string to search against.</param>
        /// <returns>A collection of all IKeePassNodes (entries and groups) that are visible to searches.</returns>
        public ICollection<IKeePassNode> GetAllSearchableNodes(string query = null)
        {
            IList<IKeePassNode> items = new List<IKeePassNode>();
            AddToSearchCollection(Document.Root.DatabaseGroup, items, query);
            return items;
        }

        /// <summary>
        /// Attempts to delete the specified node from the document.
        /// </summary>
        /// <param name="node">The group to delete.</param>
        public async void DeleteNodeAndSave(IKeePassNode node)
        {
            IKeePassGroup parent = node.Parent;

            int originalIndex = parent.Children.IndexOf(node);

            // Temporarily remove the LeavesChanged handler as we will be manually updating SortedChildren here...
            this.NavigationViewModel.LeavesChanged -= this.OnNavigationViewModelLeavesChanged;
            parent.Children.RemoveAt(originalIndex);
            this.NavigationViewModel.LeavesChanged += this.OnNavigationViewModelLeavesChanged;

            int removalIndex;
            for (removalIndex = 0; removalIndex < this.sortedChildren.Count; removalIndex++)
            {
                if (this.sortedChildren[removalIndex].Node == node)
                {
                    break;
                }
            }

            Dbg.Assert(removalIndex != this.sortedChildren.Count, "It should only be possible to remove nodes from the current list of SortedChildren");
            this.sortedChildren.RemoveAt(removalIndex);

            await Save();
        }

        /// <summary>
        /// Attempts to rename the specified node in the document.
        /// </summary>
        /// <param name="node">The node to rename.</param>
        /// <param name="newName">The name to use.</param>
        public async void RenameNodeAndSave(IKeePassNode node, string newName)
        {
            string originalName = node.Title.ClearValue;
            node.Title.ClearValue = newName;

            await Save();
        }

        /// <summary>
        /// Creates an EntryDetailsViewModel for a new entry.
        /// </summary>
        /// <param name="parent">The group to use for the entry's parent.</param>
        /// <returns>An EntryDetailsViewModel for a new entry.</returns>
        public IEntryDetailsViewModel GetEntryDetailsViewModel(IKeePassGroup parent) =>
            new EntryDetailsViewModel(
                this.resourceProvider,
                this.NavigationViewModel,
                this.PersistenceService,
                this.clipboardService,
                this.Document,
                parent,
                this.rng
            );

        /// <summary>
        /// Creates an EntryDetailsViewModel for an existing entry.
        /// </summary>
        /// <param name="entry">The entry to open.</param>
        /// <param name="editing">Whether to open the entry in edit mode.</param>
        /// <returns>An EntryDetailsViewModel for an existing entry.</returns>
        public IEntryDetailsViewModel GetEntryDetailsViewModel(IKeePassEntry entry, bool editing) =>
            new EntryDetailsViewModel(
                this.resourceProvider,
                this.NavigationViewModel,
                this.PersistenceService,
                this.clipboardService,
                this.Document,
                entry,
                !editing,
                this.rng
            );

        /// <summary>
        /// Creates a GroupDetailsViewModel for a new group.
        /// </summary>
        /// <param name="parent">The group to use for the group's parent.</param>
        /// <returns>A GroupDetailsViewModel for a new group.</returns>
        public IGroupDetailsViewModel GetGroupDetailsViewModel(IKeePassGroup parent) =>
            new GroupDetailsViewModel(
                this.NavigationViewModel,
                this.PersistenceService,
                this.Document,
                parent
            );

        /// <summary>
        /// Creates a GroupDetailsViewModel for an existing group.
        /// </summary>
        /// <param name="group">The entry to open.</param>
        /// <param name="editing">Whether to open the group in edit mode.</param>
        /// <returns>A GroupDetailsViewModel for an existing group.</returns>
        public IGroupDetailsViewModel GetGroupDetailsViewModel(IKeePassGroup group, bool editing) =>
            new GroupDetailsViewModel(
                this.NavigationViewModel,
                this.PersistenceService,
                this.Document,
                group,
                !editing
            );

        /// <summary>
        /// Sorts the child list according to the current sort mode.
        /// </summary>
        /// <param name="searchQuery">If specified, returns all nodes (based on the query) instead of just the current children.</param>
        /// <returns>A sorted enumeration of nodes.</returns>
        private IOrderedEnumerable<IDatabaseNodeViewModel> GenerateSortedChildren(string searchQuery)
        {
            Dbg.Assert(this.NavigationViewModel != null);
            Dbg.Assert(this.NavigationViewModel.ActiveGroup != null);

            ICollection<IKeePassNode> baseNodeList = (String.IsNullOrEmpty(searchQuery) ?
                this.NavigationViewModel.ActiveGroup.Children :
                GetAllSearchableNodes(searchQuery));

            IEnumerable<IDatabaseNodeViewModel> nodeList =
                baseNodeList.Select(
                    node =>
                        (node is IKeePassEntry ?
                            GetViewModelForEntryNode((IKeePassEntry)node) :
                            GetViewModelForGroupNode((IKeePassGroup)node))
                );

            Dbg.Assert(nodeList != null);

            switch (this.SortMode.SortMode)
            {
                case DatabaseSortMode.Mode.DatabaseOrder:
                    return nodeList.OrderBy(node => node.Node, DatabaseViewModel.NodeComparer);
                case DatabaseSortMode.Mode.AlphabetAscending:
                    return nodeList.OrderBy(node => node.Node, DatabaseViewModel.NodeComparer)
                        .ThenBy(node => node.Node.Title);
                case DatabaseSortMode.Mode.AlphabetDescending:
                    return nodeList.OrderBy(node => node.Node, DatabaseViewModel.NodeComparer)
                        .ThenByDescending(node => node.Node.Title);
                default:
                    Dbg.Assert(false); // This should never happen
                    goto case DatabaseSortMode.Mode.DatabaseOrder;
            }
        }

        /// <summary>
        /// Wires up events for the "Request" commands on a node viewmodel.
        /// </summary>
        /// <param name="node">The new node.</param>
        private void WireUpEventsForNodeViewModel(IDatabaseNodeViewModel node)
        {
            Dbg.Assert(node != null);
            node.RenameRequested += (n, e) => { FireRequestRenameNode((IDatabaseNodeViewModel)n); };
            node.DeleteRequested += (n, e) => { FireRequestDeleteNode((IDatabaseNodeViewModel)n); };
            node.EditRequested += (n, e) => { FireRequestDetails((IDatabaseNodeViewModel)n); };
        }

        /// <summary>
        /// Creates a ViewModel to wrap a specific entry, and wires up its request events.
        /// </summary>
        /// <param name="entry">The entry to proxy.</param>
        /// <returns>A ViewModel proxying <paramref name="entry"/>.</returns>
        private DatabaseNodeViewModel GetViewModelForEntryNode(IKeePassEntry entry)
        {
            DatabaseEntryViewModel viewModel = new DatabaseEntryViewModel(entry, !this.PersistenceService.CanSave, this.clipboardService);
            WireUpEventsForNodeViewModel(viewModel);
            return viewModel;
        }

        /// <summary>
        /// Creates a ViewModel to wrap a specific group, and wires up its request events.
        /// </summary>
        /// <param name="group">The group to proxy.</param>
        /// <returns>A ViewModel proxying <paramref name="group"/>.</returns>
        private DatabaseNodeViewModel GetViewModelForGroupNode(IKeePassGroup group)
        {
            DatabaseGroupViewModel viewModel = new DatabaseGroupViewModel(group, !this.PersistenceService.CanSave);
            WireUpEventsForNodeViewModel(viewModel);
            viewModel.OpenRequested += (n, e) =>
            {
                IKeePassGroup groupToOpen = (IKeePassGroup)(((IDatabaseGroupViewModel)n).Node);
                this.NavigationViewModel.SetGroup(groupToOpen);
            };
            return viewModel;
        }

        /// <summary>
        /// Handles property changes on the NavigationViewModel.
        /// </summary>
        /// <param name="sender">The NavigationViewModel raising the event.</param>
        /// <param name="e">EventArgs for the property change.</param>
        private void OnNavigationViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveGroup")
            {
                UpdateActiveGroupView();
            }
        }

        /// <summary>
        /// Handles external leaf changes on the NavigationViewModel.
        /// </summary>
        /// <remarks>This can happen when a user creates a new node, which adds itself to the tree.</remarks>
        /// <param name="sender">The NavigationViewModel raising the event.</param>
        /// <param name="e">EventArgs for the change.</param>
        private void OnNavigationViewModelLeavesChanged(object sender, EventArgs e)
        {
            Dbg.Trace("Manually refreshing DBVM.SortedChildren as a result of NVM.LeavesChanged event");
            this.UpdateActiveGroupView();
        }

        /// <summary>
        /// Clears and updates the values of SortedChildren based on the current nav level.
        /// </summary>
        /// <remarks>If search is specified, all nodes in the tree are returned instead of the current level.</remarks>
        private void UpdateActiveGroupView()
        {
            IKeePassGroup activeNavGroup = this.NavigationViewModel.ActiveGroup;

            this.sortedChildren.Clear();
            foreach (IDatabaseNodeViewModel node in GenerateSortedChildren(this.Filter))
            {
                this.sortedChildren.Add(node);
            }

            if (this.activeGroup == null || !this.activeGroup.Uuid.Equals(this.NavigationViewModel.ActiveGroup.Uuid))
            {
                // Update the write-able local copy of ActiveGroup
                Stack<IKeePassGroup> pathToRoot = new Stack<IKeePassGroup>();
                this.activeGroup = this.Document.Root.DatabaseGroup;

                // First, compute a chain from the NavigationViewModel ActiveGroup up to the root
                while (activeNavGroup != this.activeGroup)
                {
                    // Climb up the Parent chain to the root
                    pathToRoot.Push(activeNavGroup);
                    activeNavGroup = activeNavGroup.Parent;
                }

                // Now pop from that new Stack until it's empty, finding the correct child (backtracking)
                // via UUID
                while (pathToRoot.Count > 0)
                {
                    IKeePassGroup nextLink = pathToRoot.Pop();
                    this.activeGroup = activeGroup.Children.First(g => g.Uuid.Equals(nextLink.Uuid)) as IKeePassGroup;
                    Dbg.Assert(this.activeGroup != null);
                }
            }
        }

        /// <summary>
        /// Recursive function for updating collecting a full listing of searchable document nodes.
        /// </summary>
        /// <param name="root">The root of the recursive probe.</param>
        /// <param name="soFar">The list of searchable nodes gathered so far.</param>
        /// <param name="query">The search string.</param>
        private void AddToSearchCollection(IKeePassGroup root, IList<IKeePassNode> soFar, string query)
        {
            if (soFar == null)
            {
                throw new ArgumentNullException(nameof(soFar));
            }

            if (root == null)
            {
                return;
            }

            // Keep in mind that all groups are searched. The search flag only applies to entries within a group.
            if (String.IsNullOrEmpty(query) || root.MatchesQuery(query))
            {
                soFar.Add(root);
            }

            bool searchEntries = root.IsSearchingPermitted();
            foreach(IKeePassNode node in root.Children)
            {
                // Recurse into child groups
                if (node is IKeePassGroup)
                {
                    AddToSearchCollection((IKeePassGroup)node, soFar, query);
                }
                else if (searchEntries && node is IKeePassEntry)
                {
                    if (String.IsNullOrEmpty(query) || node.MatchesQuery(query))
                    {
                        soFar.Add(node);
                    }
                }
            }
        }
    }
}
