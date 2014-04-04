using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.ViewModels
{
    /// <summary>
    /// An abstraction representing a detailed view of a KeePassNode, such as an Entry or Group.
    /// </summary>
    /// <typeparam name="T">The type of Node.</typeparam>
    public abstract class DetailsViewModelBase<T> : ViewModelBase, INodeDetailsViewModel<T>
        where T : IKeePassNode
    {
        /// <summary>
        /// Allows access to the Node represented by this ViewModel.
        /// </summary>
        private T _item;
        public T Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }

        /// <summary>
        /// Whether or not editing is enabled for the View.
        /// </summary>
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
