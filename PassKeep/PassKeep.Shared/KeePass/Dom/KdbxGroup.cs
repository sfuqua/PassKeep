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
            private set { TrySetProperty(ref _isExpanded, value); }
        }

        private string _defaultAutoTypeSequence;
        public string DefaultAutoTypeSequence
        {
            get { return _defaultAutoTypeSequence; }
            private set { TrySetProperty(ref _defaultAutoTypeSequence, value); }
        }

        private bool? _enableAutoType;
        public bool? EnableAutoType
        {
            get { return _enableAutoType; }
            private set { TrySetProperty(ref _enableAutoType, value); }
        }

        private bool? _enableSearching;
        public bool? EnableSearching
        {
            get { return _enableSearching; }
            set { TrySetProperty(ref _enableSearching, value); }
        }

        public KeePassUuid LastTopVisibleEntry
        {
            get;
            private set;
        }

        private ObservableCollection<IKeePassNode> children;
        private ReadOnlyObservableCollection<IKeePassNode> _children;
        public ReadOnlyObservableCollection<IKeePassNode> Children
        {
            get { return this._children; }
        }

        private ObservableCollection<IKeePassEntry> _entries;
        public ObservableCollection<IKeePassEntry> Entries
        {
            get { return this._entries; }
        }

        private ObservableCollection<IKeePassGroup> _groups;
        public ObservableCollection<IKeePassGroup> Groups
        {
            get { return _groups; }
        }

        private KdbxGroup()
        {
            InitializeCollections();
        }

        public KdbxGroup(IKeePassGroup parent) : this()
        {
            Parent = parent;
            Title = new KdbxString("Name", string.Empty, null);
            Notes = new KdbxString("Notes", string.Empty, null);
            Uuid = new KeePassUuid();
            IconID = KdbxGroup.DefaultIconId;
            Times = new KdbxTimes();
            IsExpanded = true;
            LastTopVisibleEntry = new KeePassUuid(Guid.Empty);
        }

        public KdbxGroup(XElement xml, IKeePassGroup parent, IRandomNumberGenerator rng, KdbxMetadata metadata)
            : base(xml)
        {
            InitializeCollections();

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

            foreach(KdbxEntry entry in 
                GetNodes(KdbxEntry.RootName).Select(x =>
                    (IKeePassEntry)(new KdbxEntry(x, this, rng, metadata))
                ))
            {
                this._entries.Add(entry);
            }

            foreach(KdbxGroup group in
                GetNodes(KdbxGroup.RootName).Select(x =>
                    (IKeePassGroup)(new KdbxGroup(x, this, rng, metadata))
                ))
            {
                this._groups.Add(group);
            }
        }

        /// <summary>
        /// Calculates inherited value for whether searching is permitted on entries in this group.
        /// </summary>
        /// <returns>Whether searching is permitted for this group.</returns>
        public bool IsSearchingPermitted()
        {
            // If the value isn't inherited, we have our answer right away.
            if (this.EnableSearching.HasValue)
            {
                return this.EnableSearching.Value;
            }

            // If the value is inherited, check to see if we're the root...
            if (this.Parent == null)
            {
                // Return the default in this case.
                return KdbxDocument.DefaultSearchableValue;
            }

            return this.Parent.IsSearchingPermitted();
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
            clone._entries = this.Entries;
            clone._groups = this.Groups;
            return clone;
        }

        /// <summary>
        /// Updates all public properties of this node with those of the target
        /// element, and updates LastModificationTime.
        /// Does not change UUID, Parent, or Children.
        /// </summary>
        /// <param name="newGroup"></param>
        /// <param name="isUpdate"></param>
        public void SyncTo(IKeePassGroup newGroup, bool isUpdate = true)
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

            this.Times.SyncTo(newGroup.Times);

            if (isUpdate)
            {
                Times.LastModificationTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Initializes the children and groups/entries collections for the instance.
        /// </summary>
        private void InitializeCollections()
        {
            this.children = new ObservableCollection<IKeePassNode>();
            this._children = new ReadOnlyObservableCollection<IKeePassNode>(this.children);

            this._groups = new ObservableCollection<IKeePassGroup>();
            this._groups.CollectionChanged += GroupsChangedHandler;

            this._entries = new ObservableCollection<IKeePassEntry>();
            this._entries.CollectionChanged += EntriesChangedHandler;
        }

        /// <summary>
        /// Rebuilds the entire "Children" collection in the event of major changes.
        /// </summary>
        private void RebuildChildren()
        {
            this.children.Clear();

            foreach (IKeePassGroup group in this.Groups)
            {
                this.children.Add(group);
            }

            foreach (IKeePassEntry entry in this.Entries)
            {
                this.children.Add(entry);
            }
        }

        /// <summary>
        /// Propagates updates to "Entries" to "Children".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EntriesChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    RebuildChildren();
                    break;
                case NotifyCollectionChangedAction.Add:
                    this.children.Insert(this.Groups.Count + e.NewStartingIndex, (IKeePassEntry)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Move:
                    this.children.Move(this.Groups.Count + e.OldStartingIndex, this.Groups.Count + e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    this.children.RemoveAt(this.Groups.Count + e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this.children[this.Groups.Count + e.NewStartingIndex] = (IKeePassEntry)e.NewItems[0];
                    break;
            }
        }

        /// <summary>
        /// Propagates updates to "Groups" to "Children".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    RebuildChildren();
                    break;
                case NotifyCollectionChangedAction.Add:
                    this.children.Insert(0 + e.NewStartingIndex, (IKeePassGroup)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Move:
                    this.children.Move(0 + e.OldStartingIndex, 0 + e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    this.children.RemoveAt(0 + e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this.children[0 + e.NewStartingIndex] = (IKeePassGroup)e.NewItems[0];
                    break;
            }
        }
    }
}
