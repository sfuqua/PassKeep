using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Models;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    /// <summary>
    /// Represents a file that has been attached to an entry.
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

        private string fileName;

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

        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            // XXX
            int refId = 0;

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
