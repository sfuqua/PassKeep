using PassKeep.EventArgClasses;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
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
using System.Windows.Input;
using Windows.ApplicationModel.Resources;

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

        private IAppSettingsService settingsService;
        private IList<DatabaseSortMode> availableSortModes;
        private ObservableCollection<IKeePassNode> sortedChildren;
        private IKeePassGroup activeGroup;

        private DatabaseSortMode _sortMode;

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
        /// <param name="resourceLoader">The ResourceLoader used to load strings.</param>
        /// <param name="navigationViewModel">A ViewModel representing navigation state.</param>
        /// <param name="persistenceViewModel">A ViewModel used for persisting the document.</param>
        /// <param name="settingsService">The service used to access app settings.</param>
        public DatabaseViewModel(
            KdbxDocument document,
            ResourceLoader resourceLoader,
            IDatabaseNavigationViewModel navigationViewModel,
            IDatabasePersistenceService databasePersistenceService,
            IAppSettingsService settingsService
        ) : base(document, databasePersistenceService)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }

            if (navigationViewModel == null)
            {
                throw new ArgumentNullException("navigationViewModel");
            }

            if (settingsService == null)
            {
                throw new ArgumentNullException("settingsService");
            }

            this.Document = document;

            if (navigationViewModel.ActiveGroup == null)
            {
                navigationViewModel.SetGroup(this.Document.Root.DatabaseGroup);
            }
            this.NavigationViewModel = navigationViewModel;

            this.NavigationViewModel.PropertyChanged +=
                new WeakEventHandler<PropertyChangedEventArgs>(OnNavigationViewModelPropertyChanged).Handler;

            this.settingsService = settingsService;

            this.availableSortModes = new List<DatabaseSortMode>
            {
                new DatabaseSortMode(
                    DatabaseSortMode.Mode.DatabaseOrder,
                    resourceLoader.GetString(DatabaseViewModel.DatabaseOrderStringKey)
                ),
                new DatabaseSortMode(
                    DatabaseSortMode.Mode.AlphabetAscending,
                    resourceLoader.GetString(DatabaseViewModel.AlphabetOrderStringKey)
                ),
                new DatabaseSortMode(
                    DatabaseSortMode.Mode.AlphabetDescending,
                    resourceLoader.GetString(DatabaseViewModel.AlphabetOrderReverseStringKey)
                )
            };
            this.AvailableSortModes = new ReadOnlyCollection<DatabaseSortMode>(this.availableSortModes);

            // Default to DatabaseOrder.
            // Set the backing field directly since we don't want to trigger all the property logic
            // from the constructor.
            this._sortMode = this.availableSortModes[0];

            // Set up collections.
            this.sortedChildren = new ObservableCollection<IKeePassNode>();
            this.SortedChildren = new ReadOnlyObservableCollection<IKeePassNode>(this.sortedChildren);

            this.UpdateActiveGroupView();

            // Set up the copy commands.
            this.RequestCopyUsernameCommand = new TypedCommand<IKeePassEntry>(
                entry => { FireCopyRequested(entry, ClipboardTimerType.UserName); }
            );

            this.RequestCopyPasswordCommand = new TypedCommand<IKeePassEntry>(
                entry => { FireCopyRequested(entry, ClipboardTimerType.Password); }
            );
        }

        /// <summary>
        /// Fired when the user requests to copy credentials (username or password).
        /// </summary>
        public event EventHandler<CopyRequestedEventArgs> CopyRequested;
        
        /// <summary>
        /// Fires the CopyRequested event.
        /// </summary>
        /// <param name="entry">The entry whose data is being copied.</param>
        /// <param name="type">The type of copy requested.</param>
        private void FireCopyRequested(IKeePassEntry entry, ClipboardTimerType type)
        {
            if (CopyRequested != null)
            {
                CopyRequested(this, new CopyRequestedEventArgs(entry, type));
            }
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
                    throw new ArgumentException("Unknown sort mode!", "value");
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
        /// Allows binding to a continually sorted list of children in the current document view.
        /// </summary>
        public ReadOnlyObservableCollection<IKeePassNode> SortedChildren
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of queryable IKeePassNodes for search purposes.
        /// </summary>
        /// <returns>A collection of all IKeePassNodes (entries and groups) that are visible to searches.</returns>
        public ICollection<IKeePassNode> GetAllSearchableNodes()
        {
            IList<IKeePassNode> items = new List<IKeePassNode>();
            AddToSearchCollection(Document.Root.DatabaseGroup, items);
            return items;
        }

        /// <summary>
        /// Attempts to delete the specified group from the document.
        /// </summary>
        /// <param name="group">The group to delete.</param>
        public async void DeleteGroupAndSave(IKeePassGroup group)
        {
            if (group.Parent != this.activeGroup)
            {
                throw new ArgumentException("Cannot delete this group; it is not a child of the ActiveGroup", "group");
            }

            int originalIndex = this.activeGroup.Children.IndexOf(group);
            this.activeGroup.Children.RemoveAt(originalIndex);

            if (!await this.TrySave())
            {
                // If the save did not succeed, at the group back
                this.activeGroup.Children.Insert(originalIndex, group);
            }
            else
            {
                this.sortedChildren.Remove(group);
            }
        }

        /// <summary>
        /// Attempts to delete the specified entry from the document.
        /// </summary>
        /// <param name="entry">The entry to delete.</param>
        public async void DeleteEntryAndSave(IKeePassEntry entry)
        {
            if (entry.Parent != this.activeGroup)
            {
                throw new ArgumentException("Cannot delete this entry; it is not a child of the ActiveGroup", "entry");
            }

            int originalIndex = this.activeGroup.Children.IndexOf(entry);
            this.activeGroup.Children.RemoveAt(originalIndex);

            if (!await this.TrySave())
            {
                // If the save did not succeed, at the group back
                this.activeGroup.Children.Insert(originalIndex, entry);
            }
            else
            {
                this.sortedChildren.Remove(entry);
            }
        }

        /// <summary>
        /// Sorts the child list according to the current sort mode.
        /// </summary>
        /// <returns>A sorted enumeration of nodes.</returns>
        private IOrderedEnumerable<IKeePassNode> GenerateSortedChildren()
        {
            Dbg.Assert(this.NavigationViewModel != null);
            Dbg.Assert(this.NavigationViewModel.ActiveGroup != null);
            IEnumerable<IKeePassNode> nodeList = this.NavigationViewModel.ActiveGroup.Children;
            Dbg.Assert(nodeList != null);

            switch (this.SortMode.SortMode)
            {
                case DatabaseSortMode.Mode.DatabaseOrder:
                    return nodeList.OrderBy(node => node, DatabaseViewModel.NodeComparer);
                case DatabaseSortMode.Mode.AlphabetAscending:
                    return nodeList.OrderBy(node => node, DatabaseViewModel.NodeComparer)
                        .ThenBy(node => node.Title);
                case DatabaseSortMode.Mode.AlphabetDescending:
                    return nodeList.OrderBy(node => node, DatabaseViewModel.NodeComparer)
                        .ThenByDescending(node => node.Title);
                default:
                    Dbg.Assert(false); // This should never happen
                    goto case DatabaseSortMode.Mode.DatabaseOrder;
            }
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
        /// Clears and updates the values of SortedChildren based on the current nav level.
        /// </summary>
        private void UpdateActiveGroupView()
        {
            IKeePassGroup activeNavGroup = this.NavigationViewModel.ActiveGroup;

            this.sortedChildren.Clear();
            foreach (IKeePassNode node in GenerateSortedChildren())
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
        private void AddToSearchCollection(IKeePassGroup root, IList<IKeePassNode> soFar)
        {
            if (soFar == null)
            {
                throw new ArgumentNullException("soFar");
            }

            if (root == null)
            {
                return;
            }

            // Keep in mind that all groups are searched. The search flag only applies to entries within a group.
            soFar.Add(root);

            bool searchEntries = root.IsSearchingPermitted();
            foreach(IKeePassNode node in root.Children)
            {
                // Recurse into child groups
                if (node is IKeePassGroup)
                {
                    AddToSearchCollection((IKeePassGroup)node, soFar);
                }
                else if (searchEntries && node is IKeePassEntry)
                {
                    soFar.Add(node);
                }
            }
        }
    }
}
