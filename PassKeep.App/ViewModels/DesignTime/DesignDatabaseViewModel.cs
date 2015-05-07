using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.ViewModels;
using PassKeep.Models.DesignTime;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PassKeep.ViewModels.DesignTime
{
    /// <summary>
    /// A design-time ViewModel representing a decrypted database.
    /// </summary>
    public class DesignDatabaseViewModel : BindableBase, IDatabaseViewModel
    {
        public DesignDatabaseViewModel()
        {
            this.NavigationViewModel = new DatabaseNavigationViewModel();
            this.SortMode = new DatabaseSortMode(DatabaseSortMode.Mode.DatabaseOrder, "Database order");
            this.AvailableSortModes = new List<DatabaseSortMode>
            {
                this.SortMode
            };

            IKeePassGroup dbGroup = GetGroup("Database");
            IKeePassGroup subGroup = GetGroup("Subdirectory", dbGroup);
            dbGroup.Children.Add(subGroup);
            IKeePassGroup rootGroup = GetGroup("Current Root", subGroup);
            subGroup.Children.Add(rootGroup);

            rootGroup.Children.Add(GetGroup("Foo Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Bar Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Baz Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Some Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Some other node", rootGroup));
            rootGroup.Children.Add(GetGroup("Foo Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Bar Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Baz Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Some Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Some other node", rootGroup));
            rootGroup.Children.Add(GetGroup("Foo Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Bar Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Baz Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Some Directory", rootGroup));
            rootGroup.Children.Add(GetGroup("Some other node", rootGroup));
            rootGroup.Children.Add(GetEntry("Bank", "welcome", parent: rootGroup));
            rootGroup.Children.Add(GetEntry("Airline", "flymeout", "123456", "myairline.org", parent: rootGroup));
            rootGroup.Children.Add(GetEntry("Facebook", "aloha", parent: rootGroup));

            IKeePassEntry active = GetEntry("FooHub", "secure89", "Jimbo", "http://test.com/", parent: rootGroup);
            rootGroup.Children.Add(active);

            this.NavigationViewModel.SetEntry(active);
            this.SortedChildren = new ReadOnlyObservableCollection<IKeePassNode>(
                this.NavigationViewModel.ActiveGroup.Children
            );
        }

        public IDatabasePersistenceService PersistenceService
        {
            get;
            set;
        }

        public IDatabaseNavigationViewModel NavigationViewModel
        {
            get;
            set;
        }

        public KdbxDocument Document
        {
            get;
            set;
        }

        public IReadOnlyCollection<DatabaseSortMode> AvailableSortModes
        {
            get;
            set;
        }

        public DatabaseSortMode SortMode
        {
            get;
            set;
        }

        public ReadOnlyObservableCollection<IKeePassNode> SortedChildren
        {
            get;
            set;
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

        public MockGroup GetGroup(string name, IKeePassGroup parent = null)
        {
            return GetGroup(name, new KeePassUuid(), parent);
        }

        public MockGroup GetGroup(string name, KeePassUuid uuid, IKeePassGroup parent = null)
        {
            return new MockGroup
            {
                Title = new KdbxString("Title", name, null),
                Uuid = uuid,
                Parent = parent
            };
        }

        public MockEntry GetEntry(string title, string password, string user = "SomeUser", string url = "", string notes = "", IKeePassGroup parent = null)
        {
            return new MockEntry
            {
                Title = new KdbxString("Title", title, null),
                Password = new KdbxString("Password", password, new Salsa20(new byte[32]), true),
                UserName = new KdbxString("UserName", user, null),
                Url = new KdbxString("URL", url, null),
                Notes = new KdbxString("Notes", notes, null),
                Parent = parent
            };
        }

        public IEntryDetailsViewModel GetEntryDetailsViewModel(IKeePassGroup parent)
        {
            throw new NotImplementedException();
        }

        public IEntryDetailsViewModel GetEntryDetailsViewModel(IKeePassEntry entry, bool editing)
        {
            throw new NotImplementedException();
        }

        public IGroupDetailsViewModel GetGroupDetailsViewModel(IKeePassGroup parent)
        {
            throw new NotImplementedException();
        }

        public IGroupDetailsViewModel GetGroupDetailsViewModel(IKeePassGroup group, bool editing)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgClasses.CopyRequestedEventArgs> CopyRequested;

        public System.Windows.Input.ICommand RequestCopyUsernameCommand
        {
            get { throw new NotImplementedException(); }
        }

        public System.Windows.Input.ICommand RequestCopyPasswordCommand
        {
            get { throw new NotImplementedException(); }
        }
    }
}
