using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PassKeep.KeePassLib;

namespace PassKeep.Models
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

        public override void PopulateChildren(XElement xml, KeePassRng rng)
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
