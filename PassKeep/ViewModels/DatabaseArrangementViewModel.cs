using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using PassKeep.Common;
using PassKeep.Models.Abstraction;
using PassKeep.ViewModels.Converters;

namespace PassKeep.ViewModels
{
    public class DatabaseArrangementViewModel : ViewModelBase
    {
        private DatabaseSortMode _sortMode;
        public DatabaseSortMode SortMode
        {
            get { return _sortMode; }
            set
            {
                if (SetProperty(ref _sortMode, value))
                {
                    Settings.DatabaseSortMode = value.SortMode;
                }
            }
        }

        private List<DatabaseSortMode> _availableSortModes;
        public List<DatabaseSortMode> AvailableSortModes
        {
            get
            {
                if (_availableSortModes == null)
                {
                    _availableSortModes = new List<DatabaseSortMode>
                    {
                        new DatabaseSortMode(DatabaseSortMode.Mode.DatabaseOrder, "Database Order"),
                        new DatabaseSortMode(DatabaseSortMode.Mode.AlphabetAscending, "Alphabet (a-z)"),
                        new DatabaseSortMode(DatabaseSortMode.Mode.AlphabetDescending, "Alphabet (z-a)")
                    };
                }
                return _availableSortModes;
            }
        }

        private NodeTypeComparer nodeTypeComparer;

        public DatabaseArrangementViewModel(ConfigurationViewModel appSettings)
            : base(appSettings)
        {
            for (int i = 0; i < AvailableSortModes.Count; i++)
            {
                if (AvailableSortModes[i].SortMode == appSettings.DatabaseSortMode)
                {
                    SortMode = AvailableSortModes[i];
                    break;
                }
            }
            nodeTypeComparer = new NodeTypeComparer();
        }

        /// <summary>
        /// Sorts and returns the children of the specified group based on the current arrangement mode.
        /// </summary>
        /// <param name="currentGroup">The group whose children to query.</param>
        /// <returns>A list filtered and sorted based on the current mode.</returns>
        public void ArrangeChildren(IKeePassGroup currentGroup, ref CollectionViewSource viewSource)
        {
            Debug.Assert(currentGroup != null);
            if (currentGroup == null)
            {
                throw new ArgumentNullException("currentGroup");
            }

            Debug.Assert(viewSource != null);
            if (viewSource == null)
            {
                throw new ArgumentNullException("viewSource");
            }

            Debug.Assert(currentGroup.Children != null);
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
                    Debug.Assert(false);
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

    public class DatabaseSortMode : BindableBase
    {
        public enum Mode : int
        {
            DatabaseOrder = 0,
            AlphabetAscending,
            AlphabetDescending
        }

        private Mode _sortMode;
        public Mode SortMode
        {
            get { return _sortMode; }
            set { SetProperty(ref _sortMode, value); }
        }

        private string _friendlyName;
        public string FriendlyName
        {
            get { return _friendlyName; }
            set { SetProperty(ref _friendlyName, value); }
        }

        public DatabaseSortMode(Mode mode, string name)
        {
            SortMode = mode;
            FriendlyName = name;
        }
    }
}
