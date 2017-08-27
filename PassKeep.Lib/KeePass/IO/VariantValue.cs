// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using SariphLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Storage.Streams;
using static PassKeep.Lib.Util.ByteHelper;
using static System.Buffer;

namespace PassKeep.Lib.KeePass.IO
{
    /// <summary>
    /// Represents a value contained in a <see cref="VariantDictionary"/>.
    /// </summary>
    public class VariantValue
    {
        private static readonly Dictionary<byte, Type> TagMap;

        private readonly Type type;
        private readonly byte[] data;
        private readonly object value;

        /// <summary>
        /// Initializes a mapping of tag bytes to <see cref="Type"/> values.
        /// </summary>
        static VariantValue()
        {
            Type[] allTypes = (Type[])Enum.GetValues(typeof(Type));

            TagMap = new Dictionary<byte, Type>(allTypes.Length);
            foreach (Type type in allTypes)
            {
                MemberInfo enumMember = typeof(Type).GetMember(type.ToString()).FirstOrDefault();
                TagAttribute tag = enumMember.GetCustomAttribute<TagAttribute>();

                DebugHelper.Assert(tag != null);
                DebugHelper.Assert(!TagMap.ContainsKey(tag.Value));

                TagMap[tag.Value] = type;
            }
        }

        /// <summary>
        /// Initializes an instance using a uint.
        /// </summary>
        /// <param name="val"></param>
        public VariantValue(uint val)
        {
            this.value = val;
            this.type = Type.UInt32;
            this.data = GetLittleEndianBytes(val);
        }

        /// <summary>
        /// Initializes an instance using a ulong.
        /// </summary>
        /// <param name="val"></param>
        public VariantValue(ulong val)
        {
            this.value = val;
            this.type = Type.UInt64;
            this.data = GetLittleEndianBytes(val);
        }

        /// <summary>
        /// Initializes an instance using a bool.
        /// </summary>
        /// <param name="val"></param>
        public VariantValue(bool val)
        {
            this.value = val;
            this.type = Type.Bool;
            this.data = new byte[1];
            this.data[0] = (byte)(val ? 1 : 0);
        }

        /// <summary>
        /// Initializes an instance using a uint.
        /// </summary>
        /// <param name="val"></param>
        public VariantValue(int val)
        {
            this.value = val;
            this.type = Type.Int32;
            this.data = GetLittleEndianBytes((uint)val);
        }

        /// <summary>
        /// Initializes an instance using a uint.
        /// </summary>
        /// <param name="val"></param>
        public VariantValue(long val)
        {
            this.value = val;
            this.type = Type.Int64;
            this.data = GetLittleEndianBytes((ulong)val);
        }

        /// <summary>
        /// Initializes an instance using a string.
        /// </summary>
        /// <param name="val"></param>
        public VariantValue(string val)
        {
            if (val == null)
            {
                throw new ArgumentNullException(nameof(val));
            }

            this.value = val;
            this.type = Type.String;
            this.data = Encoding.UTF8.GetBytes(val);
        }

        /// <summary>
        /// Initializes an instance using a byte array.
        /// </summary>
        /// <param name="val"></param>
        public VariantValue(byte[] val)
        {
            if (val == null)
            {
                throw new ArgumentNullException(nameof(val));
            }

            this.type = Type.ByteArray;
            this.data = new byte[val.Length];

            if (val.Length > 0)
            {
                BlockCopy(val, 0, this.data, 0, val.Length);
            }

            this.value = this.data;
        }

        /// <summary>
        /// Parses a <see cref="VariantValue"/> from binary data.
        /// </summary>
        /// <param name="data">The data to parse.</param>
        /// <param name="tag">Data indicating the type of  this data.</param>
        public VariantValue(IBuffer data, byte tag)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!TagMap.ContainsKey(tag))
            {
                throw new ArgumentException($"Unknown tag value {tag}", nameof(tag));
            }

            this.type = TagMap[tag];
            this.data = new byte[data.Length];
            if (data.Length > 0)
            {
                data.CopyTo(this.data);
            }

