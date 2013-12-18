using PassKeep.Lib.Contracts.KeePass;
using System;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxDocument : KdbxPart
    {
        public static string RootName { get { return "KeePassFile"; } }
        protected override string rootName
        {
            get { return RootName; }
        }

        public KdbxMetadata Metadata
        {
            get;
            private set;
        }

        public KdbxRoot Root
        {
            get;
            private set;
        }

        public KdbxDocument(XElement xml, IRandomNumberGenerator rng) : base(xml)
        {
            XElement metadata = GetNode(KdbxMetadata.RootName);
            if (metadata == null)
            {
                throw new ArgumentException("metadata not found", "xml");
            }
            Metadata = new KdbxMetadata(metadata);

            XElement root = GetNode(KdbxRoot.RootName);
            if (root == null)
            {
                throw new ArgumentException("root not found", "xml");
            }
            Root = new KdbxRoot(root, rng, Metadata);
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng)
        {
            xml.Add(Metadata.ToXml(rng), Root.ToXml(rng));
        }

        public override bool Equals(object obj)
        {
            KdbxDocument other = obj as KdbxDocument;
            if (other == null)
            {
                return false;
            }

            return Metadata.Equals(other.Metadata) && Root.Equals(other.Root);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
