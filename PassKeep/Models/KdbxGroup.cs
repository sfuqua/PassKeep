using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using PassKeep.KeePassLib;

namespace PassKeep.Models
{
    public class KdbxGroup : KdbxPart, IGroup
    {
        public KdbxGroup Parent
        {
            get;
            private set;
        }

        public KdbxString Title
        {
            get;
            set;
        }

        public KeePassUuid Uuid
        {
            get;
            set;
        }

        public KdbxString Notes
        {
            get;
            set;
        }

        public int IconID
        {
            get;
            private set;
        }

        public KdbxTimes Times
        {
            get;
            private set;
        }

        public bool IsExpanded
        {
            get;
            private set;
        }

        public string DefaultAutoTypeSequence
        {
            get;
            private set;
        }

        public bool? EnableAutoType
        {
            get;
            private set;
        }

        public bool? EnableSearching
        {
            get;
            set;
        }

        public KeePassUuid LastTopVisibleEntry
        {
            get;
            private set;
        }

        public IList<KdbxEntry> Entries
        {
            get;
            private set;
        }

        public IList<KdbxGroup> Groups
        {
            get;
            private set;
        }

        private KdbxGroup() { }

        public KdbxGroup(KdbxGroup parent)
        {
            Parent = parent;
            Title = new KdbxString("Name", string.Empty, null);
            Notes = new KdbxString("Notes", string.Empty, null);
            Uuid = new KeePassUuid();
            IconID = 48;
            Times = new KdbxTimes();
            IsExpanded = true;
            LastTopVisibleEntry = new KeePassUuid(Guid.Empty);
            Entries = new List<KdbxEntry>();
            Groups = new List<KdbxGroup>();
        }

        public KdbxGroup(XElement xml, KdbxGroup parent, KeePassRng rng, KdbxMetadata metadata)
            : base(xml)
        {
            Parent = parent;

            Title = new KdbxString("Name", GetString("Name"), null);
            Uuid = GetUuid("UUID");
            Notes = new KdbxString("Notes", GetString("Notes"), null);
            IconID = GetInt("IconID");

            var timesElement = GetNode(KdbxTimes.RootName);
            Times = new KdbxTimes(timesElement);

            IsExpanded = GetBool("IsExpanded");
            DefaultAutoTypeSequence = GetString("DefaultAutoTypeSequence");
            EnableAutoType = GetNullableBool("EnableAutoType");
            EnableSearching = GetNullableBool("EnableSearching");
            LastTopVisibleEntry = GetUuid("LastTopVisibleEntry");

            Entries = GetNodes(KdbxEntry.RootName).Select(x =>
                new KdbxEntry(x, this, rng, metadata)
            ).ToList();

            Groups = GetNodes(KdbxGroup.RootName).Select(x =>
                new KdbxGroup(x, this, rng, metadata)
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
                GetKeePassNode("IconID", IconID),
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

        public KdbxGroup Clone()
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
        /// Updates all public properties of this group with those of the target
        /// element, and updates LastModificationTime.
        /// Does not change UUID, Parent, or children.
        /// </summary>
        /// <param name="newGroup"></param>
        public void Update(KdbxGroup newGroup)
        {
            Debug.Assert(newGroup != null);
            if (newGroup == null)
            {
                throw new ArgumentNullException("newGroup");
            }

            IconID = newGroup.IconID;
            Title = newGroup.Title;
            Notes = newGroup.Notes;

            IsExpanded = newGroup.IsExpanded;
            DefaultAutoTypeSequence = newGroup.DefaultAutoTypeSequence;
            EnableAutoType = newGroup.EnableAutoType;
            EnableSearching = newGroup.EnableSearching;
            LastTopVisibleEntry = newGroup.LastTopVisibleEntry;

            Times.LastModificationTime = DateTime.Now;
        }
    }
}
