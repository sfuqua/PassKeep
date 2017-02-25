using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.Dom
{
    public class KdbxBinary : KdbxPart
    {
        public static readonly string RootName = "Binary";

        private readonly ProtectedBinary binaryData;

        /// <summary>
        /// Decodes an instance from serialized XML.
        /// </summary>
        /// <param name="xml">The XML to deserialize.</param>
        /// <param name="parameters">Parameters controlling serialization.</param>
        public KdbxBinary(XElement xml, KdbxSerializationParameters parameters)
        {
            // Parse out int ID attribute
            string idAttr = xml?.Attribute("ID")?.Value;
            if (idAttr == null)
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"KdbxBinary was missing required ID attribute")
                );
            }

            int id;
            if (!int.TryParse(idAttr, out id))
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"KdbxBinary ID attribute could not be parsed into an int")
                );
            }

            Id = id;

            // Parse out bool Compressed attribute
            string compressAttr = xml?.Attribute("Compressed")?.Value ?? "false";

            bool compressed;
            if (!bool.TryParse(compressAttr, out compressed))
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"KdbxBinary Compressed attribute could not be parsed into a bool")
                );
            }

            // Parse base64-encoded content
            byte[] content;
            try
            {
                content = CryptographicBuffer.DecodeFromBase64String(xml?.Value)?.ToArray();
                if (content == null)
                {
                    content = new byte[0];
                }
            }
            catch (Exception)
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"Could not decode KdbxBinary content as base64 data")
                );
            }

            // Decompress content if needed
            if (compressed && content.Length > 0)
            {
                byte[] decompressed;
                using (Stream memStream = new MemoryStream(content))
                {
                    using (Stream gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[1024];
                        int read = gzipStream.Read(buffer, 0, buffer.Length);
                        List<byte> bytes = new List<byte>();
                        while (read > 0)
                        {
                            bytes.AddRange(buffer.Take(read));
                            read = gzipStream.Read(buffer, 0, buffer.Length);
                        }

                        decompressed = bytes.ToArray();
                    }
                }

                content = decompressed;
            }

            this.binaryData = new ProtectedBinary(content, false);
        }

        /// <summary>
        /// Initializes the binary with the given values.
        /// </summary>
        /// <param name="id">The ID assigned to this binary.</param>
        /// <param name="data">The data being wrapped.</param>
        public KdbxBinary(int id, ProtectedBinary data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Id = id;
            this.binaryData = data;
        }

        /// <summary>
        /// Numeric identifier for this binary relative to all other binaries in this document.
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the actual protected binary instance represented by this node.
        /// </summary>
        public ProtectedBinary BinaryData
        {
            get { return this.binaryData; }
        }

        protected override string rootName
        {
            get
            {
                return RootName;
            }
        }

        /// <summary>
        /// Helper to get a value for <see cref="Compressed"/> based on serialization parameters.
        /// </summary>
        /// <param name="parameters">Parameters to evaluate.</param>
        /// <returns>Whether this object should use GZip compression when serializing.</returns>
        public static bool ShouldCompress(KdbxSerializationParameters parameters)
        {
            return parameters?.Compression != CompressionAlgorithm.None;
        }

        /// <summary>
        /// Returns a shallow clone of this object with a new ID.
        /// </summary>
        /// <param name="id">The new ID.</param>
        /// <returns>A new instance with the same data and a new ID.</returns>
        public KdbxBinary With(int id)
        {
            return new KdbxBinary(id, BinaryData);
        }

        /// <summary>
        /// Sets attributes and encoded base64 data, compressed if needed.
        /// </summary>
        /// <param name="xml">The Binary node to populate with attributes and a value.</param>
        /// <param name="rng">Not used.</param>
        /// <param name="parameters">Parameters for serialization.</param>
        public override void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            xml.Add(new XAttribute("ID", Id));

            byte[] data = BinaryData.GetClearData();
            if (ShouldCompress(parameters))
            {
                xml.Add(new XAttribute("Compressed", ToKeePassBool(true)));

                // Compress data if needed
                if (data.Length > 0)
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        using (Stream gzipStream = new GZipStream(memStream, CompressionMode.Compress))
                        {
                            gzipStream.Write(data, 0, data.Length);
                        }

                        memStream.Flush();
                        data = memStream.ToArray();
                    }
                }
            }

            string encoded = CryptographicBuffer.EncodeToBase64String(data.AsBuffer());
            xml.SetValue(encoded);
        }

        /// <summary>
        /// Checks two binary nodes for equality. Nodes are equal if they have the
        /// same ID and same data.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            KdbxBinary other = obj as KdbxBinary;
            if (other == null)
            {
                return false;
            }

            return Id == other.Id && BinaryData.Equals(other.BinaryData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 27;
                hash = (13 * hash) + Id.GetHashCode();
                hash = (13 * hash) + BinaryData.GetHashCode();
                return hash;
            }
        }
    }
}
