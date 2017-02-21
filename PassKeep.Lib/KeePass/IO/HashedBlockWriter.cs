using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using WinRT = Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.IO
{
    /// <summary>
    /// 4 bytes: Buffer index (uint32, incrementing)
    /// 32 bytes: Hash
    /// 4 bytes: Buffer size (int32)
    ///     If size is 0, the hash must be 0 (or data is invalid)
    ///         At this point we assert EOS
    /// n bytes: Buffer
    ///     The SHA256 of this block should be equal to the hash we got
    /// </summary>
    public static class HashedBlockWriter
    {
        private const uint defaultBufferSize = 1024 * 1024;

        public static async Task<IBuffer> CreateAsync(IBuffer data)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new DataWriter(stream.AsOutputStream()))
                {
                    writer.ByteOrder = ByteOrder.LittleEndian;
                    writer.UnicodeEncoding = WinRT.UnicodeEncoding.Utf8;

                    // Create a reusable hash object.
                    var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                    var hash = sha256.CreateHash();

                    uint currentIndex = 0;
                    uint offset = 0;

                    byte[] bData = data.ToArray();
                    while (offset < data.Length)
                    {
                        // Write index 
                        writer.WriteUInt32(currentIndex++);

                        // Get data
                        uint bytesToCopy = Math.Min(defaultBufferSize, data.Length - offset);
                        IBuffer buffer = bData.AsBuffer((int)offset, (int)bytesToCopy);

                        // Write hash
                        hash.Append(buffer);
                        writer.WriteBuffer(hash.GetValueAndReset());

                        // Write block size
                        writer.WriteUInt32(bytesToCopy);

                        // Write data
                        writer.WriteBuffer(buffer);

                        // Increment offset
                        offset += bytesToCopy;
                    }
                    writer.WriteUInt32(currentIndex);
                    // Write 32 bytes of zeros (0 hash)
                    for (int i = 0; i < 4; i++)
                    {
                        writer.WriteUInt64(0);
                    }
                    // Write 0 size
                    writer.WriteUInt32(0);
                    await writer.StoreAsync();
                }
                return stream.ToArray().AsBuffer();
            }
        }
    }
}
