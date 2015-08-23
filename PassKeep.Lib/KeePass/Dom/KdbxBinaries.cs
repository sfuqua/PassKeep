using PassKeep.Lib.Contracts.KeePass;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxBinaries : KdbxPart
    {
        public static string RootName
        {
            get { return "Binaries"; }
        }
        protected override string rootName
        {
            get { return RootName; }
        }

        private XElement original;
        public KdbxBinaries(XElement xml)
            : base(xml)
        {
            original = xml;
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng) { }

        public override bool Equals(object obj)
        {
            KdbxBinaries other = obj as KdbxBinaries;
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
