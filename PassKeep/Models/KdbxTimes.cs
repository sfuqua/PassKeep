using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PassKeep.KeePassLib;

namespace PassKeep.Models
{
    public class KdbxTimes : KdbxPart
    {
        public static string RootName
        {
            get { return "Times"; }
        }

        protected override string rootName
        {
            get { return KdbxTimes.RootName; }
        }

        public DateTime LastModificationTime
        {
            get;
            set;
        }

        public DateTime CreationTime
        {
            get;
            private set;
        }

        public DateTime LastAccessTime
        {
            get;
            set;
        }

        public DateTime ExpiryTime
        {
            get;
            set;
        }

        public bool Expires
        {
            get;
            set;
        }

        public int UsageCount
        {
            get;
            set;
        }

        public DateTime LocationChanged
        {
            get;
            set;
        }

        public KdbxTimes()
        {
            LastModificationTime = DateTime.Now;
            CreationTime = DateTime.Now;
            LastAccessTime = DateTime.Now;
            ExpiryTime = DateTime.MaxValue;
            Expires = false;
            UsageCount = 0;
            LocationChanged = DateTime.Now;
        }

        public KdbxTimes(XElement xml)
            : base(xml)
        {
            LastModificationTime = GetDate("LastModificationTime");
            CreationTime = GetDate("CreationTime");
            LastAccessTime = GetDate("LastAccessTime");
            ExpiryTime = GetDate("ExpiryTime");
            Expires = GetBool("Expires");
            UsageCount = GetInt("UsageCount");
            LocationChanged = GetDate("LocationChanged");
        }

        public override void PopulateChildren(XElement xml, KeePassRng rng)
        {
            xml.Add(
                GetKeePassNode("LastModificationTime", LastModificationTime),
                GetKeePassNode("CreationTime", CreationTime),
                GetKeePassNode("LastAccessTime", LastAccessTime),
                GetKeePassNode("ExpiryTime", ExpiryTime),
                GetKeePassNode("Expires", Expires),
                GetKeePassNode("UsageCount", UsageCount),
                GetKeePassNode("LocationChanged", LocationChanged)
            );
        }

        public KdbxTimes Clone()
        {
            KdbxTimes clone = new KdbxTimes();
            clone.LastModificationTime = this.LastModificationTime;
            clone.CreationTime = this.CreationTime;
            clone.LastAccessTime = this.LastAccessTime;
            clone.ExpiryTime = this.ExpiryTime;
            clone.Expires = this.Expires;
            clone.UsageCount = this.UsageCount;
            clone.LocationChanged = this.LocationChanged;
            return clone;
        }

        public override bool Equals(object obj)
        {
            KdbxTimes other = obj as KdbxTimes;
            if (other == null)
            {
                return false;
            }

            return LastModificationTime == other.LastModificationTime &&
                CreationTime == other.CreationTime &&
                LastAccessTime == other.LastAccessTime &&
                ExpiryTime == other.ExpiryTime &&
                Expires == other.Expires &&
                UsageCount == other.UsageCount &&
                LocationChanged == other.LocationChanged;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
