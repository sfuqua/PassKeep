using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxHistory : KdbxPart, IKeePassHistory
    {
        private KdbxMetadata metadata;
        private List<IKeePassEntry> entries;

        public KdbxHistory(KdbxMetadata metadata)
        {
            this.entries = new List<IKeePassEntry>();
            this.metadata = metadata;
        }

        public KdbxHistory(XElement xml, IRandomNumberGenerator rng, KdbxMetadata metadata)
            : base(xml)
        {
            this.entries = GetNodes(KdbxEntry.RootName)
                .Select(x => (IKeePassEntry)(new KdbxEntry(x, null, rng, metadata))).ToList();

            this.metadata = metadata;
        }

        public static string RootName
        {
            get { return "History"; }
        }

        public IReadOnlyList<IKeePassEntry> Entries
        {
            get { return this.entries; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng)
        {
            foreach (IKeePassEntry entry in Entries)
            {
                xml.Add(entry.ToXml(rng));
            }
        }

        public IKeePassHistory Clone()
        {
            KdbxHistory clone = new KdbxHistory(metadata);
            clone.entries = this.Entries.Select(e => e.Clone()).ToList();
            return clone;
        }

        public void Add(IKeePassEntry entry)
        {
            IKeePassEntry historyEntry = entry.Clone(/* preserveHistory */ false);
            this.entries.Add(historyEntry);
            if (metadata.HistoryMaxItems >= 0)
            {
                while (Entries.Count > metadata.HistoryMaxItems)
                {
                    this.entries.RemoveAt(0);
                }
            }
        }

        public override bool Equals(object obj)
        {
            KdbxHistory other = obj as KdbxHistory;
            if (other == null)
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

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
