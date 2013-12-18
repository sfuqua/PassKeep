using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxGroup : KdbxNode, IKeePassGroup
    {
        public const int DefaultIconId = 48;

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            private set { SetProperty(ref _isExpanded, value); }
        }

        private string _defaultAutoTypeSequence;
        public string DefaultAutoTypeSequence
        {
            get { return _defaultAutoTypeSequence; }
            private set { SetProperty(ref _defaultAutoTypeSequence, value); }
        }

        private bool? _enableAutoType;
        public bool? EnableAutoType
        {
            get { return _enableAutoType; }
            private set { SetProperty(ref _enableAutoType, value); }
        }

        private bool? _enableSearching;
        public bool? EnableSearching
        {
            get { return _enableSearching; }
            set { SetProperty(ref _enableSearching, value); }
        }

        public KeePassUuid LastTopVisibleEntry
        {
            get;
            private set;
        }

        private bool suppressChildrenChanges = false;
        private bool childUpdateInProgress = false;
        private ObservableCollection<IKeePassNode> _children;
        public ObservableCollection<IKeePassNode> Children
        {
            get { return _children; }
            private set
            {
                if (value != null && value.Count > 0)
                {
                    throw new InvalidOperationException();
                }

                ObservableCollection<IKeePassNode> oldValue = _children;
                if (SetProperty(ref _children, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= childrenChangedHandler;
                    }

                    if (_children != null)
                    {
                        _children.CollectionChanged -= childrenChangedHandler;
                    }
                }

                if (!childUpdateInProgress)
                {
                    suppressChildrenChanges = true;
                    Entries = new ObservableCollection<IKeePassEntry>();
                    Groups = new ObservableCollection<IKeePassGroup>();
                    suppressChildrenChanges = false;
                }
            }
        }

        private ObservableCollection<IKeePassEntry> _entries;
        public ObservableCollection<IKeePassEntry> Entries
        {
            get { return _entries; }
            private set
            {
                ObservableCollection<IKeePassEntry> oldValue = _entries;
                if (SetProperty(ref _entries, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= entriesChangedHandler;
                    }

                    if (_entries != null)
                    {
                        _entries.CollectionChanged += entriesChangedHandler;
                    }
                }

                rebuildChildren();
            }
        }

        private ObservableCollection<IKeePassGroup> _groups;
        public ObservableCollection<IKeePassGroup> Groups
        {
            get { return _groups; }
            private set
            {
                ObservableCollection<IKeePassGroup> oldValue = _groups;
                if (SetProperty(ref _groups, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= groupsChangedHandler;
                    }

                    if (_groups != null)
                    {
                        _groups.CollectionChanged += groupsChangedHandler;
                    }
                }

                rebuildChildren();
            }
        }

        private void rebuildChildren()
        {
            if (suppressChildrenChanges)
            {
                return;
            }

            childUpdateInProgress = true;
            Children = new ObservableCollection<IKeePassNode>();

            if (Groups != null)
            {
                foreach (IKeePassGroup group in Groups)
                {
                    Children.Add(group);
                }
            }

            if (Entries != null)
            {
                foreach (IKeePassEntry entry in Entries)
                {
                    Children.Add(entry);
                }
            }
            childUpdateInProgress = false;
        }

        private void childrenChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (childUpdateInProgress)
            {
                return;
            }

            suppressChildrenChanges = true;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    if (Children.Count > 0)
                    {
                        throw new InvalidOperationException();
                    }

                    Entries = new ObservableCollection<IKeePassEntry>();
                    Groups = new ObservableCollection<IKeePassGroup>();
                    
                    break;
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < Groups.Count)
                    {
                        Groups.Insert(e.NewStartingIndex, (IKeePassGroup)e.NewItems[0]);
                    }
                    else
                    {
                        Entries.Insert(e.NewStartingIndex - Groups.Count, (IKeePassEntry)e.NewItems[0]);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < Groups.Count)
                    {
                        if (e.NewStartingIndex >= Groups.Count)
                        {
                            throw new InvalidOperationException();
                        }

                        Groups.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else
                    {
                        if (e.NewStartingIndex < Groups.Count)
                        {
                            throw new InvalidOperationException();
                        }

                        Entries.Move(e.OldStartingIndex - Groups.Count, e.NewStartingIndex - Groups.Count);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < Groups.Count)
                    {
                        Groups.RemoveAt(e.OldStartingIndex);
                    }
                    else
                    {
                        Entries.RemoveAt(e.OldStartingIndex - Groups.Count);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    object replacement = e.NewItems[0];
                    if (e.NewStartingIndex < Groups.Count)
                    {
                        Groups[e.NewStartingIndex] = (IKeePassGroup)replacement;
                    }
                    else
                    {
                        Entries[e.NewStartingIndex - Groups.Count] = (IKeePassEntry)replacement;
                    }
                    break;
            }
            suppressChildrenChanges = false;
        }

        private void entriesChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (suppressChildrenChanges)
            {
                return;
            }

            childUpdateInProgress = true;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    rebuildChildren();
                    break;
                case NotifyCollectionChangedAction.Add:
                    Children.Insert(Groups.Count + e.NewStartingIndex, (IKeePassEntry)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Move:
                    Children.Move(Groups.Count + e.OldStartingIndex, Groups.Count + e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Children.RemoveAt(Groups.Count + e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Children[Groups.Count + e.NewStartingIndex] = (IKeePassEntry)e.NewItems[0];
                    break;
            }
            childUpdateInProgress = false;
        }

        private void groupsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (suppressChildrenChanges)
            {
                return;
            }

            childUpdateInProgress = true;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    rebuildChildren();
                    break;
                case NotifyCollectionChangedAction.Add:
                    Children.Insert(0 + e.NewStartingIndex, (IKeePassGroup)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Move:
                    Children.Move(0 + e.OldStartingIndex, Groups.Count + e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Children.RemoveAt(0 + e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Children[0 + e.NewStartingIndex] = (IKeePassGroup)e.NewItems[0];
                    break;
            }
            childUpdateInProgress = false;
        }

        private KdbxGroup()
        {
            Children = new ObservableCollection<IKeePassNode>();
        }

        public KdbxGroup(IKeePassGroup parent)
        {
            Parent = parent;
            Title = new KdbxString("Name", string.Empty, null);
            Notes = new KdbxString("Notes", string.Empty, null);
            Uuid = new KeePassUuid();
            IconID = KdbxGroup.DefaultIconId;
            Times = new KdbxTimes();
            IsExpanded = true;
            LastTopVisibleEntry = new KeePassUuid(Guid.Empty);
            Entries = new ObservableCollection<IKeePassEntry>();
            Groups = new ObservableCollection<IKeePassGroup>();
        }

        public KdbxGroup(XElement xml, IKeePassGroup parent, IRandomNumberGenerator rng, KdbxMetadata metadata)
            : base(xml)
        {
            Parent = parent;

            Title = new KdbxString("Name", GetString("Name"), null);
            Uuid = GetUuid("UUID");
            Notes = new KdbxString("Notes", GetString("Notes"), null);
            IconID = GetInt("IconID");
            CustomIconUuid = GetUuid("CustomIconUUID", false);

            var timesElement = GetNode(KdbxTimes.RootName);
            Times = new KdbxTimes(timesElement);

            IsExpanded = GetBool("IsExpanded");
            DefaultAutoTypeSequence = GetString("DefaultAutoTypeSequence");
            EnableAutoType = GetNullableBool("EnableAutoType");
            EnableSearching = GetNullableBool("EnableSearching");
            LastTopVisibleEntry = GetUuid("LastTopVisibleEntry");

            suppressChildrenChanges = true;
            Entries = new ObservableCollection<IKeePassEntry>(
                GetNodes(KdbxEntry.RootName).Select(x =>
                    (IKeePassEntry)(new KdbxEntry(x, this, rng, metadata))
                )
            );

            Groups = new ObservableCollection<IKeePassGroup>(
                GetNodes(KdbxGroup.RootName).Select(x =>
                    (IKeePassGroup)(new KdbxGroup(x, this, rng, metadata))
                )
            );
            suppressChildrenChanges = false;

            rebuildChildren();
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

        public static string RootName
        {
            get { return "Group"; }
        }

        protected override string rootName
        {
            get { return KdbxGroup.RootName; }
        }

        private XElement getBizarroNullableBool(string name, bool? value)
        {
            XElement node = GetKeePassNode(name, value);
            if (string.IsNullOrEmpty(node.Value))
            {
                node.SetValue("null");
            }
            else
            {
                node.Value = node.Value.ToLower();
            }
            return node;
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng)
        {
            xml.Add(
                GetKeePassNode("UUID", Uuid),
                GetKeePassNode("Name", Title.ClearValue),
                GetKeePassNode("Notes", Notes.ClearValue),
                GetKeePassNode("IconID", IconID)
            );

            if (CustomIconUuid != null)
            {
                xml.Add(GetKeePassNode("CustomIconUUID", CustomIconUuid));
            }

            xml.Add(
                Times.ToXml(rng),
                GetKeePassNode("IsExpanded", IsExpanded),
                GetKeePassNode("DefaultAutoTypeSequence", DefaultAutoTypeSequence),
                getBizarroNullableBool("EnableAutoType", EnableAutoType),
                getBizarroNullableBool("EnableSearching", EnableSearching),
                GetKeePassNode("LastTopVisibleEntry", LastTopVisibleEntry)
            );

            foreach (KdbxEntry entry in Entries)
            {
                xml.Add(entry.ToXml(rng));
            }

            foreach (KdbxGroup group in Groups)
            {
                xml.Add(group.ToXml(rng));
            }
        }

        public override bool MatchesQuery(string query)
        {
            return Title.ClearValue.ToUpperInvariant().Contains(query.ToUpperInvariant());
        }

        public override bool Equals(object obj)
        {
            KdbxGroup other = obj as KdbxGroup;
            if (other == null)
            {
                return false;
            }

            if (Parent != null)
            {
                if (other.Parent == null)
                {
                    return false;
                }
                if (!Parent.Uuid.Equals(other.Parent.Uuid))
                {
                    return false;
                }
            }
            else
            {
                if (other.Parent != null) { return false; }
            }

            if (!Uuid.Equals(other.Uuid))
            {
                return false;
            }

            if (!Title.Equals(other.Title) || !Notes.Equals(other.Notes))
            {
                return false;
            }

            if (IconID != other.IconID)
            {
                return false;
            }

            if (CustomIconUuid != null)
            {
                if (!CustomIconUuid.Equals(other.CustomIconUuid)) { return false; }
            }
            else
            {
                if (other.CustomIconUuid != null) { return false; }
            }

            if (!Times.Equals(other.Times))
            {
                return false;
            }

            if (IsExpanded != other.IsExpanded || !LastTopVisibleEntry.Equals(other.LastTopVisibleEntry))
            {
                return false;
            }

            if (DefaultAutoTypeSequence != other.DefaultAutoTypeSequence ||
                EnableAutoType != other.EnableAutoType ||
                EnableSearching != other.EnableSearching)
            {
                return false;
            }

            if (Entries.Count != other.Entries.Count)
            {
                return false;
            }

            for (int i = 0; i < Entries.Count; i++)
            {
                if (!Entries[i].Equals(other.Entries[i]))
                {
                    return false;
                }
            }

            if (Groups.Count != other.Groups.Count)
            {
                return false;
            }

            for (int i = 0; i < Groups.Count; i++)
            {
                if (!Groups[i].Equals(other.Groups[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public IKeePassGroup Clone()
        {
            KdbxGroup clone = new KdbxGroup();
            clone.Parent = this.Parent;
            if (Title != null)
            {
                clone.Title = this.Title.Clone();
            }
            else
            {
                clone.Title = null;
            }
            clone.Uuid = this.Uuid.Clone();
            if (Notes != null)
            {
                clone.Notes = this.Notes.Clone();
            }
            else
            {
                clone.Notes = null;
            }
            clone.IconID = this.IconID;
            if (this.CustomIconUuid != null)
            {
                clone.CustomIconUuid = this.CustomIconUuid.Clone();
            }
            else
            {
                clone.CustomIconUuid = null;
            }
            clone.Times = this.Times.Clone();
            clone.IsExpanded = this.IsExpanded;
            clone.DefaultAutoTypeSequence = this.DefaultAutoTypeSequence;
            clone.EnableAutoType = this.EnableAutoType;
            clone.EnableSearching = this.EnableSearching;
            if (LastTopVisibleEntry != null)
            {
                clone.LastTopVisibleEntry = this.LastTopVisibleEntry.Clone();
            }
            else
            {
                clone.LastTopVisibleEntry = null;
            }
            clone.Entries = this.Entries;
            clone.Groups = this.Groups;
            return clone;
        }

        /// <summary>
        /// Updates all public properties of this node with those of the target
        /// element, and updates LastModificationTime.
        /// Does not change UUID, Parent, or Children.
        /// </summary>
        /// <param name="newGroup"></param>
        /// <param name="updateModificationTime"></param>
        public void Update(IKeePassGroup newGroup, bool updateModificationTime = true)
        {
            Debug.Assert(newGroup != null);
            if (newGroup == null)
            {
                throw new ArgumentNullException("newGroup");
            }

            IconID = newGroup.IconID;
            CustomIconUuid = newGroup.CustomIconUuid;
            Title = (newGroup.Title != null ? newGroup.Title.Clone() : null);
            Notes = (newGroup.Notes != null ? newGroup.Notes.Clone() : null);

            IsExpanded = newGroup.IsExpanded;
            DefaultAutoTypeSequence = newGroup.DefaultAutoTypeSequence;
            EnableAutoType = newGroup.EnableAutoType;
            EnableSearching = newGroup.EnableSearching;
            LastTopVisibleEntry = newGroup.LastTopVisibleEntry;

            if (updateModificationTime)
            {
                Times.LastModificationTime = DateTime.Now;
            }
        }
    }
}
