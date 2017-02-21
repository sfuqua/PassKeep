using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.DatabaseCiphers;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Kdf;
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
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.IO
{
    public sealed class KdbxWriter : KdbxFileHandler, IKdbxWriter
    {
        private readonly KdbxSerializationParameters parameters;
        private IEnumerable<ISecurityToken> securityTokens;
        private IBuffer rawKey;

        /// <summary>
        /// Initializes a new KdbxWriter with the given options.
        /// </summary>
        /// <param name="rawKey">The raw 32-byte key for encryption.</param>
        /// <param name="cipher">The algorithm to use for encrypting the database.</param>
        /// <param name="rngAlgorithm">The random number generator used for String protection.</param>
        /// <param name="compression">The document compression algorithm.</param>
        /// <param name="kdfParams">Recipe for transforming <paramref name="rawKey"/>.</param>
        public KdbxWriter(
                IBuffer rawKey,
                EncryptionAlgorithm cipher,
                RngAlgorithm rngAlgorithm,
                CompressionAlgorithm compression,
                KdfParameters kdfParams
            )
            : this(cipher, rngAlgorithm, compression, kdfParams)
        {
            if (rawKey == null)
            {
                throw new ArgumentNullException(nameof(rawKey));
            }

            if (rawKey.Length != 32)
            {
                throw new ArgumentException("Raw key must be 32 bytes", nameof(rawKey));
            }

            this.rawKey = rawKey;
        }

        /// <summary>
        /// Initializes a new KdbxWriter with the given options.
        /// </summary>
        /// <param name="tokenList">An enumeration of security tokens used for encryption.</param>
        /// <param name="cipher">The algorithm to use for encrypting the database.</param>
        /// <param name="rngAlgorithm">The random number generator used for String protection.</param>
        /// <param name="compression">The document compression algorithm.</param>
        /// <param name="kdfParams">Recipe for transforming the raw key.</param>
        public KdbxWriter(
                IEnumerable<ISecurityToken> securityTokens,
                EncryptionAlgorithm cipher,
                RngAlgorithm rngAlgorithm,
                CompressionAlgorithm compression,
                KdfParameters kdfParams
            )
            : this(cipher, rngAlgorithm, compression, kdfParams)
        {
            if (securityTokens == null)
            {
                throw new ArgumentNullException(nameof(securityTokens));
            }

            this.securityTokens = securityTokens;
        }

        /// <summary>
        /// Initializes a new KdbxWriter with the given options.
        /// </summary>
        /// <param name="cipher">The algorithm to use for encrypting the database.</param>
        /// <param name="rngAlgorithm">The random number generator used for String protection.</param>
        /// <param name="compression">The document compression algorithm.</param>
        /// <param name="kdfParams">Recipe for transforming the raw key. This will be reseeded.</param>
        private KdbxWriter(
            EncryptionAlgorithm cipher,
            RngAlgorithm rngAlgorithm,
            CompressionAlgorithm compression,
            KdfParameters kdfParams
        )
        : base()
        {
            if (kdfParams == null)
            {
                throw new ArgumentNullException(nameof(kdfParams));
            }

            uint ivBytes;
            if (cipher == EncryptionAlgorithm.Aes)
            {
                ivBytes = AesCipher.IvBytes;
            }
            else
            {
                Dbg.Assert(cipher == EncryptionAlgorithm.ChaCha20);
                ivBytes = ChaCha20Cipher.IvBytes;
            }

            HeaderData = new KdbxHeaderData
            {
                Cipher = cipher,
                Compression = compression,
                MasterSeed = CryptographicBuffer.GenerateRandom(32),
                KdfParameters = kdfParams.Reseed(),
                EncryptionIV = CryptographicBuffer.GenerateRandom(ivBytes),
                StreamStartBytes = CryptographicBuffer.GenerateRandom(32),
                InnerRandomStreamKey = CryptographicBuffer.GenerateRandom(32).ToArray(),
                InnerRandomStream = rngAlgorithm
            };

            KdbxVersion version = KdbxVersion.Three;
            if (cipher == EncryptionAlgorithm.ChaCha20 || rngAlgorithm == RngAlgorithm.ChaCha20
                || !kdfParams.Uuid.Equals(AesParameters.AesUuid))
            {
                Dbg.Trace("Useing KDBX4 for serialization due to header parameters");
                version = KdbxVersion.Four;
            }

            this.parameters = new KdbxSerializationParameters(version)
            {
                Compression = HeaderData.Compression
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
                throw new ArgumentNullException(nameof(stream));
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
                        HeaderData.FullHeader = headerReader.ReadBuffer((uint)headerStream.Size);

                        var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                        var hash = sha256.CreateHash();
                        hash.Append(HeaderData.FullHeader);

                        IBuffer hashedHeaderBuffer = hash.GetValueAndReset();
                        HeaderData.HeaderHash = CryptographicBuffer.EncodeToBase64String(hashedHeaderBuffer);
                        document.Metadata.HeaderHash = HeaderData.HeaderHash;

                        XDocument xmlDocument = new XDocument(document.ToXml(HeaderData.GenerateRng(), new KdbxSerializationParameters(KdbxVersion.Three)));
                        try
                        {
                            this.rawKey = this.rawKey ?? await KeyHelper.GetRawKey(this.securityTokens);
                            IBuffer transformedKey = await HeaderData.KdfParameters.CreateEngine().TransformKeyAsync(this.rawKey, token);
                            if (transformedKey == null)
                            {
                                throw new OperationCanceledException();
                            }

                            Dbg.Trace("Got transformed k from KDF.");
                            
                            token.ThrowIfCancellationRequested();

                            writer.WriteBuffer(HeaderData.FullHeader);
                            await writer.StoreAsync();
                            token.ThrowIfCancellationRequested();

                            if (this.parameters.UseInlineHeaderAuthentication)
                            {
                                // In KDBX4, after the header is an HMAC-SHA-256 value computed over the header
                                // allowing validation of header integrity. 
                                IBuffer hmacKey = HmacBlockHandler.DeriveHmacKey(transformedKey, HeaderData.MasterSeed);
                                HmacBlockHandler hmacHandler = new HmacBlockHandler(hmacKey);

                                // Write plain hash, followed by HMAC
                                writer.WriteBuffer(hashedHeaderBuffer);

                                var algorithm = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
                                var hmacHash = algorithm.CreateHash(hmacHandler.GetKeyForBlock(ulong.MaxValue));
                                hmacHash.Append(HeaderData.FullHeader);

                                IBuffer headerMac = hmacHash.GetValueAndReset();
                                writer.WriteBuffer(headerMac);

                                token.ThrowIfCancellationRequested();
                            }

                            IBuffer body = await GetBody(xmlDocument, transformedKey, token);
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
        /// Gets the writable body of a KDBX document.
        /// </summary>
        /// <param name="xmlDocument">The XDocument to encrypt.</param>
        /// <param name="transformedKey">The transformed user key for encryption.</param>
        /// <param name="token">A CancellationToken used to abort the encryption operation.</param>
        /// <returns>A Task representing an IBuffer representing the encryption result.</returns>
        private async Task<IBuffer> GetBody(XDocument xmlDocument, IBuffer transformedKey, CancellationToken token)
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
                IBuffer hashedData = await HashedBlockWriter.CreateAsync(compressedData);

                IBuffer clearFile = (new byte[this.HeaderData.StreamStartBytes.Length + hashedData.Length]).AsBuffer();
                this.HeaderData.StreamStartBytes.CopyTo(0, clearFile, 0, this.HeaderData.StreamStartBytes.Length);
                hashedData.CopyTo(0, clearFile, this.HeaderData.StreamStartBytes.Length, hashedData.Length);

                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                CryptographicHash hash = sha256.CreateHash();
                hash.Append(this.HeaderData.MasterSeed);

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

        private void WriteFieldId(DataWriter writer, OuterHeaderField field)
        {
            writer.WriteByte((byte)field);
        }

        private void WriteFieldSize(DataWriter writer, uint size)
        {
            if (this.parameters.HeaderFieldSizeBytes == 2)
            {
                Dbg.Assert(size <= ushort.MaxValue);
                writer.WriteUInt16((ushort)size);
            }
            else
            {
                Dbg.Assert(this.parameters.HeaderFieldSizeBytes == 4);
                writer.WriteUInt32(size);
            }
        }

        /// <summary>
        /// Writes out the KDBX header.
        /// </summary>
        /// <param name="writer">The DataWriter to write to.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task WriteHeader(DataWriter writer)
        {
            // We assume AES because that's all the reader supports.
            WriteFieldId(writer, OuterHeaderField.CipherID);
            WriteFieldSize(writer, 16);
            writer.WriteBytes(GetCipherUuid(HeaderData.Cipher).ToByteArray());
            await writer.StoreAsync();

            WriteFieldId(writer, OuterHeaderField.CompressionFlags);
            WriteFieldSize(writer, 4);
            writer.WriteUInt32((uint)HeaderData.Compression);
            await writer.StoreAsync();

            WriteFieldId(writer, OuterHeaderField.MasterSeed);
            WriteFieldSize(writer, 32);
            writer.WriteBuffer(HeaderData.MasterSeed);
            await writer.StoreAsync();

            if (!this.parameters.UseExtensibleKdf)
            {
                AesParameters kdfParams = HeaderData.KdfParameters as AesParameters;
                Dbg.Assert(kdfParams != null);

                WriteFieldId(writer, OuterHeaderField.TransformSeed);
                WriteFieldSize(writer, 32);
                writer.WriteBuffer(kdfParams.Seed);
                await writer.StoreAsync();

                WriteFieldId(writer, OuterHeaderField.TransformRounds);
                WriteFieldSize(writer, 8);
                writer.WriteUInt64(kdfParams.Rounds);
                await writer.StoreAsync();
            }
            else
            {
                WriteFieldId(writer, OuterHeaderField.KdfParameters);

                VariantDictionary kdfDict = HeaderData.KdfParameters.ToVariantDictionary();
                WriteFieldSize(writer, (uint)kdfDict.GetSerializedSize());
                await kdfDict.WriteToAsync(writer);
            }

            WriteFieldId(writer, OuterHeaderField.EncryptionIV);
            WriteFieldSize(writer, HeaderData.EncryptionIV.Length);
            writer.WriteBuffer(HeaderData.EncryptionIV);
            await writer.StoreAsync();

            if (!this.parameters.UseHmacBlocks)
            {
                WriteFieldId(writer, OuterHeaderField.StreamStartBytes);
                WriteFieldSize(writer, this.HeaderData.StreamStartBytes.Length);
                writer.WriteBuffer(this.HeaderData.StreamStartBytes);
                await writer.StoreAsync();
            }

            if (!this.parameters.UseInnerHeader)
            {
                WriteFieldId(writer, OuterHeaderField.InnerRandomStreamID);
                WriteFieldSize(writer, 4);
                writer.WriteUInt32((UInt32)this.HeaderData.InnerRandomStream);
                await writer.StoreAsync();

                WriteFieldId(writer, OuterHeaderField.ProtectedStreamKey);
                WriteFieldSize(writer, 32);
                writer.WriteBytes(this.HeaderData.InnerRandomStreamKey);
                await writer.StoreAsync();
            }

            WriteFieldId(writer, OuterHeaderField.EndOfHeader);
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
            writer.WriteUInt32(FileVersion32_3);
        }

        /// <summary>
        /// Maps the <see cref="EncryptionAlgorithm"/> enum to cipher GUIDs for serialization.
        /// </summary>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        private Guid GetCipherUuid(EncryptionAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case EncryptionAlgorithm.Aes:
                    return AesCipher.Uuid;
                case EncryptionAlgorithm.ChaCha20:
                    return ChaCha20Cipher.Uuid;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Maps a cipher UUID to <see cref="EncryptionAlgorithm"/> values.
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>
        public EncryptionAlgorithm GetCipher(Guid uuid)
        {
            if (uuid.Equals(AesCipher.Uuid))
            {
                return EncryptionAlgorithm.Aes;
            }
            else
            {
                Dbg.Assert(uuid.Equals(ChaCha20Cipher.Uuid));
                return EncryptionAlgorithm.ChaCha20;
            }
        }
    }
}
