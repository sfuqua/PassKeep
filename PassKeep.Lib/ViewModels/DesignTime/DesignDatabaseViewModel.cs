// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
using Windows.Foundation;

namespace PassKeep.ViewModels.DesignTime
{
    /// <summary>
    /// A design-time ViewModel representing a decrypted database.
    /// </summary>
    public class DesignDatabaseViewModel : AbstractViewModel, IDatabaseViewModel
    {
        public DesignDatabaseViewModel()
        {
            NavigationViewModel = new DatabaseNavigationViewModel();
            SortMode = new DatabaseSortMode(DatabaseSortMode.Mode.DatabaseOrder, "Database order");
            AvailableSortModes = new List<DatabaseSortMode>
            {
                SortMode
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

            NavigationViewModel.SetEntry(active);
            SortedChildren = new ReadOnlyObservableCollection<IDatabaseNodeViewModel>(
                new ObservableCollection<IDatabaseNodeViewModel>(
                    NavigationViewModel.ActiveGroup.Children
                        .Select(node => new DatabaseNodeViewModel(node, false)))
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

        public ReadOnlyObservableCollection<IDatabaseNodeViewModel> SortedChildren
        {
            get;
            set;
        }

        public ICollection<IKeePassNode> GetAllSearchableNodes()
        {
            throw new NotImplementedException();
        }

        public void DeleteNodeAndSave(IKeePassNode node)
        {
            throw new NotImplementedException();
        }

        public Task Save()
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

        public ICollection<IKeePassNode> GetAllSearchableNodes(string query)
        {
            throw new NotImplementedException();
        }

        public void RenameNodeAndSave(IKeePassNode node, string newName)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgClasses.CopyRequestedEventArgs> CopyRequested
        {
            add { }
            remove { }
        }
        public event TypedEventHandler<IDatabaseViewModel, IDatabaseNodeViewModel> RequestRenameNode
        {
            add { }
            remove { }
        }
        public event TypedEventHandler<IDatabaseViewModel, IDatabaseNodeViewModel> RequestDeleteNode
        {
            add { }
            remove { }
        }
        public event TypedEventHandler<IDatabaseViewModel, IDatabaseNodeViewModel> RequestDetails
        {
            add { }
            remove { }
        }

        public System.Windows.Input.ICommand RequestCopyUsernameCommand
        {
            get { throw new NotImplementedException(); }
        }

        public System.Windows.Input.ICommand RequestCopyPasswordCommand
        {
            get { throw new NotImplementedException(); }
        }

        public string Filter
        {
            get
            {
                return String.Empty;
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
