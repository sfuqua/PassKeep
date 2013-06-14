using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using PassKeep.Common;
using PassKeep.KeePassLib;
using PassKeep.Models;
using PassKeep.Models.Abstraction;

namespace PassKeep.ViewModels.Design
{
    public class DatabaseDesignViewModel : BindableBase
    {
        private DatabaseNavigationViewModel _breadcrumbViewModel;
        public DatabaseNavigationViewModel BreadcrumbViewModel
        {
            get { return _breadcrumbViewModel; }
            set { SetProperty(ref _breadcrumbViewModel, value); }
        }

        public DatabaseDesignViewModel()
        {
            BreadcrumbViewModel = new DatabaseNavigationViewModel(appSettings: null);

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

            BreadcrumbViewModel.SetEntry(active);
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
                Password = new KdbxString("Password", password, new Salsa20Rng(new byte[32]), true),
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
            }

            public void AddChild(Group group)
            {
                group.Parent = this;
                Groups.Add(group);
            }

            public void AddChild(Entry entry)
            {
                entry.Parent = this;
                Entries.Add(entry);
            }

            public IList<IKeePassGroup> Groups
            {
                get;
                set;
            }

            public IList<IKeePassEntry> Entries
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

            public XElement ToXml(KeePassRng rng)
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

            public void Update(IKeePassGroup template, bool updateModificationTime = true)
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
                get { throw new NotImplementedException(); }
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public KeePassUuid CustomIconUuid
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

            public bool MatchesQuery(string query)
            {
                throw new NotImplementedException();
            }

            public XElement ToXml(KeePassRng rng)
            {
                throw new NotImplementedException();
            }


            public IKeePassEntry Clone(bool preserveHistory = true)
            {
                throw new NotImplementedException();
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public int IconID
            {
                get { throw new NotImplementedException(); }
            }


            public void Update(IKeePassEntry template)
            {
                throw new NotImplementedException();
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public Windows.UI.Color? ForegroundColor
            {
                get { throw new NotImplementedException(); }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public Windows.UI.Color? BackgroundColor
            {
                get { throw new NotImplementedException(); }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public KdbxAutoType AutoType
            {
                get { throw new NotImplementedException(); }
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public string Tags
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


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public KeePassUuid CustomIconUuid
            {
                get { throw new NotImplementedException(); }
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            public ObservableCollection<KdbxBinary> Binaries
            {
                get { throw new NotImplementedException(); }
            }
        }

        #endregion
    }
}
