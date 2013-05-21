using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.ViewModels
{
    public abstract class DetailsViewModelBase<T> : ViewModelBase
    {
        private T _item;
        public T Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { SetProperty(ref _isReadOnly, value); }
        }

        public DatabaseViewModel DatabaseViewModel
        {
            get;
            set;
        }

        public abstract T GetBackup(out int index);

        public DetailsViewModelBase(DatabaseViewModel dbViewModel, ConfigurationViewModel settings)
            : base(settings)
        {
            DatabaseViewModel = dbViewModel;
        }

        public abstract Task<bool> Save();
        public abstract bool Revert();
    }
}
