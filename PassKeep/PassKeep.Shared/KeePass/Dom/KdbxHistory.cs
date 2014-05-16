using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxHistory : KdbxPart
    {
        public static string RootName
        {
            get { return "History"; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        public IList<IKeePassEntry> Entries;

        private KdbxMetadata _metadata;
        public KdbxHistory(KdbxMetadata metadata)
        {
            Entries = new List<IKeePassEntry>();
            _metadata = metadata;
        }

        public KdbxHistory(XElement xml, IRandomNumberGenerator rng, KdbxMetadata metadata)
            : base(xml)
        {
            Entries = GetNodes(KdbxEntry.RootName)
                .Select(x => (IKeePassEntry)(new KdbxEntry(x, null, rng, metadata))).ToList();

            _metadata = metadata;
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng)
        {
            foreach (IKeePassEntry entry in Entries)
            {
                xml.Add(entry.ToXml(rng));
            }
        }

        public KdbxHistory Clone()
        {
            KdbxHistory clone = new KdbxHistory(_metadata);
            clone.Entries = this.Entries.Select(e => e.Clone()).ToList();
            return clone;
        }

        public void Add(IKeePassEntry entry)
        {
            IKeePassEntry historyEntry = entry.Clone(false);
            Entries.Add(historyEntry);
            if (_metadata.HistoryMaxItems >= 0)
            {
                while (Entries.Count > _metadata.HistoryMaxItems)
                {
                    Entries.RemoveAt(0);
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
