using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Models;
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
            dbGroup.Groups.Add(subGroup);
            IKeePassGroup rootGroup = GetGroup("Current Root", subGroup);
            subGroup.Groups.Add(rootGroup);

            rootGroup.Groups.Add(GetGroup("Foo Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Bar Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Baz Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Some Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Some other node", rootGroup));
            rootGroup.Groups.Add(GetGroup("Foo Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Bar Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Baz Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Some Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Some other node", rootGroup));
            rootGroup.Groups.Add(GetGroup("Foo Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Bar Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Baz Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Some Directory", rootGroup));
            rootGroup.Groups.Add(GetGroup("Some other node", rootGroup));
            rootGroup.Entries.Add(GetEntry("Bank", "welcome", parent: rootGroup));
            rootGroup.Entries.Add(GetEntry("Airline", "flymeout", "123456", "myairline.org", parent: rootGroup));
            rootGroup.Entries.Add(GetEntry("Facebook", "aloha", parent: rootGroup));

            IKeePassEntry active = GetEntry("FooHub", "secure89", "Jimbo", "http://test.com/", parent: rootGroup);
            rootGroup.Entries.Add(active);

            this.NavigationViewModel.SetEntry(active);

            this.SortedEntries = new ReadOnlyObservableCollection<IKeePassEntry>(this.NavigationViewModel.ActiveGroup.Entries);
            this.SortedGroups = new ReadOnlyObservableCollection<IKeePassGroup>(this.NavigationViewModel.ActiveGroup.Groups);
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

        public ReadOnlyObservableCollection<IKeePassGroup> SortedGroups
        {
            get;
            set;
        }

        public ReadOnlyObservableCollection<IKeePassEntry> SortedEntries
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
    }
}
