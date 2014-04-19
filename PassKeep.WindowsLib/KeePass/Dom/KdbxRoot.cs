using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
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

        public IKeePassGroup DatabaseGroup
        {
            get;
            private set;
        }

        public KdbxRoot(XElement xml, IRandomNumberGenerator rng, KdbxMetadata metadata)
            : base(xml)
        {
            DatabaseGroup = new KdbxGroup(GetNode(KdbxGroup.RootName), null, rng, metadata);
            deletedObjs = GetNode("DeletedObjects");
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng)
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
