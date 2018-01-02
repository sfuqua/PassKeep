// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.IO;
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
            this.originalXml = xml;
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        { }

        public override bool Equals(object obj)
        {
            KdbxCustomIcons other = obj as KdbxCustomIcons;
            if (other == null)
            {
                return false;
            }

            return XElement.DeepEquals(this.originalXml, other.originalXml);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
