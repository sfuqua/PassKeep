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
