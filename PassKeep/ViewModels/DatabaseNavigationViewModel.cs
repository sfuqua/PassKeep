using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PassKeep.Models.Abstraction;

namespace PassKeep.ViewModels
{
    public sealed class DatabaseNavigationViewModel : ViewModelBase
    {
        private ObservableCollection<IKeePassGroup> _breadcrumbs;
        public ObservableCollection<IKeePassGroup> Breadcrumbs
        {
            get { return _breadcrumbs; }
            private set { SetProperty(ref _breadcrumbs, value); }
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
            // Is this a no-op?
            if (group == ActiveGroup)
            {
                return;
            }

            // Are we clearing everything?
            if (group == null)
            {
                Breadcrumbs.Clear();
            }
            else
            {
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
            }

            OnPropertyChanged("ActiveGroup");
        }
    }
}
