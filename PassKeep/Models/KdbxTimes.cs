using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PassKeep.KeePassLib;
using PassKeep.KeePassLib.Crypto;

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

        public KdbxTimes(XElement xml, KdbxSerializationParameters parameters)
            : base(xml)
        {
            LastModificationTime = GetDate("LastModificationTime", parameters);
            CreationTime = GetDate("CreationTime", parameters);
            LastAccessTime = GetDate("LastAccessTime", parameters);
            ExpiryTime = GetDate("ExpiryTime", parameters);
            Expires = GetBool("Expires");
            UsageCount = GetInt("UsageCount");
            LocationChanged = GetDate("LocationChanged", parameters);
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            xml.Add(
                GetKeePassNode("LastModificationTime", LastModificationTime, parameters),
                GetKeePassNode("CreationTime", CreationTime, parameters),
                GetKeePassNode("LastAccessTime", LastAccessTime, parameters),
                GetKeePassNode("ExpiryTime", ExpiryTime, parameters),
                GetKeePassNode("Expires", Expires, parameters),
                GetKeePassNode("UsageCount", UsageCount, parameters),
                GetKeePassNode("LocationChanged", LocationChanged, parameters)
            );
        }

        /// <summary>
        /// Updates the values of this instance to the specified copy.
        /// </summary>
        /// <param name="times">The master copy to synchronize values to.</param>
        public void SyncTo(KdbxTimes times)
        {
            if (times == null)
            {
                throw new ArgumentNullException("times");
            }

            LastModificationTime = times.LastModificationTime;
            CreationTime = times.CreationTime;
            LastAccessTime = times.LastAccessTime;
            ExpiryTime = times.ExpiryTime;
            Expires = times.Expires;
            UsageCount = times.UsageCount;
            LocationChanged = times.LocationChanged;
        }

        public KdbxTimes Clone()
        {
            IKeePassTimes clone = new KdbxTimes(
                LastModificationTime,
                CreationTime,
                LastAccessTime,
                ExpiryTime,
                Expires,
                UsageCount,
                LocationChanged
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
