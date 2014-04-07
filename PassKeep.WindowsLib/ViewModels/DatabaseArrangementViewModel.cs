using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace PassKeep.Lib.ViewModels
{
    public class DatabaseArrangementViewModel : BindableBase, IDatabaseArrangementViewModel
    {
        private NodeTypeComparer nodeTypeComparer;
        private ObservableCollection<DatabaseSortMode> availableSortModes;
        private IAppSettingsService settingsService;

        private DatabaseSortMode _sortMode;
        private ReadOnlyObservableCollection<DatabaseSortMode> _availableSortModes;

        public DatabaseArrangementViewModel(IAppSettingsService settingsService)
        {
            this.nodeTypeComparer = new NodeTypeComparer();

            this.availableSortModes = new ObservableCollection<DatabaseSortMode>
            {
                new DatabaseSortMode(DatabaseSortMode.Mode.DatabaseOrder, "Database Order"),
                new DatabaseSortMode(DatabaseSortMode.Mode.AlphabetAscending, "Alphabet (a-z)"),
                new DatabaseSortMode(DatabaseSortMode.Mode.AlphabetDescending, "Alphabet (z-a)")
            };
            this._availableSortModes = new ReadOnlyObservableCollection<DatabaseSortMode>(this.availableSortModes);

            // Default to DatabaseOrder.
            this.SortMode = this.availableSortModes[0];
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

                if (SetProperty(ref this._sortMode, value))
                {
                    this.settingsService.DatabaseSortMode = value.SortMode;
                }
            }
        }

        /// <summary>
        /// A listing of all known, available sort modes.
        /// </summary>
        public ReadOnlyObservableCollection<DatabaseSortMode> AvailableSortModes
        {
            get { return this._availableSortModes; }
        }

        /// <summary>
        /// Sorts and returns the children of the specified group based on the current arrangement mode.
        /// </summary>
        /// <param name="currentGroup">The group whose children to query.</param>
        /// <param name="viewSource">A CollectionViewSource to populate as a result of the arrangement.</param>
        /// <returns>A list filtered and sorted based on the current mode.</returns>
        public void ArrangeChildren(IKeePassGroup currentGroup, ref CollectionViewSource viewSource)
        {
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            if (viewSource == null)
            {
                throw new ArgumentNullException("viewSource");
            }

            if (currentGroup.Children == null)
            {
                throw new InvalidOperationException();
            }

            object newSource;
            switch (SortMode.SortMode)
            {
                case DatabaseSortMode.Mode.DatabaseOrder:
                    newSource = currentGroup.Children;
                    break;
                case DatabaseSortMode.Mode.AlphabetAscending:
                    newSource = currentGroup.Children.OrderBy(node => node.Title.ClearValue).OrderBy(node => node, nodeTypeComparer);
                    break;
                case DatabaseSortMode.Mode.AlphabetDescending:
                    newSource = currentGroup.Children.OrderByDescending(node => node.Title.ClearValue).OrderBy(node => node, nodeTypeComparer);
                    break;
                default:
                    Debug.Assert(false); // This should never happen
                    goto case DatabaseSortMode.Mode.DatabaseOrder;
            }

            viewSource.Source = newSource;
        }

        /// <summary>
        /// Simple IComparer that sorts Nodes based on type (Entry, Group).
        /// </summary>
        private class NodeTypeComparer : IComparer<IKeePassNode>
        {
            /// <summary>
            /// Compares two Nodes based on type.
            /// Groups precede Entries.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(IKeePassNode x, IKeePassNode y)
            {
                if (x is IKeePassGroup)
                {
                    return (y is IKeePassGroup ? 0 : -1);
                }

                // x is IKeePassEntry
                return (y is IKeePassGroup ? 1 : 0);
            }
        }
    }
}
