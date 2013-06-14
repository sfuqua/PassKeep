using System.Collections.ObjectModel;
using PassKeep.Models.Abstraction;

namespace PassKeep.ViewModels
{
    public class DatabaseNavigationViewModel : ViewModelBase
    {
        private ObservableCollection<IKeePassGroup> _breadcrumbs;
        public ObservableCollection<IKeePassGroup> Breadcrumbs
        {
            get { return _breadcrumbs; }
            set { SetProperty(ref _breadcrumbs, value); }
        }

        private ObservableCollection<IKeePassNode> _leaves;
        public ObservableCollection<IKeePassNode> Leaves
        {
            get { return _leaves; }
            set { SetProperty(ref _leaves, value); }
        }

        public IKeePassGroup ActiveGroup
        {
            get
            {
                if (Breadcrumbs == null || Breadcrumbs.Count == 0)
                {
                    return null;
                }
                return Breadcrumbs[Breadcrumbs.Count - 1];
            }
        }

        private IKeePassEntry _activeLeaf;
        public IKeePassEntry ActiveLeaf
        {
            get { return _activeLeaf; }
            private set { SetProperty(ref _activeLeaf, value); }
        }

        public DatabaseNavigationViewModel(ConfigurationViewModel appSettings)
            : base(appSettings)
        {
            Breadcrumbs = new ObservableCollection<IKeePassGroup>();
            Leaves = new ObservableCollection<IKeePassNode>();
        }

        public DatabaseNavigationViewModel(IKeePassEntry lastEntry, ConfigurationViewModel appSettings)
            : this(appSettings)
        {
            SetEntry(lastEntry);
        }

        public DatabaseNavigationViewModel(IKeePassGroup lastGroup, ConfigurationViewModel appSettings)
            : this(appSettings)
        {
            SetGroup(lastGroup);
        }

        /// <summary>
        /// Reloads the list of leaves in the tree from the last Breadcrumb Group.
        /// </summary>
        public void RefreshLeaves()
        {
            Leaves = new ObservableCollection<IKeePassNode>();
            ActiveLeaf = null;

            if (Breadcrumbs.Count == 0)
            {
                return;
            }

            IKeePassGroup lastGroup = ActiveGroup;
            foreach (IKeePassNode group in lastGroup.Groups)
            {
                Leaves.Add(group);
            }

            foreach (IKeePassNode entry in lastGroup.Entries)
            {
                Leaves.Add(entry);
            }
        }

        /// <summary>
        /// Clears out the ActiveLeaf.
        /// </summary>
        public void Prune()
        {
            ActiveLeaf = null;
        }

        /// <summary>
        /// Sets the last Breadcrumb to an Entry's parent, resets the leaves, and flags the Entry as active.
        /// </summary>
        /// <param name="newActiveEntry"></param>
        public void SetEntry(IKeePassEntry entry)
        {
            if (entry == null)
            {
                SetGroup(null);
            }
            else
            {
                SetGroup(entry.Parent);
            }

            ActiveLeaf = entry;
        }

        /// <summary>
        /// Updates the Breadcrumb list and resets all the leaves (including the active one).
        /// </summary>
        /// <param name="node"></param>
        public void SetGroup(IKeePassGroup group)
        {
            if (group == null)
            {
                Breadcrumbs.Clear();
                RefreshLeaves();
                return;
            }

            if (group.Equals(ActiveGroup))
            {
                return;
            }

            if (group.Parent == null || Breadcrumbs.Count == 0 || !group.Parent.Equals(ActiveGroup))
            {
                // Either: 
                // * The Group has no parent
                // * There are no breadcrumbs
                // * The Group is not a child of the last Group
                Breadcrumbs.Clear();

                while (group != null)
                {
                    Breadcrumbs.Insert(0, group);
                    group = group.Parent;
                }
            }
            else
            {
                // The Group is a direct child of the last Group
                Breadcrumbs.Add(group);
            }

            RefreshLeaves();
        }
    }
}
