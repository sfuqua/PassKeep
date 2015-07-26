using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.SecurityTokens;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.IO
{
    public sealed class KdbxWriter : KdbxFileHandler, IKdbxWriter
    {
        private IEnumerable<ISecurityToken> securityTokens;

        /// <summary>
        /// Initializes a new KdbxWriter with the given options.
        /// </summary>
        /// <param name="tokenList">An enumeration of security tokens used for encryption.</param>
        /// <param name="rngAlgorithm">The random number generator used for String protection.</param>
        /// <param name="compression">The document compression algorithm.</param>
        /// <param name="transformRounds">The number of rounds <paramref name="transformedKey"/> went through.</param>
        public KdbxWriter(
                IEnumerable<ISecurityToken> securityTokens,
                RngAlgorithm rngAlgorithm,
                CompressionAlgorithm compression,
                ulong transformRounds
            )
            : base()
        {
            if (securityTokens == null)
            {
                throw new ArgumentNullException(nameof(securityTokens));
            }

            this.securityTokens = securityTokens;
            this.HeaderData = new KdbxHeaderData
            {
                Compression = compression,
                MasterSeed = CryptographicBuffer.GenerateRandom(32),
                TransformSeed = CryptographicBuffer.GenerateRandom(32),
                TransformRounds = transformRounds,
                EncryptionIV = CryptographicBuffer.GenerateRandom(16),
                StreamStartBytes = CryptographicBuffer.GenerateRandom(32),
                ProtectedStreamKey = CryptographicBuffer.GenerateRandom(32).ToArray(),
                InnerRandomStream = rngAlgorithm
            };
        }

        public KdbxHeaderData HeaderData
        {
            get;
            private set;
        }

        /// <summary>
        /// Writes a document to the specified stream.
        /// </summary>
        /// <param name="file">The stream to write to.</param>
        /// <param name="document">The document to write.</param>
        /// <param name="token">A token allowing the operation to be cancelled.</param>
        /// <returns>Whether the write succeeded.</returns>
        public async Task<bool> Write(IOutputStream stream, KdbxDocument document, CancellationToken token)
        {
            Dbg.Assert(stream != null);
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (DataWriter writer = new DataWriter(stream))
            {
                // Configure the DataWriter
                writer.UnicodeEncoding = UnicodeEncoding.Utf8;
                writer.ByteOrder = ByteOrder.LittleEndian;

                // Write the header in-memory first, so that we can generate the header hash.
                // This is because the header hash is the first thing written into the header.
                using (InMemoryRandomAccessStream headerStream = new InMemoryRandomAccessStream())
                {
                    using (DataWriter headerWriter = new DataWriter(headerStream) { UnicodeEncoding = UnicodeEncoding.Utf8, ByteOrder = ByteOrder.LittleEndian })
                    {
                        WriteSignature(headerWriter);
                        WriteVersion(headerWriter);
                        await headerWriter.StoreAsync();

                        await WriteHeader(headerWriter);
                        await headerWriter.StoreAsync();

                        headerWriter.DetachStream();
                    }

                    // Seek to the start of this temporary stream, so we can hash what we have.
                    headerStream.Seek(0);
                    using (DataReader headerReader = new DataReader(headerStream) { UnicodeEncoding = UnicodeEncoding.Utf8, ByteOrder = ByteOrder.LittleEndian })
                    {
                        await headerReader.LoadAsync((uint)headerStream.Size);
                        IBuffer headerBuffer = headerReader.ReadBuffer((uint)headerStream.Size);

                        var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                        var hash = sha256.CreateHash();
                        hash.Append(headerBuffer);

                        IBuffer hashedHeaderBuffer = hash.GetValueAndReset();
                        this.HeaderData.HeaderHash = CryptographicBuffer.EncodeToBase64String(hashedHeaderBuffer);
                        document.Metadata.HeaderHash = this.HeaderData.HeaderHash;

                        XDocument xmlDocument = new XDocument(document.ToXml(this.HeaderData.GenerateRng()));
                        try
                        {
                            IBuffer body = await GetBody(xmlDocument, token);

                            writer.WriteBuffer(headerBuffer);
                            await writer.StoreAsync();
                            token.ThrowIfCancellationRequested();

                            writer.WriteBuffer(body);
                            await writer.StoreAsync();
                            token.ThrowIfCancellationRequested();
                        }
                        catch (OperationCanceledException)
                        {
                            return false;
                        }
                    }
                }

                await stream.FlushAsync();
                writer.DetachStream();
                return true;
            }
        }

        /// <summary>
        /// Generates a writable file in the %temp% directory.
        /// </summary>
        /// <returns>A StorageFile that can be used for temporary writing.</returns>
        private async Task<StorageFile> GetTemporaryFile()
        {
            return await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                String.Format("{0}.kdbx", Guid.NewGuid()),
                CreationCollisionOption.ReplaceExisting
            );
        }

        /// <summary>
        /// Gets the writable body of a KDBX document.
        /// </summary>
        /// <param name="xmlDocument">The XDocument to encrypt.</param>
        /// <param name="token">A CancellationToken used to abort the encryption operation.</param>
        /// <returns>A Task representing an IBuffer representing the encryption result.</returns>
        private async Task<IBuffer> GetBody(XDocument xmlDocument, CancellationToken token)
        {
            using (var memStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memStream, CompressionMode.Compress))
                {
                    Stream writeStream;
                    switch (this.HeaderData.Compression)
                    {
                        case CompressionAlgorithm.GZip:
                            writeStream = gzipStream;
                            break;
                        default:
                            writeStream = memStream;
                            break;
                    }

                    xmlDocument.Save(writeStream);
                }

                IBuffer compressedData = memStream.ToArray().AsBuffer();
                IBuffer hashedData = await HashedBlockWriter.Create(compressedData);

                IBuffer clearFile = (new byte[this.HeaderData.StreamStartBytes.Length + hashedData.Length]).AsBuffer();
                this.HeaderData.StreamStartBytes.CopyTo(0, clearFile, 0, this.HeaderData.StreamStartBytes.Length);
                hashedData.CopyTo(0, clearFile, this.HeaderData.StreamStartBytes.Length, hashedData.Length);

                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                CryptographicHash hash = sha256.CreateHash();
                hash.Append(this.HeaderData.MasterSeed);

                IBuffer raw32 = await KeyHelper.GetRawKey(this.securityTokens);
                IBuffer transformedKey = await KeyHelper.TransformKey(raw32, this.HeaderData.TransformSeed, this.HeaderData.TransformRounds, token);
                if (transformedKey == null)
                {
                    Dbg.Trace("Decryption was cancelled.");
                    return null;
                }

                Dbg.Trace("Successfully got transformed k for encryption.");

                // Hash transformed key k (with the master seed) to get final AES k
                hash.Append(transformedKey);
                IBuffer aesKeyBuffer = hash.GetValueAndReset();
                Dbg.Trace("Got final AES k from transformed k.");

                // Encrypt the data we've generated
                var aes = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
                var key = aes.CreateSymmetricKey(aesKeyBuffer);
                Dbg.Trace("Created SymmetricKey.");

                IBuffer encrypted = CryptographicEngine.Encrypt(key, clearFile, this.HeaderData.EncryptionIV);
                byte[] encBytes = encrypted.ToArray();

                return encrypted;
            }
        }

        private void WriteFieldId(DataWriter writer, KdbxHeaderField field)
        {
            writer.WriteByte((byte)field);
        }

        private void WriteFieldSize(DataWriter writer, ushort size)
        {
            writer.WriteUInt16(size);
        }

        private void WriteFieldSize(DataWriter writer, uint size)
        {
            WriteFieldSize(writer, (ushort)size);
        }

        private void WriteFieldSize(DataWriter writer, int size)
        {
            WriteFieldSize(writer, (ushort)size);
        }

        /// <summary>
        /// Writes out the KDBX header.
        /// </summary>
        /// <param name="writer">The DataWriter to write to.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task WriteHeader(DataWriter writer)
        {
            // We assume AES because that's all the reader supports.
            WriteFieldId(writer, KdbxHeaderField.CipherID);
            WriteFieldSize(writer, 16);
            writer.WriteBytes(AesUuid.Uid.ToByteArray());
            await writer.StoreAsync();

            WriteFieldId(writer, KdbxHeaderField.CompressionFlags);
            WriteFieldSize(writer, 4);
            writer.WriteUInt32((UInt32)this.HeaderData.Compression);
            await writer.StoreAsync();

            WriteFieldId(writer, KdbxHeaderField.MasterSeed);
            WriteFieldSize(writer, 32);
            writer.WriteBuffer(this.HeaderData.MasterSeed);
            await writer.StoreAsync();

            WriteFieldId(writer, KdbxHeaderField.TransformSeed);
            WriteFieldSize(writer, 32);
            writer.WriteBuffer(this.HeaderData.TransformSeed);
            await writer.StoreAsync();

            WriteFieldId(writer, KdbxHeaderField.TransformRounds);
            WriteFieldSize(writer, 8);
            writer.WriteUInt64(this.HeaderData.TransformRounds);
            await writer.StoreAsync();

            WriteFieldId(writer, KdbxHeaderField.EncryptionIV);
            WriteFieldSize(writer, 16);
            writer.WriteBuffer(this.HeaderData.EncryptionIV);
            await writer.StoreAsync();

            WriteFieldId(writer, KdbxHeaderField.ProtectedStreamKey);
            WriteFieldSize(writer, 32);
            writer.WriteBytes(this.HeaderData.ProtectedStreamKey);
            await writer.StoreAsync();

            WriteFieldId(writer, KdbxHeaderField.StreamStartBytes);
            WriteFieldSize(writer, this.HeaderData.StreamStartBytes.Length);
            writer.WriteBuffer(this.HeaderData.StreamStartBytes);
            await writer.StoreAsync();

            WriteFieldId(writer, KdbxHeaderField.InnerRandomStreamID);
            WriteFieldSize(writer, 4);
            writer.WriteUInt32((UInt32)this.HeaderData.InnerRandomStream);
            await writer.StoreAsync();

            WriteFieldId(writer, KdbxHeaderField.EndOfHeader);
            WriteFieldSize(writer, 4);
            writer.WriteByte(0x0D);
            writer.WriteByte(0x0A);
            writer.WriteByte(0x0D);
            writer.WriteByte(0x0A);
            await writer.StoreAsync();
        }

        /// <summary>
        /// Writes the KeePass signature.
        /// </summary>
        /// <param name="writer">The DataWriter to write to.</param>
        private void WriteSignature(DataWriter writer)
        {
            writer.WriteUInt32(SIG1);
            writer.WriteUInt32(SIG2);
        }

        /// <summary>
        /// Writes the KeePass version.
        /// </summary>
        /// <param name="writer">The DataWriter to write to.</param>
        private void WriteVersion(DataWriter writer)
        {
            writer.WriteUInt32(FileVersion32);
        }
    }
}
