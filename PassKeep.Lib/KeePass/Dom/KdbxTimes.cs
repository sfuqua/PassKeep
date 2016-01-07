using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxTimes : KdbxPart, IKeePassTimes
    {
        public static string RootName
        {
            get { return "Times"; }
        }

        protected override string rootName
        {
            get { return KdbxTimes.RootName; }
        }

        public DateTime? LastModificationTime
        {
            get;
            set;
        }

        public DateTime? CreationTime
        {
            get;
            private set;
        }

        public DateTime? LastAccessTime
        {
            get;
            set;
        }

        public DateTime? ExpiryTime
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

        public DateTime? LocationChanged
        {
            get;
            set;
        }

        public KdbxTimes() :
            this(
                lastModificationTime: DateTime.Now,
                creationTime: DateTime.Now,
                lastAccessTime: DateTime.Now,
                expiryTime: DateTime.MaxValue,
                expires: false,
                usageCount: 0,
                locationChanged: DateTime.Now
            )
        { }

        public KdbxTimes(
            DateTime? lastModificationTime,
            DateTime? creationTime,
            DateTime? lastAccessTime,
            DateTime? expiryTime,
            bool expires,
            int usageCount,
            DateTime? locationChanged
        )
        {
            LastModificationTime = lastModificationTime;
            CreationTime = creationTime;
            LastAccessTime = lastAccessTime;
            ExpiryTime = expiryTime;
            Expires = expires;
            UsageCount = usageCount;
            LocationChanged = locationChanged;
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

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng)
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

        /// <summary>
        /// Updates the values of this instance to the specified copy.
        /// </summary>
        /// <param name="times">The master copy to synchronize values to.</param>
        public void SyncTo(IKeePassTimes times)
        {
            if (times == null)
            {
                throw new ArgumentNullException("times");
            }

            this.LastModificationTime = times.LastModificationTime;
            this.CreationTime = times.CreationTime;
            this.LastAccessTime = times.LastAccessTime;
            this.ExpiryTime = times.ExpiryTime;
            this.Expires = times.Expires;
            this.UsageCount = times.UsageCount;
            this.LocationChanged = times.LocationChanged;
        }

        public IKeePassTimes Clone()
        {
            IKeePassTimes clone = new KdbxTimes(
                this.LastModificationTime,
                this.CreationTime,
                this.LastAccessTime,
                this.ExpiryTime,
                this.Expires,
                this.UsageCount,
                this.LocationChanged
            );
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
