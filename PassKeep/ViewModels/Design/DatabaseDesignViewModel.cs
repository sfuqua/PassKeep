using System.Collections.ObjectModel;
using PassKeep.Common;
using PassKeep.Models;

namespace PassKeep.ViewModels.Design
{
    public class DatabaseDesignViewModel : BindableBase
    {
        private ObservableCollection<IGroup> _items;
        public ObservableCollection<IGroup> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        private ObservableCollection<object> _breadcrumbs;
        public ObservableCollection<object> Breadcrumbs
        {
            get { return _breadcrumbs; }
            set { SetProperty(ref _breadcrumbs, value); }
        }

        private IEntry _entry;
        public IEntry ActiveEntry
        {
            get { return _entry; }
            set { SetProperty(ref _entry, value); }
        }

        private bool _hasEntries;
        public bool HasEntries
        {
            get { return _hasEntries; }
            set { SetProperty(ref _hasEntries, value); }
        }

        public DatabaseDesignViewModel()
        {
            HasEntries = true;
            Items = new ObservableCollection<IGroup>
            {
                getItem("Foo Directory"),
                getItem("Bar Directory"),
                getItem("Baz Directory"),
                getItem("Some Directory"),
                getItem("Some other directory"),
                getItem("Foo Directory"),
                getItem("Bar Directory"),
                getItem("Baz Directory"),
                getItem("Some Directory"),
                getItem("Some other directory"),
                getItem("Foo Directory"),
                getItem("Bar Directory"),
                getItem("Baz Directory"),
                getItem("Some Directory"),
                getItem("Some other directory"),
                getEntry("Bank", "welcome"),
                getEntry("Airline", "flymeout", "123456", "myairline.org"),
                getEntry("Facebook", "aloha")
            };

            Breadcrumbs = new ObservableCollection<object>
            {
                getItem("Database"),
                getItem("Subdirectory"),
                getItem("Current Root")
            };

            ActiveEntry = getEntry("FooHub", "secure89", "Jimbo", "http://test.com/");
        }

        public IGroup getItem(string name)
        {
            return getItem(name, new KeePassUuid());
        }

        public IGroup getItem(string name, KeePassUuid uuid)
        {
            return new DatabaseDesignViewModel.Group
            {
                Title = new KdbxString("Title", name, null),
                Uuid = uuid
            };
        }

        public IEntry getEntry(string title, string password, string user="SomeUser", string url="", string notes="")
        {
            return new DatabaseDesignViewModel.Entry
            {
                Title = new KdbxString("Title", title, null),
                Password = new KdbxString("Password", password, null),
                UserName = new KdbxString("UserName", user, null),
                Url = new KdbxString("URL", url, null),
                Notes = new KdbxString("Notes", notes, null)
            };
        }

        public class Group : IGroup
        {
            public KdbxString Title { get; set; }
            public KeePassUuid Uuid { get; set; }
            public KdbxString Notes { get; set; }

            public bool MatchesQuery(string s)
            {
                return true;
            }
        }

        public class Entry : Group, IEntry
        {
            public KdbxString Password { get; set; }
            public string OverrideUrl { get; set; }
            public KdbxString Url { get; set; }
            public KdbxString UserName { get; set; }
        }
    }
}
