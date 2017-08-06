// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.IO;
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

        public KdbxRoot()
        {
            DatabaseGroup = new KdbxGroup(null);
            DatabaseGroup.Title.ClearValue = "Database Root";
        }

        public KdbxRoot(XElement xml, IRandomNumberGenerator rng, KdbxMetadata metadata, KdbxSerializationParameters parameters)
            : base(xml)
        {
            DatabaseGroup = new KdbxGroup(GetNode(KdbxGroup.RootName), null, rng, metadata, parameters);
            deletedObjs = GetNode("DeletedObjects");
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            xml.Add(DatabaseGroup.ToXml(rng, parameters));
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
