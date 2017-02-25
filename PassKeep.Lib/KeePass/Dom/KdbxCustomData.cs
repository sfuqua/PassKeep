using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace PassKeep.Lib.KeePass.Dom
{
    /// <summary>
    /// Represents a string-to-string dictionary used by extensions to store
    /// information specific to a database or node.
    /// </summary>
    public class KdbxCustomData : KdbxPart
    {
        public static string RootName
        {
            get { return "CustomData"; }
        }
        protected override string rootName
        {
            get { return RootName; }
        }

        private readonly SortedDictionary<string, string> data;

        /// <summary>
        /// Deserializes custom data.
        /// </summary>
        /// <param name="xml">The XML to parse.</param>
        public KdbxCustomData(XElement xml)
            : base(xml)
        {
            this.data = new SortedDictionary<string, string>();
            foreach (XElement child in GetNodes("Item"))
            {
                string key = child.Element("Key")?.Value;
                if (key == null)
                {
                    throw new KdbxParseException(new ReaderResult(KdbxParserCode.CouldNotDeserialize, "CustomData item was missing key"));
                }

                string value = child.Element("Value")?.Value ?? string.Empty;
                this.data[key] = value;
            }
        }

        /// <summary>
        /// Initializes the data in the new instance as a clone of the provided parameter.
        /// </summary>
        /// <param name="data"></param>
        public KdbxCustomData(IDictionary<string, string> data)
        {
            this.data = new SortedDictionary<string, string>();

            if (data != null)
            {
                foreach (KeyValuePair<string, string> kvp in data)
                {
                    data[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Appends the data children to the XML node being serialized.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="rng"></param>
        /// <param name="parameters"></param>
        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            foreach (KeyValuePair<string, string> kvp in this.data)
            {
                xml.Add(
                    new XElement("Item",
                        new XElement("Key", kvp.Key),
                        new XElement("Value", kvp.Value)
                    )
                );
            }
        }

        /// <summary>
        /// Returns a deep clone of the the dictionary in this object.
        /// </summary>
        /// <returns></returns>
        public KdbxCustomData Clone()
        {
            return new KdbxCustomData(this.data);
        }

        /// <summary>
        /// Compares the data of two <see cref="CustomData"/> instances.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            KdbxCustomData other = obj as KdbxCustomData;
            if (other == null)
            {
                return false;
            }

            if (this.data.Count != other.data.Count)
            {
                return false;
            }

            var myData = this.data.Select(kvp => new Tuple<string, string>(kvp.Key, kvp.Value)).ToList();
            var otherData = other.data.Select(kvp => new Tuple<string, string>(kvp.Key, kvp.Value)).ToList();

            for (int i = 0; i < myData.Count; i++)
            {
                if (myData[i].Item1 != otherData[i].Item1 ||
                    myData[i].Item2 != otherData[i].Item2)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return this.data.GetHashCode();
        }
    }
}
