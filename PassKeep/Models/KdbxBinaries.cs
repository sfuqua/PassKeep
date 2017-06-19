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
    /// <summary>
    /// Represents a collection of <see cref="KdbxBinary"/> objects.
    /// </summary>
    public class KdbxBinaries : KdbxPart
    {
        public static readonly string RootName = "Binaries";
    
        private readonly SortedDictionary<int, KdbxBinary> binaries;

        /// <summary>
        /// Initializes an empty collection of binaries.
        /// </summary>
        public KdbxBinaries()
        {
            this.binaries = new SortedDictionary<int, KdbxBinary>();
        }

        /// <summary>
        /// Initializes an instance around an existing group of binaries.
        /// </summary>
        /// <param name="binaries">The collection of binaries to encapsulate.</param>
        public KdbxBinaries(IEnumerable<ProtectedBinary> binaries)
            : this()
        {
            if (binaries == null)
            {
                throw new ArgumentNullException("binaries");
            }

            int i = 0;
            foreach (ProtectedBinary bin in binaries)
            {
                if (bin == null)
                {
                    continue;
                }

                this.binaries[i] = new KdbxBinary(i, bin);
                i++;
            }
        }

        /// <summary>
        /// Parses a collection of binaries from the given XML.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="parameters"></param>
        public KdbxBinaries(XElement xml, KdbxSerializationParameters parameters)
            : base(xml)
        {
            this.binaries = new SortedDictionary<int, KdbxBinary>();

            foreach (XElement ele in GetNodes(KdbxBinary.RootName))
            {
                KdbxBinary bin = new KdbxBinary(ele, parameters);
                if (this.binaries.ContainsKey(bin.Id))
                {
                    throw new KdbxParseException(
                        ReaderResult.FromXmlParseFailure("Duplicate binary key" + bin.Id)
                    );
                }

                this.binaries.Add(bin.Id, bin);
            }
        }

        protected override string rootName
        {
            get { return RootName; }
        }

        /// <summary>
        /// Populates the given <see cref="XElement"/> with <see cref="KdbxBinary"/> children,
        /// with new IDs based on the current state of this instance.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="rng"></param>
        /// <param name="parameters"></param>
        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            int i = 0;
            foreach (var kvp in this.binaries)
            {
                xml.Add(kvp.Value.With(id: i++).ToXml(rng, parameters));
            }
        }

        /// <summary>
        /// Enumerates the binary data contained in this instance.
        /// </summary>
        public IEnumerable<ProtectedBinary> Binaries
        {
            get { return this.binaries.Values.Select(bin => bin.BinaryData); }
        }

        /// <summary>
        /// Whether two <see cref="KdbxBinaries"/> instances contain the same data.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            KdbxBinaries other = obj as KdbxBinaries;
            if (other == null)
            {
                return false;
            }

            if (this.binaries.Count != other.binaries.Count)
            {
                return false;
            }

            foreach (var kvp in this.binaries)
            {
                if (!other.binaries.ContainsKey(kvp.Key))
                {
                    return false;
                }

                if (!this.binaries[kvp.Key].Equals(other.binaries[kvp.Key]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the <see cref="ProtectedBinary"/> corresponding to the
        /// <see cref="KdbxBinary"/> maintained by this instance at ID <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The key to retrieve.</param>
        /// <returns>The binary data corresponding the specified DOM node.</returns>
        public ProtectedBinary GetById(int id)
        {
            return this.binaries[id].BinaryData;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
