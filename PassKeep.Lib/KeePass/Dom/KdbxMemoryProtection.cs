// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.IO;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxMemoryProtection : KdbxPart
    {
        public static string RootName
        {
            get { return "MemoryProtection"; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        public bool ProtectTitle
        {
            get;
            private set;
        }

        public bool ProtectUserName
        {
            get;
            private set;
        }

        public bool ProtectPassword
        {
            get;
            private set;
        }

        public bool ProtectUrl
        {
            get;
            private set;
        }

        public bool ProtectNotes
        {
            get;
            private set;
        }

        public KdbxMemoryProtection()
            : base()
        {
            ProtectTitle = false;
            ProtectUserName = false;
            ProtectPassword = true;
            ProtectUrl = false;
            ProtectNotes = false;
        }

        public KdbxMemoryProtection(XElement xml)
            : base(xml)
        {
            ProtectTitle = GetBool("ProtectTitle");
            ProtectUserName = GetBool("ProtectUserName");
            ProtectPassword = GetBool("ProtectPassword");
            ProtectUrl = GetBool("ProtectURL");
            ProtectNotes = GetBool("ProtectNotes");
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            xml.Add(
                new XElement("ProtectTitle", ToKeePassBool(ProtectTitle)),
                new XElement("ProtectUserName", ToKeePassBool(ProtectUserName)),
                new XElement("ProtectPassword", ToKeePassBool(ProtectPassword)),
                new XElement("ProtectURL", ToKeePassBool(ProtectUrl)),
                new XElement("ProtectNotes", ToKeePassBool(ProtectNotes))
            );
        }

        public override bool Equals(object obj)
        {
            KdbxMemoryProtection other = obj as KdbxMemoryProtection;
            if (other == null)
            {
                return false;
            }

            return ProtectTitle == other.ProtectTitle &&
                ProtectUserName == other.ProtectUserName &&
                ProtectPassword == other.ProtectPassword &&
                ProtectUrl == other.ProtectUrl &&
                ProtectNotes == other.ProtectNotes;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
