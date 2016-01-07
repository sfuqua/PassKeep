using PassKeep.Lib.Contracts.KeePass;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxCustomIcons : KdbxPart
    {
        public static string RootName
        {
            get { return "CustomIcons"; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        private XElement originalXml;
        public KdbxCustomIcons(XElement xml)
            : base(xml)
        {
            originalXml = xml;
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng)
        { }

        public override bool Equals(object obj)
        {
            KdbxCustomIcons other = obj as KdbxCustomIcons;
            if (other == null)
            {
                return false;
            }

            return XElement.DeepEquals(originalXml, other.originalXml);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
