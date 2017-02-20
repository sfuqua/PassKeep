using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Models;
using SariphLib.Infrastructure;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    /// <summary>
    /// Represents a file that has been attached to an entry. The XML is implemented
    /// as a "Ref" to an index in the database's binary collection.
    /// </summary>
    public class KdbxBinAttachment : KdbxPart, IKeePassBinAttachment
    {
        public static string RootName
        {
            get { return "Binary"; }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        private readonly KdbxBinaries binaryCollection;
        private string fileName;

        /// <summary>
        /// Deserializes this node by using the <see cref="KdbxMetadata"/> binary collection
        /// to dereference @Ref.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="metadata">Used to dereference the Ref attribute.</param>
        /// <param name="parameters"></param>
        public KdbxBinAttachment(XElement xml, KdbxMetadata metadata, KdbxSerializationParameters parameters)
            : base(xml)
        {
            FileName = GetString("Key", true);

            XElement valueNode = GetNode("Value");
            if (valueNode == null)
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"Node {rootName} missing required child Value")
                );
            }

            int refId;
            string refAttr = valueNode.Attribute("Ref")?.Value;
            if (refAttr == null || !int.TryParse(refAttr, out refId))
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"Child Value node of {rootName} missing required int @Ref")
                );
            }

            Data = metadata.Binaries.GetById(refId);
            this.binaryCollection = metadata.Binaries;
        }

        /// <summary>
        /// The key for this binary - its name.
        /// </summary>
        public string FileName
        {
            get
            {
                return this.fileName;
            }
            set
            {
                TrySetProperty(ref this.fileName, value);
            }
        }

        /// <summary>
        /// The data that makes up this binary.
        /// </summary>
        public ProtectedBinary Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Looks up the data represented by this attachment in the metadata binary collection
        /// to obtain a new @Ref ID for XML serialization.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="rng"></param>
        /// <param name="parameters"></param>
        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            int refId = 0;
            bool foundBin = false;

            foreach (var bin in this.binaryCollection.Binaries)
            {
                if (bin.Equals(Data))
                {
                    foundBin = true;
                    break;
                }

                refId++;
            }

            Dbg.Assert(foundBin);

            xml.Add(
                new XElement("Key", FileName),
                new XElement("Value", new XAttribute("Ref", refId))
            );
        }

        /// <summary>
        /// Compares equality based on both filename and data.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            KdbxBinAttachment other = obj as KdbxBinAttachment;
            if (other == null)
            {
                return false;
            }

            return FileName == other.FileName && Data.Equals(other.Data);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
