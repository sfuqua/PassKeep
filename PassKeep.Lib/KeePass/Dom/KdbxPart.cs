// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.KeePass.IO;
using PassKeep.Lib.Util;
using SariphLib.Diagnostics;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI;

namespace PassKeep.Lib.KeePass.Dom
{
    /// <summary>
    /// Contains common/shared logic for serializing and deserializing KDBX
    /// DOM components.
    /// </summary>
    public abstract class KdbxPart : BindableBase, IKeePassSerializable
    {
        protected abstract string rootName { get; }
        private XElement _rootNode;

        // Contains the child nodes of the node that makes up this object.
        // As objects are parsed out, they are removed from "pristine".
        // This allows us to maintain unknown tags "as-is" when reserializing. 
        private Dictionary<string, IList<XElement>> _pristine;

        public KdbxPart()
        { }

        // We rely on an abstract string property which will not cause any problems.
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public KdbxPart(XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            // We should only be parsing expected elements
            if (element.Name != rootName)
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure(
                        $"KdbxPart parse mismatch - expected root node {rootName}, got {element.Name}"
                    )
                );
            }

            // Build up a list of child elements to parse later
            this._rootNode = element;
            this._pristine = new Dictionary<string, IList<XElement>>();
            foreach (XElement node in element.Elements())
            {
                string key = node.Name.ToString();
                if (this._pristine.ContainsKey(key))
                {
                    this._pristine[key].Add(node);
                }
                else
                {
                    this._pristine[key] = new List<XElement> { node };
                }
            }
        }

        /// <summary>
        /// Given an <see cref="XElement"/> that is currently being constructed for serialization purposes,
        /// populates the children of the element.
        /// </summary>
        /// <param name="xml">The node being populated.</param>
        /// <param name="rng">Random number generator used for serializing protected strings.</param>
        /// <param name="parameters">Parameters controlling serialization.</param>
        public abstract void PopulateChildren(XElement xml, IRandomNumberGenerator rng, KdbxSerializationParameters parameters);

        /// <summary>
        /// Creates an <see cref="XElement"/> that represents this object.
        /// </summary>
        /// <param name="rng">Random number generator used for serializing protected strings.</param>
        /// <param name="parameters">Parameters controlling serialization.</param>
        /// <returns>An XML object that represents the current instance.</returns>
        public XElement ToXml(IRandomNumberGenerator rng, KdbxSerializationParameters parameters)
        {
            XElement xml = new XElement(rootName);
            PopulateChildren(xml, rng, parameters);

            // For each child we didn't parse out earlier during deserialization, add it
            // as-is.
            if (_pristine != null)
            {
                foreach (var kvp in _pristine)
                {
                    foreach (XElement node in kvp.Value)
                    {
                        xml.Add(node);
                    }
                }
            }

            return xml;
        }

        /// <summary>
        /// Deletes the specified node name from the underlying memory
        /// of clean XML nodes.
        /// </summary>
        /// <param name="name">The XML tag to stop tracking.</param>
        public void ForgetNodes(string name)
        {
            if (_pristine.ContainsKey(name))
            {
                _pristine.Remove(name);
            }
        }

        public XElement GetNode(string name)
        {
            if (_pristine.ContainsKey(name))
            {
                var nodes = _pristine[name];
                XElement node = nodes[0];
                if (nodes.Count == 1)
                {
                    _pristine.Remove(name);
                }
                else
                {
                    nodes.RemoveAt(0);
                }

                return node;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<XElement> GetNodes(string name)
        {
            if (_pristine.ContainsKey(name))
            {
                var nodes = _pristine[name];
                _pristine.Remove(name);
                return nodes;
            }
            else
            {
                return new List<XElement>();
            }
        }

        public string GetString(string name, bool required = false)
        {
            DebugHelper.Assert(!string.IsNullOrEmpty(name));
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name cannot be null or empty", nameof(name));
            }

            XElement child = GetNode(name);
            if (child == null)
            {
                if (!required)
                {
                    return null;
                }
                else
                {
                    throw new KdbxParseException(
                        ReaderResult.FromXmlParseFailure($"Node {rootName} missing required string child {name}")
                    );
                }
            }

            return child.Value ?? string.Empty;
        }

        public DateTime? GetDate(string name, KdbxSerializationParameters parameters, bool required = false)
        {
            string dtString = GetString(name, required);
            if (String.IsNullOrEmpty(dtString))
            {
                return null;
            }

            // KeePass interop weirdness - they trim the UTC timezone specifier off, then parse,
            // then convert to local time. When serializing they convert back to UTC and add the "Z".
            // Can't say why, but mimicing the behavior to avoid bugs.
            if (dtString.EndsWith("Z"))
            {
                dtString = dtString.Substring(0, dtString.Length - 1);
            }

            DateTime dt;
            if (DateTime.TryParse(dtString, out dt))
            {
                dt = dt.ToLocalTime();
                return dt;
            }
            else if (parameters.UseBase64DateTimeEncoding)
            {
                // Try to parse the DateTime as a base64 string
                IBuffer data = CryptographicBuffer.DecodeFromBase64String(dtString);
                if (data.Length == 8)
                {
                    long elapsedSeconds = (long)ByteHelper.BufferToLittleEndianUInt64(data.ToArray(), 0);
                    return new DateTime(elapsedSeconds * TimeSpan.TicksPerSecond, DateTimeKind.Utc);
                }
            }

            // This used to be a parse failure, but due to the strangeness of parsing dates, and because KeePass only considers
            // this an assertion failure with a fallback, we will also fallback.
            DebugHelper.Assert(false, $"Investigate why this DateTime failed to parse: {dtString}");
            return DateTime.Now;
        }

        public static string ToKeePassDate(DateTime? dt, KdbxSerializationParameters parameters)
        {
            if (!dt.HasValue)
            {
                return null;
            }

            if (parameters.UseBase64DateTimeEncoding)
            {
                long elapsedSeconds = dt.Value.Ticks / TimeSpan.TicksPerSecond;
                byte[] buffer = ByteHelper.GetLittleEndianBytes((ulong)elapsedSeconds);

                return CryptographicBuffer.EncodeToBase64String(buffer.AsBuffer());
            }
            else
            {
                // ToString("s") does not contain the Z UTC timezone specifier, which we want.
                return dt.Value.ToUniversalTime().ToString("s") + "Z";
            }
        }

        public bool GetBool(string name)
        {
            bool? nullableB = GetNullableBool(name, true);
            if (nullableB.HasValue)
            {
                return nullableB.Value;
            }
            else
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"Node {rootName} missing required bool child {name}")
                );
            }
        }

        public bool? GetNullableBool(string name, bool required = false)
        {
            string bString = GetString(name, required);

            bool b;
            if (bool.TryParse(bString, out b))
            {
                return b;
            }
            else
            {
                return null;
            }
        }

        public static string ToKeePassBool(bool? b)
        {
            if (b.HasValue)
            {
                return (b.Value ? "True" : "False");
            }
            else
            {
                return "null";
            }
        }

        public int GetInt(string name, int? def = null)
        {
            string iString = GetString(name, !def.HasValue);
            if (iString == null)
            {
                iString = def.Value.ToString();
            }

            int i;
            if (int.TryParse(iString, out i))
            {
                return i;
            }
            else
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"Could not parse {rootName}'s int child {name} - value: {iString}")
                );
            }
        }

        public KeePassUuid GetUuid(string name, bool required = true)
        {
            string uString = GetString(name, required);
            if (uString == null)
            {
                return null;
            }
            else
            {
                return new KeePassUuid(uString);
            }
        }

        public Color? GetNullableColor(string name)
        {
            string cString = GetString(name);
            if (cString == null || cString.Length != 7 || cString[0] != '#')
            {
                return null;
            }

            string rr = cString.Substring(1, 2);
            string gg = cString.Substring(3, 2);
            string bb = cString.Substring(5, 2);

            try
            {
                byte red = byte.Parse(rr, NumberStyles.HexNumber);
                byte green = byte.Parse(gg, NumberStyles.HexNumber);
                byte blue = byte.Parse(bb, NumberStyles.HexNumber);

                return Color.FromArgb(0xFF, red, green, blue);
            }
            catch (FormatException)
            {
                throw new KdbxParseException(
                    ReaderResult.FromXmlParseFailure($"Could not parse {rootName}'s color child {name} - value: {cString}")
                );
            }
        }

        public static string ToKeePassColor(Color? c)
        {
            if (c.HasValue)
            {
                return string.Format("#{0}{1}{2}",
                    c.Value.R.ToString("X2"),
                    c.Value.G.ToString("X2"),
                    c.Value.B.ToString("X2")
                );
            }
            else
            {
                return null;
            }
        }

        public static XElement GetKeePassNode<T>(string name, T tData, KdbxSerializationParameters kdbxParams)
        {
            XElement element = new XElement(name);
            if (tData == null)
            {
                return element;
            }
            
            string strValue;
            Type tType = typeof(T);
            object data = (object)tData;
            if (tType == typeof(Color?) || tType == typeof(Color))
            {
                strValue = ToKeePassColor((Color?)data);
            }
            else if (tType == typeof(bool?) || tType == typeof(bool))
            {
                strValue = ToKeePassBool((bool?)data);
            }
            else if (tType == typeof(DateTime?) || tType == typeof(DateTime))
            {
                strValue = ToKeePassDate((DateTime?)data, kdbxParams);
            }
            else
            {
                strValue = data.ToString();
            }

            if (!string.IsNullOrEmpty(strValue))
            {
                element.SetValue(strValue);
            }
            return element;
        }
    }
}
