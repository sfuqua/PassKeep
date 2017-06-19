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
    public class KdbxAutoType : KdbxPart
    {
        public static string RootName
        {
            get { return "AutoType"; }
        }
        protected override string rootName
        {
            get { return RootName; }
        }

        private XElement original;
        public KdbxAutoType(XElement xml)
            : base(xml)
        {
            original = xml;
        }

        public override void PopulateChildren(XElement element, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        { }

        public override bool Equals(object obj)
        {
            KdbxAutoType other = obj as KdbxAutoType;
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
