using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.IO
{
    /// <summary>
    /// A serializable string -> object dictionary used by the KDBX4 file format.
    /// </summary>
    public sealed class VariantDictionary
    {
        public static readonly ushort Version = 0x0100;

        private readonly Dictionary<string, VariantValue> backingDict;

        /// <summary>
        /// Initializes an instance from an existing .NET dictionary. The dictionary is
        /// copied.
        /// </summary>
        /// <param name="backingDict">The dictionary to back this instance with.</param>
        public VariantDictionary(Dictionary<string, VariantValue> backingDict)
        {
            this.backingDict = new Dictionary<string, VariantValue>(backingDict);
        }

        /// <summary>
        /// Asynchronously deserializes a <see cref="VariantDictionary"/> instance from a data stream.
        /// </summary>
        /// <param name="reader">The reader to use for deserialization.</param>
        /// <returns>A task that resolves to the initialized <see cref="VariantDictionary"/>.</returns>
        public static async Task<VariantDictionary> ReadDictionaryAsync(DataReader reader)
        {
            // Read 2 byte version
            uint loadedBytes = await reader.LoadAsync(2);
            if (loadedBytes < 2)
            {
                throw new FormatException("Expected 2 bytes for VariantDictionary version");
            }

            ushort version = reader.ReadUInt16();

            ushort supportedVersion = (ushort)(Version & 0xFF00);
            ushort thisVersion = (ushort)(version & 0xFF00);

            if (thisVersion > supportedVersion)
            {
                throw new FormatException("Invalid dictionary; version is too high");
            }

            Dictionary<string, VariantValue> backingDict = new Dictionary<string, VariantValue>();

            // Read n items
            while (true)
            {
                // Read 1 byte for field type
                loadedBytes = await reader.LoadAsync(1);
                if (loadedBytes == 0)
                {
                    throw new FormatException("Expected 1 byte for field type");
                }

                byte fieldType = reader.ReadByte();
                if (fieldType == 0)
                {
                    // Null terminator indicates we're done
                    return new VariantDictionary(backingDict);
                }

                loadedBytes = await reader.LoadAsync(4);
                if (loadedBytes < 4)
                {
                    throw new FormatException("Expected 4 bytes for key name length");
                }

                uint keyNameBytes = reader.ReadUInt32();

                loadedBytes = await reader.LoadAsync(keyNameBytes);
                if (loadedBytes < keyNameBytes)
                {
                    throw new FormatException($"Expected {keyNameBytes} bytes for key name");
                }

                byte[] keyNameBuffer = new byte[keyNameBytes];
                reader.ReadBytes(keyNameBuffer);
                string keyName = Encoding.UTF8.GetString(keyNameBuffer);

                loadedBytes = await reader.LoadAsync(4);
                if (loadedBytes < 4)
                {
                    throw new FormatException("Expected 4 bytes for value length");
                }

                uint valueBytes = reader.ReadUInt32();

                loadedBytes = await reader.LoadAsync(valueBytes);
                if (loadedBytes < valueBytes)
                {
                    throw new FormatException($"Expected {valueBytes} bytes for value");
                }

                IBuffer data = reader.ReadBuffer(valueBytes);
                backingDict[keyName] = new VariantValue(data, fieldType);
            }
        }

        /// <summary>
        /// Asynchronously serializes a variant key-value-pair to the given writer.
        /// </summary>
        /// <param name="writer">The object to write the pair to.</param>
        /// <param name="kvp">The pair to write.</param>
        /// <returns>A task that resolves when writing is finished.</returns>
        public static async Task WriteKvpAsync(DataWriter writer, KeyValuePair<string, VariantValue> kvp)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            byte[] data = kvp.Value.GetData();

            writer.WriteByte(kvp.Value.GetTag());
            writer.WriteInt32(kvp.Key.Length);
            writer.WriteBytes(Encoding.UTF8.GetBytes(kvp.Key));
            writer.WriteInt32(data.Length);
            writer.WriteBytes(data);
            await writer.StoreAsync();
        }

        /// <summary>
        /// Asynchronously serializes this dictionary to the given writer.
        /// </summary>
        /// <param name="writer">The object to write the dictionary to.</param>
        /// <returns>A task that completes when writing is done.</returns>
        public async Task WriteToAsync(DataWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            // 2 byte version
            writer.WriteUInt16(Version);
            await writer.StoreAsync();

            foreach (var kvp in this.backingDict)
            {
                await WriteKvpAsync(writer, kvp).ConfigureAwait(false);
            }

            // Null terminator byte
            writer.WriteByte(0);
            await writer.StoreAsync();
        }

        /// <summary>
        /// Returns the value mapped to <paramref name="key"/> if it exists,
        /// else null.
        /// </summary>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The retrieved value, or null if it doesn't exist.</returns>
        public object GetValue(string key)
        {
            return this.backingDict.ContainsKey(key) ? this.backingDict[key].Value : null;
        }

        /// <summary>
        /// Computes the size (in bytes) that this dictionary will serialize to.
        /// </summary>
        /// <returns>The size in bytes that this dictionary will consume serialized.</returns>
        public int GetSerializedSize()
        {
            // 2 byte version, 1 byte terminator
            int size = 3;

            foreach (var kvp in this.backingDict)
            {
                // Tag
                size += 1;
                // 4 byte key length
                size += 4;
                // k byte key
                size += Encoding.UTF8.GetByteCount(kvp.Key);
                // 4 byte value length
                size += 4;
                // v byte value
                size += kvp.Value.Size;
            }

            return size;
        }
    }
}
