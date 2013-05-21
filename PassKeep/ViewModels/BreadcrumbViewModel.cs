using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassKeep.Models;

namespace PassKeep.ViewModels
{
    public class BreadcrumbViewModel : ViewModelBase
    {
        private ObservableCollection<KdbxGroup> _breadcrumbs;
        public ObservableCollection<KdbxGroup> Breadcrumbs
        {
            get { return _breadcrumbs; }
            set { SetProperty(ref _breadcrumbs, value); }
        }

        public BreadcrumbViewModel(ConfigurationViewModel appSettings)
            : base(appSettings)
        {
            Breadcrumbs = new ObservableCollection<KdbxGroup>();
        }

        public BreadcrumbViewModel(KdbxEntry lastEntry, ConfigurationViewModel appSettings)
            : this(appSettings)
        {
            SetEntry(lastEntry);
        }

        public BreadcrumbViewModel(KdbxGroup lastGroup, ConfigurationViewModel appSettings)
            : this(appSettings)
        {
            SetGroup(lastGroup);
        }

        public void SetEntry(KdbxEntry entry)
        {
            Breadcrumbs.Clear();

            KdbxGroup root = entry.Parent;
            while (root != null)
            {
                Breadcrumbs.Insert(0, root);
                root = root.Parent;
            }
        }

        public void SetGroup(KdbxGroup group)
        {
            Breadcrumbs.Clear();

            while (group != null)
            {
                Breadcrumbs.Insert(0, group);
                group = group.Parent;
            }
        }
    }
}
