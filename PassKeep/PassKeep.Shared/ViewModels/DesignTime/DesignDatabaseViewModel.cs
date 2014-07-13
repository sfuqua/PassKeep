using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PassKeep.ViewModels.DesignTime
{
    /// <summary>
    /// A design-time ViewModel representing a decrypted database.
    /// </summary>
    public class DesignDatabaseViewModel : BindableBase, IDatabaseViewModel
    {
        public IDatabaseNavigationViewModel NavigationViewModel
        {
            get { throw new NotImplementedException(); }
        }

        public KdbxDocument Document
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyCollection<DatabaseSortMode> AvailableSortModes
        {
            get { throw new NotImplementedException(); }
        }

        public DatabaseSortMode SortMode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ReadOnlyObservableCollection<IKeePassGroup> SortedGroups
        {
            get { throw new NotImplementedException(); }
        }

        public ReadOnlyObservableCollection<IKeePassEntry> SortedEntries
        {
            get { throw new NotImplementedException(); }
        }

        public ICollection<IKeePassNode> GetAllSearchableNodes()
        {
            throw new NotImplementedException();
        }

        public void DeleteGroupAndSave(IKeePassGroup group)
        {
            throw new NotImplementedException();
        }

        public void DeleteEntryAndSave(IKeePassEntry entry)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<CancellableEventArgs> StartedSave;

        public event EventHandler StoppedSave;

        public Task<bool> TrySave()
        {
            throw new NotImplementedException();
        }
    }
}
