using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Eventing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

        private IAppSettingsService settingsService;
        private IList<DatabaseSortMode> availableSortModes;
        private ObservableCollection<IKeePassGroup> sortedGroups;
        private ObservableCollection<IKeePassEntry> sortedEntries;
        private IKeePassGroup activeGroup;

        private DatabaseSortMode _sortMode;

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
            this.sortedGroups = new ObservableCollection<IKeePassGroup>();
            this.SortedGroups = new ReadOnlyObservableCollection<IKeePassGroup>(this.sortedGroups);

            this.sortedEntries = new ObservableCollection<IKeePassEntry>();
            this.SortedEntries = new ReadOnlyObservableCollection<IKeePassEntry>(this.sortedEntries);

            this.UpdateActiveGroupView();
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
        /// Allows binding to a continually sorted list of groups in the current document view.
        /// </summary>
        public ReadOnlyObservableCollection<IKeePassGroup> SortedGroups
        {
            get;
            private set;
        }

        /// <summary>
        /// Allows binding to a continually sorted list of entries in the current document view.
        /// </summary>
        public ReadOnlyObservableCollection<IKeePassEntry> SortedEntries
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
                this.sortedGroups.Remove(group);
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
                this.sortedEntries.Remove(entry);
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
        /// Clears and updates the values of SortedGroups and SortedEntries based on the current nav level.
        /// </summary>
        private void UpdateActiveGroupView()
        {
            // XXX: This function was designed with old behavior in mind - that a group would have collections
            // for Entries and Groups.
            // This has been worked around with LINQ for the time being, but the design should be revisited.

            IKeePassGroup activeNavGroup = this.NavigationViewModel.ActiveGroup;

            this.sortedGroups.Clear();
            IEnumerable<IKeePassGroup> newlySortedGroups = GetSortedNodes<IKeePassGroup>(
                new ObservableCollection<IKeePassGroup>(
                    activeNavGroup.Children.Where(node => node is IKeePassGroup).Cast<IKeePassGroup>()
                )
            );

            foreach(IKeePassGroup group in newlySortedGroups)
            {
                this.sortedGroups.Add(group);
            }

            this.sortedEntries.Clear();
            IEnumerable<IKeePassEntry> newlySortedEntries = GetSortedNodes<IKeePassEntry>(
                new ObservableCollection<IKeePassEntry>(
                    activeNavGroup.Children.Where(node => node is IKeePassEntry).Cast<IKeePassEntry>()
                )
            );

            foreach(IKeePassEntry entry in newlySortedEntries)
            {
                this.sortedEntries.Add(entry);
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
                    Debug.Assert(this.activeGroup != null);
                }
            }
        }

        /// <summary>
        /// Sorts a child list according to the current sort mode.
        /// </summary>
        /// <typeparam name="T">The type of child being sorted.</typeparam>
        /// <param name="nodeList">The child list to sort.</param>
        /// <returns>A sorted enumeration of nodes.</returns>
        private IEnumerable<T> GetSortedNodes<T>(ObservableCollection<T> nodeList)
            where T : IKeePassNode
        {
            switch (this.SortMode.SortMode)
            {
                case DatabaseSortMode.Mode.DatabaseOrder:
                    return nodeList;
                case DatabaseSortMode.Mode.AlphabetAscending:
                    return nodeList.OrderBy(node => node.Title);
                case DatabaseSortMode.Mode.AlphabetDescending:
                    return nodeList.OrderByDescending(node => node.Title);
                default:
                    Debug.Assert(false); // This should never happen
                    goto case DatabaseSortMode.Mode.DatabaseOrder;
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
