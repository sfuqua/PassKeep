﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PassKeep.KeePassLib;
using PassKeep.KeePassLib.Crypto;
using PassKeep.Models.Abstraction;

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
