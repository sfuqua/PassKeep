using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PassKeep.KeePassLib;

namespace PassKeep.Models
{
    public class KdbxRoot : KdbxPart
    {
        public static string RootName
        {
            get { return "Root"; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        private XElement deletedObjs;

        public KdbxGroup DatabaseGroup
        {
            get;
            private set;
        }

        public KdbxRoot(XElement xml, KeePassRng rng, KdbxMetadata metadata)
            : base(xml)
        {
            DatabaseGroup = new KdbxGroup(GetNode(KdbxGroup.RootName), null, rng, metadata);
            deletedObjs = GetNode("DeletedObjects");
        }

        public override void PopulateChildren(XElement xml, KeePassRng rng)
        {
            xml.Add(DatabaseGroup.ToXml(rng));
            if (deletedObjs != null)
            {
                xml.Add(deletedObjs);
            }
        }

        public override bool Equals(object obj)
        {
            KdbxRoot other = obj as KdbxRoot;
            if (other == null)
            {
                return false;
            }

            return DatabaseGroup.Equals(other.DatabaseGroup) && XElement.DeepEquals(deletedObjs, other.deletedObjs);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
