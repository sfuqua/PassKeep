using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ObservableCollection<IKeePassNode> _children;
        public ObservableCollection<IKeePassNode> Children
        {
            get { return this._children; }
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
            LastTopVisibleEntry = KeePassUuid.Empty;
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
            LastTopVisibleEntry = GetUuid("LastTopVisibleEntry", false) ?? KeePassUuid.Empty;

            // The order in which we deserialize entries and groups matters.
            // They must be constructed in the order that they appear in the XML,
            // or the RNG will enter undefined territory.
            ForgetNodes(KdbxEntry.RootName);
            ForgetNodes(KdbxGroup.RootName);

            // First, we need to select each XElement that represents either a group or an entry.
            // From these, we construct KdbxEntries and KdbxGroups.
            // Then we sort them, groups first, and add them to the child collection.
            IEnumerable<IKeePassNode> nodes = xml.Elements()
                .Where(element => element.Name == KdbxEntry.RootName || element.Name == KdbxGroup.RootName)
                .Select(
                    matchedElement =>
                    {
                        if (matchedElement.Name == KdbxEntry.RootName)
                        {
                            return new KdbxEntry(matchedElement, this, rng, metadata)
                                as IKeePassNode;
                        }
                        else
                        {
                            return new KdbxGroup(matchedElement, this, rng, metadata)
                                as IKeePassNode;
                        }
                    }
                )
                .OrderBy(
                    node => node is IKeePassGroup,
                    Comparer<bool>.Create((b1, b2) => b1.CompareTo(b2))
                );

            foreach(IKeePassNode node in nodes)
            {
                this._children.Add(node);
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

            foreach (IKeePassEntry entry in this.Children)
            {
                if (entry.Uuid.Equals(node.Uuid))
                {
                    return true;
                }
            }

            foreach (IKeePassGroup group in this.Children)
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

            foreach(IKeePassNode child in this.Children)
            {
                xml.Add(child.ToXml(rng));
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

            int childCount = this.Children.Count;
            if (childCount != other.Children.Count)
            {
                return false;
            }

            for (int i = 0; i < childCount; i++)
            {
                if (!this.Children[i].Equals(other.Children[i]))
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
            clone._children = this.Children;
            return clone;
        }

        /// <summary>
        /// Updates all public properties of this child with those of the target
        /// element, and updates LastModificationTime.
        /// Does not change UUID, Parent, or Children.
        /// </summary>
        /// <param name="newGroup"></param>
        /// <param name="isUpdate"></param>
        public void SyncTo(IKeePassGroup newGroup, bool isUpdate = true)
        {
            Dbg.Assert(newGroup != null);
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
        /// Attempts to locate the given node in the tree, and adopts it if possible.
        /// </summary>
        /// <param name="encodedUuid">The encoded Uuid of the node to adopt.</param>
        /// <returns>Whether adoption was successful.</returns>
        public bool TryAdopt(string encodedUuid)
        {
            IKeePassNode adoptee = FindNode(FindRoot(), encodedUuid);
            if (adoptee == null)
            {
                return false;
            }

            adoptee.Reparent(this);
            return true;
        }

        /// <summary>
        /// Attempts to locate a node in the tree given a parent from which to begin recursively searching.
        /// </summary>
        /// <param name="parent">The parent to use as a root.</param>
        /// <param name="encodedUuid">The Uuid to search for.</param>
        /// <returns>The node if found, else null.</returns>
        private static IKeePassNode FindNode(IKeePassGroup parent, string encodedUuid)
        {
            // Base case
            if (parent.Uuid.EncodedValue == encodedUuid)
            {
                return parent;
            }

            foreach (IKeePassNode node in parent.Children)
            {
                if (node.Uuid.EncodedValue == encodedUuid)
                {
                    return node;
                }

                // Recurse into child groups, depth first
                IKeePassGroup subGroup = node as IKeePassGroup;
                if (subGroup != null)
                {
                    IKeePassNode locatedNode = FindNode(subGroup, encodedUuid);
                    if (locatedNode != null)
                    {
                        return locatedNode;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the root of the tree by traversing parents.
        /// </summary>
        /// <returns>The root (Parent == null).</returns>
        private IKeePassGroup FindRoot()
        {
            IKeePassGroup root = this;
            while (root.Parent != null)
            {
                root = root.Parent;
            }

            return root;
        }

        /// <summary>
        /// Initializes the children and groups/entries collections for the instance.
        /// </summary>
        private void InitializeCollections()
        {
            this._children = new ObservableCollection<IKeePassNode>();
        }
    }
}
