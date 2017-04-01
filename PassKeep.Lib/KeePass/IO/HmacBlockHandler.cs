using PassKeep.Lib.Util;
using SariphLib.Infrastructure;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.IO
{
    /// <summary>
    /// Handles HMAC-SHA-256 block parsing and writing used by the KDBX4
    /// file format.
    /// </summary>
    /// <remarks>
    /// The ith block in a stream is:
    /// * 32 bytes; HMAC
    /// * 4 bytes; block size n in bytes (0 to 2^31-1, little endian)
    ///     KeePass 2.35 uses 2^20 byte blocks
    /// * n bytes; ciphertext C
    /// 
    /// The hashed message is i || n || C, where i is 64-bit little endian.
    /// The key for a block, Ki, is SHA-512(i || K) where K is a 512-bit key
    /// derived from the user's composite key and the seed from the header.
    /// </remarks>
    public class HmacBlockHandler
    {
        /// <summary>
        /// Number of bytes of cipher text to write per block.
        /// </summary>
        /// <remarks>
        /// Only used for writing; when reading blocksize is encoded in the
        /// data.
        /// </remarks>
        public static readonly uint BlockSize = 2 << 19; // 2^20, 1 MiB

        /// <summary>
        /// Number of bytes to use in the base HMAC key that is used to 
        /// derive block-specific keys.
        /// </summary>
        public static readonly int BaseKeySize = 64;

        private readonly MacAlgorithmProvider hmacAlgo;
        private readonly IBuffer key;

        /// <summary>
        /// Initializes an instance for the purposes of reading or writing
        /// HMAC blocks with the given key.
        /// </summary>
        /// <param name="key">The <see cref="BaseKeySize"/>-byte key used to
        /// compute block individual keys.</param>
        public HmacBlockHandler(IBuffer key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length != BaseKeySize)
            {
                throw new ArgumentException("The HMAC key must be 512 bits", nameof(key));
            }

            this.hmacAlgo = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
            this.key = key;
        }

        /// <summary>
        /// Derives the base key used by <see cref="HmacBlockHandler"/> given 
        /// a database's master seed and the user's composite key.
        /// 
        /// The derived key is a SHA-512 hash of the result of concatenating 
        /// these values together, with the byte 0x1 appended at the end.
        /// </summary>
        /// <param name="compositeKey">The user's composite key.</param>
        /// <param name="masterSeed">The database's master seed.</param>
        /// <returns>A buffer to use for an HMAC block key.</returns>
        public static IBuffer DeriveHmacKey(IBuffer compositeKey, IBuffer masterSeed)
        {
            if (compositeKey == null)
            {
                throw new ArgumentNullException(nameof(compositeKey));
            }

            if (masterSeed == null)
            {
                throw new ArgumentNullException(nameof(masterSeed));
            }

            if (compositeKey.Length != 32)
            {
                throw new ArgumentException("Composite key should be 32 bytes", nameof(compositeKey));
            }

            if (masterSeed.Length != 32)
            {
                throw new ArgumentException("Master seed should be 32 bytes", nameof(masterSeed));
            }

            byte[] buffer = new byte[compositeKey.Length + masterSeed.Length + 1];

            masterSeed.CopyTo(buffer);
            compositeKey.CopyTo(0, buffer, (int)masterSeed.Length, (int)compositeKey.Length);
            buffer[buffer.Length - 1] = 1;

            HashAlgorithmProvider sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            CryptographicHash hash = sha512.CreateHash();
            hash.Append(buffer.AsBuffer());
            return hash.GetValueAndReset();
        }

        /// <summary>
        /// Generates the HMAC key to use for a specific block.
        /// </summary>
        /// <param name="i">The block index.</param>
        /// <returns>The 512-bit key to use for a block at index <paramref name="i"/>.</returns>
        public IBuffer GetKeyForBlock(ulong i)
        {
            byte[] buffer = new byte[BaseKeySize + 8];
            ByteHelper.GetLittleEndianBytes(i, buffer);
            this.key.CopyTo(0, buffer, 8, BaseKeySize);

            HashAlgorithmProvider sha512 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha512);
            CryptographicHash hash = sha512.CreateHash();
            hash.Append(buffer.AsBuffer());
            return hash.GetValueAndReset();
        }

        /// <summary>
        /// Generates the HMAC-SHA-256 value for a given block.
        /// The actual hashed value is i || n (block size) || C.
        /// </summary>
        /// <param name="i">The block index.</param>
        /// <param name="cipherText">Encrypted block value.</param>
        /// <param name="offset">Offset into <paramref name="cipherText"/>.</param>
        /// <param name="length">Number of bytes of <paramref name="cipherText"/> to use.</param>
        /// <returns>The HMAC value of the block.</returns>
        public IBuffer GetMacForBlock(ulong i, IBuffer cipherText, uint offset, uint length)
        {
            if (cipherText == null)
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            if (offset > cipherText.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            length = (offset + length > cipherText.Length ? cipherText.Length - offset : length);

            Dbg.Assert(length <= int.MaxValue);

            // Construct the HMAC buffer: i || n || C
            byte[] buffer = new byte[8 + 4 + length];

            ByteHelper.GetLittleEndianBytes(i, buffer);
            ByteHelper.GetLittleEndianBytes(length, buffer, 8);
            if (length > 0)
            {
                // Necessary because buffers don't play nice if they're empty...
                cipherText.CopyTo(offset, buffer, 12, (int)length);
            }

            CryptographicHash hash = this.hmacAlgo.CreateHash(GetKeyForBlock(i));
            hash.Append(buffer.AsBuffer());
            IBuffer macValue = hash.GetValueAndReset();

            Dbg.Trace($"Generating MAC value for block #{i}, data length {length}");
            if (length > 0)
            {
                Dbg.Trace($"data[0]: { buffer[12]}, data[n]: { buffer[buffer.Length - 1]}");
            }
            Dbg.Trace($"MAC[0]: {macValue.GetByte(0)}, MAC[31]: {macValue.GetByte(31)}");
            return macValue;
        }

        /// <summary>
        /// Generates the HMAC-SHA-256 value for a given block.
        /// The actual hashed value is i || n (block size) || C.
        /// </summary>
        /// <param name="i">The block index.</param>
        /// <param name="cipherText">Encrypted block value.</param>
        /// <returns>The HMAC value of the block.</returns>
        public IBuffer GetMacForBlock(ulong i, IBuffer cipherText)
        {
            return GetMacForBlock(i, cipherText, 0, cipherText.Length);
        }

        /// <summary>
        /// Attempts to read an HMAC-authenticated cipher block from a given data source.
        /// Returns null on EOS.
        /// </summary>
        /// <param name="reader">Reader used to access data.</param>
        /// <param name="blockIndex">The index of this block.</param>
        /// <returns>A buffer of read data.</returns>
        public async Task<IBuffer> ReadCipherBlockAsync(DataReader reader, ulong blockIndex)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            uint loadedBytes = await reader.LoadAsync(32);
            if (loadedBytes == 0)
            {
                return null;
            }

            if (loadedBytes < 32)
            {
                throw new FormatException("Couldn't load 32 bytes for HMAC value");
            }

            IBuffer actualHmacValue = reader.ReadBuffer(32);

            loadedBytes = await reader.LoadAsync(4);
            if (loadedBytes < 4)
            {
                throw new FormatException("Couldn't load 4 bytes for block size");
            }

            int blockSize = reader.ReadInt32();
            if (blockSize == 0)
            {
                return null;
            }
            else if (blockSize < 0)
            {
                throw new FormatException($"Block size must be a positive int32, got {blockSize}");
            }

            loadedBytes = await reader.LoadAsync((uint)blockSize);
            if (loadedBytes < blockSize)
            {
                throw new FormatException($"Expected to load {blockSize} bytes for block, only got {loadedBytes}");
            }

            IBuffer cipherText = reader.ReadBuffer(loadedBytes);
            IBuffer expectedHmacValue = GetMacForBlock(blockIndex, cipherText);

            // Validate HMAC
            if (expectedHmacValue.Length != actualHmacValue.Length)
            {
                throw new FormatException("Expected and actual HMAC values had different lengths");
            }

            Dbg.Trace($"Read HMAC block #{blockIndex}; block is {loadedBytes} bytes.");
            Dbg.Trace($"MAC[0]: {actualHmacValue.GetByte(0)}, MAC[31]: {actualHmacValue.GetByte(31)}, data[0]: {cipherText.GetByte(0)}, data[n]: {cipherText.GetByte(loadedBytes - 1)}");

            for (uint i = 0; i < expectedHmacValue.Length; i++)
            {
                if (expectedHmacValue.GetByte(i) != actualHmacValue.GetByte(i))
                {
                    throw new FormatException($"HMAC values differed at index {i}");
                }
            }

            // Ciphertext was validated
            return cipherText;
        }

        /// <summary>
        /// Hashes and writes the provided data to the provided stream.
        /// </summary>
        /// <param name="writer">Writer used to output data.</param>
        /// <param name="cipherText">The data block to write.</param>
        /// <param name="offset">Offset into <paramref name="cipherText"/>.</param>
        /// <param name="length">Number of bytes of <paramref name="cipherText"/> to use.</param>
        /// <param name="blockIndex">Block index used to compute HMAC key.</param>
        /// <returns>A task that resolves when output is finished.</returns>
        public async Task WriteCipherBlockAsync(DataWriter writer, IBuffer cipherText, uint offset, uint length, ulong blockIndex)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (cipherText == null)
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            if (cipherText.Length > int.MaxValue)
            {
                throw new ArgumentException("Block size is limited to int.MaxValue", nameof(cipherText));
            }

            if (offset >= cipherText.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            
            length = (offset + length > cipherText.Length ? cipherText.Length - offset : length);

            IBuffer hmacValue = GetMacForBlock(blockIndex, cipherText, offset, length);
            writer.WriteBuffer(hmacValue);
            await writer.StoreAsync();

            writer.WriteUInt32(length);
            await writer.StoreAsync();

            writer.WriteBuffer(cipherText, offset, length);
            await writer.StoreAsync();

            Dbg.Trace($"Wrote HMAC block #{blockIndex}; block is {length} bytes.");
            Dbg.Trace($"MAC[0]: {hmacValue.GetByte(0)}, MAC[31]: {hmacValue.GetByte(31)}, data[0]: {cipherText.GetByte(offset)}, data[n]: {cipherText.GetByte(offset + length - 1)}");
        }

        /// <summary>
        /// Hashes and writes the provided data to the provided stream.
        /// </summary>
        /// <param name="writer">Writer used to output data.</param>
        /// <param name="cipherText">The data block to write.</param>
        /// <param name="blockIndex">Block index used to compute HMAC key.</param>
        /// <returns>A task that resolves when output is finished.</returns>
        public Task WriteCipherBlockAsync(DataWriter writer, IBuffer cipherText, ulong blockIndex)
        {
            return WriteCipherBlockAsync(writer, cipherText, 0, cipherText.Length, blockIndex);
        }

        /// <summary>
        /// Hashes and writes a null terminator block to the specified writer.
        /// </summary>
        /// <param name="writer">Writer used to output data.</param>
        /// <param name="blockIndex">Block index used to compute HMAC key.</param>
        /// <returns>A task that resolves when output is finished.</returns>
        public async Task WriteTerminatorAsync(DataWriter writer, ulong blockIndex)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            IBuffer nullData = WindowsRuntimeBuffer.Create(0);
            IBuffer hmacValue = GetMacForBlock(blockIndex, nullData, 0, 0);
            writer.WriteBuffer(hmacValue);
            await writer.StoreAsync();

            writer.WriteUInt32(0);
            await writer.StoreAsync();

            Dbg.Trace($"Wrote HMAC terminator block #{blockIndex}.");
            Dbg.Trace($"MAC[0]: {hmacValue.GetByte(0)}, MAC[31]: {hmacValue.GetByte(31)}");
        }
    }
}
