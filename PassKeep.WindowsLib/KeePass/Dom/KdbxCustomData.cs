using PassKeep.Lib.Contracts.KeePass;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxCustomData : KdbxPart
    {
        public static string RootName
        {
            get { return "CustomData"; }
        }
        protected override string rootName
        {
            get { return RootName; }
        }

        private XElement original;
        public KdbxCustomData(XElement xml)
            : base(xml)
        {
            original = xml;
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng) { }

        public override bool Equals(object obj)
        {
            KdbxCustomData other = obj as KdbxCustomData;
            if (other == null)
            {
                return false;
            }

            return XElement.DeepEquals(original, other.original);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