            switch (VariantType)
            {
                case Type.UInt32:
                    if (Size != 4)
                    {
                        throw new ArgumentException($"Expected 4 bytes for {VariantType}, got {Size}", nameof(data));
                    }
                    this.value = BufferToLittleEndianUInt32(this.data, 0);

                    break;
                case Type.UInt64:
                    if (Size != 8)
                    {
                        throw new ArgumentException($"Expected 8 bytes for {VariantType}, got {Size}", nameof(data));
                    }
                    this.value = BufferToLittleEndianUInt64(this.data, 0);

                    break;
                case Type.Bool:
                    if (Size != 1)
                    {
                        throw new ArgumentException($"Expected 1 byte for {VariantType}, got {Size}", nameof(data));
                    }
                    this.value = this.data[0] != 0;

                    break;
                case Type.Int32:
                    if (Size != 4)
                    {
                        throw new ArgumentException($"Expected 4 bytes for {VariantType}, got {Size}", nameof(data));
                    }
                    this.value = (int)BufferToLittleEndianUInt32(this.data, 0);

                    break;
                case Type.Int64:
                    if (Size != 8)
                    {
                        throw new ArgumentException($"Expected 8 bytes for {VariantType}, got {Size}", nameof(data));
                    }
                    this.value = (long)BufferToLittleEndianUInt64(this.data, 0);

                    break;
                case Type.String:
                    this.value = Encoding.UTF8.GetString(this.data);
                    break;
                case Type.ByteArray:
                    this.value = this.data;
                    break;
                default:
                    DebugHelper.Assert(false);
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the code used to identify the type of the serialized
        /// object.
        /// </summary>
        public Type VariantType
        {
            get { return this.type; }
        }

        /// <summary>
        /// Gets the number of bytes that make up this object.
        /// </summary>
        public int Size
        {
            get { return this.data.Length; }
        }

        /// <summary>
        /// Gets the object represented by this instance.
        /// </summary>
        public object Value
        {
            get { return this.value; }
        }

        /// <summary>
        /// Gets the byte serialization tag for a given type.
        /// </summary>
        /// <param name="type">Type to look up.</param>
        /// <returns>The tag to use for (de)serialization.</returns>
        public byte GetTag()
        {
            MemberInfo enumMember = typeof(Type).GetMember(VariantType.ToString())
                .FirstOrDefault();
            TagAttribute tag = enumMember.GetCustomAttribute<TagAttribute>();

            if (tag == null)
            {
                // Shouldn't happen
                throw new InvalidOperationException();
            }

            return tag.Value;
        }

        /// <summary>
        /// Generates a copy of the underlying data.
        /// </summary>
        /// <returns></returns>
        public byte[] GetData()
        {
            byte[] value = new byte[this.data.Length];
            if (Size > 0)
            {
                BlockCopy(this.data, 0, value, 0, Size);
            }

            return value;
        }

        /// <summary>
        /// Copies the underlying data to another array.
        /// </summary>
        /// <param name="dest">Array to copy to.</param>
        /// <param name="offset">Offset into <paramref name="dest"/> to begin the copy.</param>
        public void CopyTo(byte[] dest, int offset)
        {
            if (Size > 0)
            {
                BlockCopy(this.data, 0, dest, offset, Size);
            }
        }

        /// <summary>
        /// The type of a <see cref="VariantValue"/>.
        /// </summary>
        public enum Type
        {
            [Tag(0x04)]
            UInt32,

            [Tag(0x05)]
            UInt64,

            [Tag(0x08)]
            Bool,

            [Tag(0x0C)]
            Int32,

            [Tag(0x0D)]
            Int64,

            [Tag(0x18)]
            String,

            [Tag(0x42)]
            ByteArray
        }

        /// <summary>
        /// Maps values of <see cref="Type"/> to tag bytes for (de)serializing.
        /// </summary>
        private class TagAttribute : Attribute
        {
            /// <summary>
            /// Initializes the tag with a value.
            /// </summary>
            /// <param name="value">The type identifier for serializing.</param>
            public TagAttribute(byte value)
            {
                Value = value;
            }

            /// <summary>
            /// The value of the tag.
            /// </summary>
            public byte Value { get; set; }
        }
    }
}
