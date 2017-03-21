using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Models;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxDocument : KdbxPart
    {
        /// <summary>
        /// Whether KdbxGroups are searchable by default.
        /// </summary>
        public const bool DefaultSearchableValue = true;

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

        /// <summary>
        /// Initializes an empty document with provided metadata.
        /// </summary>
        /// <param name="metadata"></param>
        public KdbxDocument(KdbxMetadata metadata)
        {
            Metadata = metadata;
            Root = new KdbxRoot();
        }

        /// <summary>
        /// Parses a KeePass document from the specified XML.
        /// </summary>
        /// <param name="xml">XML to deserialize.</param>
        /// <param name="headerBinaries">Any binaries that were parsed from a header.</param>
        /// <param name="rng">RNG used to encrypt protected strings.</param>
        /// <param name="parameters">Parameters controlling serialization.</param>
        public KdbxDocument(XElement xml, IEnumerable<ProtectedBinary> headerBinaries, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
            : base(xml)
        {
            XElement metadata = GetNode(KdbxMetadata.RootName);
            if (metadata == null)
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"Document has no {KdbxMetadata.RootName} node")
                );
            }
            Metadata = new KdbxMetadata(metadata, headerBinaries, parameters);

            XElement root = GetNode(KdbxRoot.RootName);
            if (root == null)
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"Document has no {KdbxRoot.RootName} node")
                );
            }
            Root = new KdbxRoot(root, rng, Metadata, parameters);
        }

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            xml.Add(Metadata.ToXml(rng, parameters), Root.ToXml(rng, parameters));
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
