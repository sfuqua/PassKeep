using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PassKeep.KeePassLib;

namespace PassKeep.Models
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

        public override void PopulateChildren(XElement xml, KeePassRng rng) { }

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
