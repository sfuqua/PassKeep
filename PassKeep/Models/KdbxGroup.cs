using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using PassKeep.KeePassLib;
using PassKeep.Models.Abstraction;

namespace PassKeep.Models
{
    public class KdbxGroup : KdbxPart, IKeePassGroup
    {
        private IKeePassGroup _parent;
        public IKeePassGroup Parent
        {
            get { return _parent; }
            private set { SetProperty(ref _parent, value); }
        }

        private IProtectedString _title;
        public IProtectedString Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private KeePassUuid _uuid;
        public KeePassUuid Uuid
        {
            get { return _uuid; }
            set { SetProperty(ref _uuid, value); }
        }

        private IProtectedString _notes;
        public IProtectedString Notes
        {
            get { return _notes; }
            set { SetProperty(ref _notes, value); }
        }

        public const int DefaultIconId = 48;
        private int _iconId;
        public int IconID
        {
            get { return _iconId; }
            private set { SetProperty(ref _iconId, value); }
        }

        private KeePassUuid _customIconUuid;
        public KeePassUuid CustomIconUuid
        {
            get { return _customIconUuid; }
            private set { SetProperty(ref _customIconUuid, value); }
        }

        private KdbxTimes _times;
        public KdbxTimes Times
        {
            get { return _times; }
            private set { SetProperty(ref _times, value); }
        }

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

        public IList<IKeePassEntry> Entries
        {
            get;
            private set;
        }

        public IList<IKeePassGroup> Groups
        {
            get;
            private set;
        }

        private KdbxGroup() { }

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
            Entries = new List<IKeePassEntry>();
            Groups = new List<IKeePassGroup>();
        }

        public KdbxGroup(XElement xml, IKeePassGroup parent, KeePassRng rng, KdbxMetadata metadata)
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

            Entries = GetNodes(KdbxEntry.RootName).Select(x =>
                (IKeePassEntry)(new KdbxEntry(x, this, rng, metadata))
            ).ToList();

            Groups = GetNodes(KdbxGroup.RootName).Select(x =>
                (IKeePassGroup)(new KdbxGroup(x, this, rng, metadata))
            ).ToList();
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

        public override void PopulateChildren(XElement xml, KeePassRng rng)
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

        public bool MatchesQuery(string query)
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
        /// Does not change UUID, Parent, or children.
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
