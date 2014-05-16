using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.Models;
using PassKeep.Lib.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PassKeep.ViewModels.Design
{
    public class DatabaseDesignViewModel : IDatabaseViewModel
    {
        public DatabaseDesignViewModel()
        {
            this.AvailableSortModes = new List<DatabaseSortMode> {
                new DatabaseSortMode(DatabaseSortMode.Mode.DatabaseOrder, "Database Order")
            };

            this.SortMode = this.AvailableSortModes.First();
            this.NavigationViewModel = new DatabaseNavigationViewModel();

            var dbGroup = getGroup("Database");
            var subGroup = getGroup("Subdirectory");
            dbGroup.AddChild(subGroup);
            var rootGroup = getGroup("Current Root");
            subGroup.AddChild(rootGroup);

            rootGroup.AddChild(getGroup("Foo Directory"));
            rootGroup.AddChild(getGroup("Bar Directory"));
            rootGroup.AddChild(getGroup("Baz Directory"));
            rootGroup.AddChild(getGroup("Some Directory"));
            rootGroup.AddChild(getGroup("Some other node"));
            rootGroup.AddChild(getGroup("Foo Directory"));
            rootGroup.AddChild(getGroup("Bar Directory"));
            rootGroup.AddChild(getGroup("Baz Directory"));
            rootGroup.AddChild(getGroup("Some Directory"));
            rootGroup.AddChild(getGroup("Some other node"));
            rootGroup.AddChild(getGroup("Foo Directory"));
            rootGroup.AddChild(getGroup("Bar Directory"));
            rootGroup.AddChild(getGroup("Baz Directory"));
            rootGroup.AddChild(getGroup("Some Directory"));
            rootGroup.AddChild(getGroup("Some other node"));
            rootGroup.AddChild(getEntry("Bank", "welcome"));
            rootGroup.AddChild(getEntry("Airline", "flymeout", "123456", "myairline.org"));
            rootGroup.AddChild(getEntry("Facebook", "aloha"));

            var active = getEntry("FooHub", "secure89", "Jimbo", "http://test.com/");
            rootGroup.AddChild(active);

            this.NavigationViewModel.SetEntry(active);
        }

        public Group getGroup(string name)
        {
            return getGroup(name, new KeePassUuid());
        }

        public Group getGroup(string name, KeePassUuid uuid)
        {
            return new DatabaseDesignViewModel.Group
            {
                Title = new KdbxString("Title", name, null),
                Uuid = uuid
            };
        }

        public Entry getEntry(string title, string password, string user="SomeUser", string url="", string notes="")
        {
            return new DatabaseDesignViewModel.Entry
            {
                Title = new KdbxString("Title", title, null),
                Password = new KdbxString("Password", password, new Salsa20(new byte[32]), true),
                UserName = new KdbxString("UserName", user, null),
                Url = new KdbxString("URL", url, null),
                Notes = new KdbxString("Notes", notes, null)
            };
        }

        #region Helper classes

        public class Group : IKeePassGroup
        {
            public Group()
            {
                Groups = new ObservableCollection<IKeePassGroup>();
                Entries = new ObservableCollection<IKeePassEntry>();
                Children = new ObservableCollection<IKeePassNode>();
            }

            public bool HasAncestor(IKeePassGroup group)
            {
                if (group == null)
                {
                    throw new ArgumentNullException("group");
                }

                IKeePassGroup currentAncestor = Parent;
                while (currentAncestor != null)
                {
                    if (currentAncestor.Uuid.Equals(group.Uuid))
                    {
                        return true;
                    }

                    currentAncestor = currentAncestor.Parent;
                }

                return false;
            }

            public bool HasDescendant(IKeePassNode node)
            {
                if (node == null)
                {
                    throw new ArgumentNullException("node");
                }

                foreach (IKeePassEntry entry in Entries)
                {
                    if (entry.Uuid.Equals(node.Uuid))
                    {
                        return true;
                    }
                }

                foreach (IKeePassGroup group in Groups)
                {
                    if (group.HasDescendant(node))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void AddChild(Group group)
            {
                group.Parent = this;
                Groups.Add(group);
                Children.Add(group);
            }

            public void AddChild(Entry entry)
            {
                entry.Parent = this;
                Entries.Add(entry);
                Children.Add(entry);
            }

            public ObservableCollection<IKeePassNode> Children
            {
                get;
                set;
            }

            public ObservableCollection<IKeePassGroup> Groups
            {
                get;
                set;
            }

            public ObservableCollection<IKeePassEntry> Entries
            {
                get;
                set;
            }

            public KeePassUuid Uuid
            {
                get;
                set;
            }

            public IProtectedString Title
            {
                get;
                set;
            }

            public IProtectedString Notes
            {
                get;
                set;
            }

            public IKeePassGroup Parent
            {
                get;
                set;
            }

            public bool? EnableSearching
            {
                get;
                set;
            }

            public bool MatchesQuery(string query)
            {
                throw new NotImplementedException();
            }

            public XElement ToXml(IRandomNumberGenerator rng)
            {
                throw new NotImplementedException();
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public bool IsExpanded
            {
                get { throw new NotImplementedException(); }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public string DefaultAutoTypeSequence
            {
                get { throw new NotImplementedException(); }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public bool? EnableAutoType
            {
                get { throw new NotImplementedException(); }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public KeePassUuid LastTopVisibleEntry
            {
                get { throw new NotImplementedException(); }
            }

            public void SyncTo(IKeePassGroup template, bool updateModificationTime = true)
            {
                throw new NotImplementedException();
            }

            public IKeePassGroup Clone()
            {
                throw new NotImplementedException();
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public int IconID
            {
                get { return KdbxGroup.DefaultIconId; }
            }


            public KeePassUuid CustomIconUuid
            {
                get { return null; }
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public KdbxTimes Times
            {
                get { throw new NotImplementedException(); }
            }
        }

        public class Entry : IKeePassEntry
        {
            public IProtectedString Password
            {
                get;
                set;
            }

            public IProtectedString Url
            {
                get;
                set;
            }

            public IProtectedString UserName
            {
                get;
                set;
            }

            public string OverrideUrl
            {
                get;
                set;
            }

            public KeePassUuid Uuid
            {
                get;
                set;
            }

            public IProtectedString Title
            {
                get;
                set;
            }

            public IProtectedString Notes
            {
                get;
                set;
            }

            public IKeePassGroup Parent
            {
                get;
                set;
            }

            public bool HasAncestor(IKeePassGroup group)
            {
                if (group == null)
                {
                    throw new ArgumentNullException("group");
                }

                IKeePassGroup currentAncestor = Parent;
                while (currentAncestor != null)
                {
                    if (currentAncestor.Uuid.Equals(group.Uuid))
                    {
                        return true;
                    }

                    currentAncestor = currentAncestor.Parent;
                }

                return false;
            }

            public bool MatchesQuery(string query)
            {
                throw new NotImplementedException();
            }

            public XElement ToXml(IRandomNumberGenerator rng)
            {
                throw new NotImplementedException();
            }


            public IKeePassEntry Clone(bool preserveHistory = true)
            {
                throw new NotImplementedException();
            }


            public int IconID
            {
                get { return KdbxEntry.DefaultIconId; }
            }


            public void Update(IKeePassEntry template)
            {
                throw new NotImplementedException();
            }


            public Windows.UI.Color? ForegroundColor
            {
                get { return null; }
            }

            public Windows.UI.Color? BackgroundColor
            {
                get { return null; }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public KdbxAutoType AutoType
            {
                get { throw new NotImplementedException(); }
            }


            public string Tags
            {
                get;
                set;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public ObservableCollection<IProtectedString> Fields
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


            public KeePassUuid CustomIconUuid
            {
                get { return null; }
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public ObservableCollection<KdbxBinary> Binaries
            {
                get { throw new NotImplementedException(); }
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public KdbxTimes Times
            {
                get { throw new NotImplementedException(); }
            }
        }

        #endregion

        public IDatabaseNavigationViewModel NavigationViewModel
        {
            get;
            private set;
        }

        public Lib.KeePass.Dom.KdbxDocument Document
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyCollection<DatabaseSortMode> AvailableSortModes
        {
            get;
            private set;
        }

        public DatabaseSortMode SortMode
        {
            get;
            set;
        }

        public ReadOnlyObservableCollection<IKeePassGroup> SortedGroups
        {
            get
            {
                return new ReadOnlyObservableCollection<IKeePassGroup>(this.NavigationViewModel.ActiveGroup.Groups);
            }
        }

        public ReadOnlyObservableCollection<IKeePassEntry> SortedEntries
        {
            get
            {
                return new ReadOnlyObservableCollection<IKeePassEntry>(this.NavigationViewModel.ActiveGroup.Entries);
            }
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
