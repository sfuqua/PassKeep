using SariphLib.Infrastructure;
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
        public static readonly uint Version = 0x1000;

        private readonly Dictionary<string, object> backingDict;

        /// <summary>
        /// Initializes an instance from an existing .NET dictionary.
        /// </summary>
        /// <param name="backingDict">The dictionary to back this instance with.</param>
        public VariantDictionary(Dictionary<string, object> backingDict)
        {
            this.backingDict = backingDict;
        }

        /// <summary>
        /// Asynchronously deserializes a <see cref="VariantDictionary"/> instance from a data stream.
        /// </summary>
        /// <param name="reader">The reader to use for deserialization.</param>
        /// <returns>A task that resolves to the initialized <see cref="VariantDictionary"/>.</returns>
        public static async Task<VariantDictionary> ReadDictionaryAsync(DataReader reader)
        {
            // Read 2 byte version
            await reader.LoadAsync(2);
            ushort version = reader.ReadUInt16();

            ushort supportedVersion = (ushort)(Version & 0xFF00);
            ushort thisVersion = (ushort)(version & 0xFF00);

            if (thisVersion > supportedVersion)
            {
                throw new InvalidOperationException("Invalid dictionary; version is too high");
            }

            Dictionary<string, object> backingDict = new Dictionary<string, object>();

            // Read n items
            while (true)
            {
                // Read 1 byte for field type
                await reader.LoadAsync(1);
                byte fieldType = reader.ReadByte();
                if (fieldType == 0)
                {
                    return new VariantDictionary(backingDict);
                }

                await reader.LoadAsync(4);
                uint keyNameBytes = reader.ReadUInt32();

                await reader.LoadAsync(keyNameBytes);
                byte[] keyNameBuffer = new byte[keyNameBytes];
                reader.ReadBytes(keyNameBuffer);
                string keyName = Encoding.UTF8.GetString(keyNameBuffer);

                await reader.LoadAsync(4);
                uint valueBytes = reader.ReadUInt32();

                await reader.LoadAsync(valueBytes);

                object value;
                switch (fieldType)
                {
                    case 0x04: // uint32
                        Dbg.Assert(valueBytes == 4);
                        value = reader.ReadUInt32();
                        break;
                    case 0x05: // uint64
                        Dbg.Assert(valueBytes == 8);
                        value = reader.ReadUInt64();
                        break;
                    case 0x08: // bool
                        Dbg.Assert(valueBytes == 1);
                        byte bitValue = reader.ReadByte();
                        if (bitValue == 0)
                        {
                            value = false;
                        }
                        else if (bitValue == 1)
                        {
                            value = true;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unexpected bool value in VariantDictionary");
                        }
                        break;
                    case 0x0C:
                        Dbg.Assert(valueBytes == 4);
                        value = reader.ReadInt32();
                        break;
                    case 0x0D:
                        Dbg.Assert(valueBytes == 4);
                        value = reader.ReadInt64();
                        break;
                    case 0x18:
                        byte[] valueBuffer = new byte[valueBytes];
                        reader.ReadBytes(valueBuffer);
                        value = Encoding.UTF8.GetString(valueBuffer);
                        break;
                    case 0x42:
                        byte[] buffer = new byte[valueBytes];
                        reader.ReadBytes(buffer);
                        value = buffer;
                        break;
                    default:
                        throw new InvalidOperationException("Unknown VariantDictionary value type");
                }

                backingDict[keyName] = value;
            }
        }
    }
}
