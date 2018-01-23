// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.KeePass.DatabaseCiphers;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Kdf;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.KeePass.SecurityTokens;
using PassKeep.Lib.Models;
using PassKeep.Lib.Util;
using SariphLib.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
            this.securityTokens = securityTokens ?? throw new ArgumentNullException(nameof(securityTokens));
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

            KdbxVersion version = KdbxVersion.Three;
            if (cipher == EncryptionAlgorithm.ChaCha20 || rngAlgorithm == RngAlgorithm.ChaCha20
                || !kdfParams.Uuid.Equals(AesParameters.AesUuid))
            {
                DebugHelper.Trace("Useing KDBX4 for serialization due to header parameters");
                version = KdbxVersion.Four;
            }

            this.parameters = new KdbxSerializationParameters(version)
            {
                Compression = compression
            };

            // "Stream start bytes" are random data encrypted at the beginning
            // of the KDBX data block. They have been superceded by HMAC authentication.
            IBuffer streamStartBytes;
            if (this.parameters.UseHmacBlocks)
            {
                streamStartBytes = new byte[0].AsBuffer();
            }
            else
            {
                streamStartBytes = CryptographicBuffer.GenerateRandom(32);
            }

            HeaderData = new KdbxHeaderData(KdbxHeaderData.Mode.Write)
            {
                Cipher = cipher, // This will automatically set EncryptionIV
                Compression = compression,
                MasterSeed = CryptographicBuffer.GenerateRandom(32),
                KdfParameters = kdfParams.Reseed(),
                StreamStartBytes = streamStartBytes,
                InnerRandomStreamKey = CryptographicBuffer.GenerateRandom(32).ToArray(),
                InnerRandomStream = rngAlgorithm
            };
        }

        public KdbxHeaderData HeaderData
        {
            get;
            private set;
        }

        /// <summary>
        /// Configures the cipher used to encrypt the database.
        /// </summary>
        EncryptionAlgorithm IDatabaseSettingsProvider.Cipher
        {
            get
            {
                return HeaderData.Cipher;
            }
            set
            {
                HeaderData.Cipher = value;
            }
        }

        /// <summary>
        /// Configures the parameters used to transform the key.
        /// </summary>
        KdfParameters IDatabaseSettingsProvider.KdfParameters
        {
            get
            {
                return HeaderData.KdfParameters;
            }
            set
            {
                HeaderData.KdfParameters = value;
            }
        }

        /// <summary>
        /// Updates the security tokens that will be used to persist this databae.
        /// </summary>
        /// <param name="tokens">The tokens to use.</param>
        public void UpdateSecurityTokens(IEnumerable<ISecurityToken> tokens)
        {
            this.securityTokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            this.rawKey = null;
        }

        /// <summary>
        /// Writes a document to the specified stream.
        /// </summary>
        /// <param name="file">The stream to write to.</param>
        /// <param name="document">The document to write.</param>
        /// <param name="token">A token allowing the operation to be cancelled.</param>
        /// <returns>Whether the write succeeded.</returns>
        public async Task<bool> WriteAsync(IOutputStream stream, KdbxDocument document, CancellationToken token)
        {
            DebugHelper.Assert(stream != null);
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            HeaderData.ProtectedBinaries.Clear();
            foreach (ProtectedBinary bin in document?.Metadata?.Binaries?.Binaries ?? Enumerable.Empty<ProtectedBinary>())
            {
                HeaderData.ProtectedBinaries.Add(bin);
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

                        await WriteOuterHeaderAsync(headerWriter);
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
                        CryptographicHash hash = sha256.CreateHash();
                        hash.Append(HeaderData.FullHeader);

                        IBuffer hashedHeaderBuffer = hash.GetValueAndReset();
                        HeaderData.HeaderHash = CryptographicBuffer.EncodeToBase64String(hashedHeaderBuffer);
                        document.Metadata.HeaderHash = HeaderData.HeaderHash;

                        XDocument xmlDocument = new XDocument(document.ToXml(HeaderData.GenerateRng(), this.parameters));
                        try
                        {
                            this.rawKey = this.rawKey ?? await KeyHelper.GetRawKey(this.securityTokens);
                            IBuffer transformedKey = await HeaderData.KdfParameters.CreateEngine().TransformKeyAsync(this.rawKey, token);
                            if (transformedKey == null)
                            {
                                throw new OperationCanceledException();
                            }

                            DebugHelper.Trace("Got transformed k from KDF.");
                            
                            token.ThrowIfCancellationRequested();

                            writer.WriteBuffer(HeaderData.FullHeader);
                            await writer.StoreAsync();
                            token.ThrowIfCancellationRequested();

                            // In KDBX4, after the header is an HMAC-SHA-256 value computed over the header
                            // allowing validation of header integrity. 
                            IBuffer hmacKey = HmacBlockHandler.DeriveHmacKey(transformedKey, HeaderData.MasterSeed);
                            HmacBlockHandler hmacHandler = new HmacBlockHandler(hmacKey);

                            if (this.parameters.UseInlineHeaderAuthentication)
                            {
                                // Write plain hash, followed by HMAC
                                writer.WriteBuffer(hashedHeaderBuffer);

                                var algorithm = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
                                CryptographicHash hmacHash = algorithm.CreateHash(hmacHandler.GetKeyForBlock(UInt64.MaxValue));
                                hmacHash.Append(HeaderData.FullHeader);

                                IBuffer headerMac = hmacHash.GetValueAndReset();
                                writer.WriteBuffer(headerMac);

                                token.ThrowIfCancellationRequested();
                            }

                            // Write the encrypted content that comes after the header
                            // For KDBX3 this is the database, for KDBX4 it includes the inner header
                            IBuffer cipherText = await GetCipherTextAsync(xmlDocument, transformedKey, token);
                            if (this.parameters.UseHmacBlocks)
                            {
                                uint blockCount = (cipherText.Length + HmacBlockHandler.BlockSize - 1) / HmacBlockHandler.BlockSize;
                                for (uint i = 0; i < blockCount; i++)
                                {
                                    await hmacHandler.WriteCipherBlockAsync(writer, cipherText, i * HmacBlockHandler.BlockSize, HmacBlockHandler.BlockSize, i);
                                }

                                // We signal we're done by writing an empty HMAC "terminator" block
                                await hmacHandler.WriteTerminatorAsync(writer, blockCount);
                            }
                            else
                            {
                                writer.WriteBuffer(cipherText);
                                await writer.StoreAsync();
                            }
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
        /// Gets the writable encrypted content (potentially inner header, plus body) of a KDBX document.
        /// </summary>
        /// <param name="xmlDocument">The XDocument to encrypt.</param>
        /// <param name="transformedKey">The transformed user key for encryption.</param>
        /// <param name="token">A CancellationToken used to abort the encryption operation.</param>
        /// <returns>A Task representing an IBuffer representing the encryption result.</returns>
        private async Task<IBuffer> GetCipherTextAsync(XDocument xmlDocument, IBuffer transformedKey, CancellationToken token)
        {
            using (var memStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memStream, CompressionMode.Compress))
                {
                    Stream writeStream;
                    switch (this.parameters.Compression)
                    {
                        case CompressionAlgorithm.GZip:
                            writeStream = gzipStream;
                            break;
                        default:
                            writeStream = memStream;
                            break;
                    }

                    if (this.parameters.UseInnerHeader)
                    {
                        using (DataWriter writer = new DataWriter(writeStream.AsOutputStream())
                        {
                            ByteOrder = ByteOrder.LittleEndian,
                            UnicodeEncoding = UnicodeEncoding.Utf8
                        })
                        {
                            await WriteInnerHeaderAsync(writer);
                            writer.DetachStream();
                        }
                    }

                    xmlDocument.Save(writeStream);
                }

                // We have now written the inner header and the XML, and if necessary,
                // compressed it.
                // Now, if necessary, partition the data into hashed blocks.
                IBuffer compressedData = memStream.ToArray().AsBuffer();
                IBuffer hashedData;
                if (this.parameters.UseLegacyHashedBlocks)
                {
                    hashedData = await HashedBlockWriter.CreateAsync(compressedData);
                }
                else
                {
                    hashedData = compressedData;
                }

                // We now have plaintext mostly ready to encrypt. We create a new buffer "clearText" 
                // which (might) have bonus plaintext bytes for data authentication.
                IBuffer clearText = (new byte[HeaderData.StreamStartBytes.Length + hashedData.Length]).AsBuffer();
                if (HeaderData.StreamStartBytes.Length > 0)
                {
                    HeaderData.StreamStartBytes.CopyTo(0, clearText, 0, HeaderData.StreamStartBytes.Length);
                }

                // Copy remaining "real" data (inner header + KDBX)
                hashedData.CopyTo(0, clearText, HeaderData.StreamStartBytes.Length, hashedData.Length);

                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                CryptographicHash hash = sha256.CreateHash();
                hash.Append(HeaderData.MasterSeed);

                // Hash transformed key k (with the master seed) to get final cipher k
                hash.Append(transformedKey);
                IBuffer cipherKey = hash.GetValueAndReset();
                DebugHelper.Trace("Got final cipher k from transformed k.");

                if (HeaderData.Cipher == EncryptionAlgorithm.Aes)
                {
                    // Encrypt the data we've generated
                    var aes = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
                    CryptographicKey key = aes.CreateSymmetricKey(cipherKey);
                    DebugHelper.Trace("Created SymmetricKey for AES.");

                    IBuffer encrypted = CryptographicEngine.Encrypt(key, clearText, HeaderData.EncryptionIV);
                    return encrypted;
                }
                else
                {
                    DebugHelper.Assert(HeaderData.Cipher == EncryptionAlgorithm.ChaCha20);
                    ChaCha20 c = new ChaCha20(cipherKey.ToArray(), HeaderData.EncryptionIV.ToArray(), 0);
                    byte[] pad = c.GetBytes(clearText.Length);
                    byte[] cipherData = clearText.ToArray();
                    ByteHelper.Xor(pad, 0, cipherData, 0, pad.Length);

                    return cipherData.AsBuffer();
                }
            }
        }

        private void WriteFieldId(DataWriter writer, OuterHeaderField field)
        {
            writer.WriteByte((byte)field);
        }

        private void WriteFieldId(DataWriter writer, InnerHeaderField field)
        {
            writer.WriteByte((byte)field);
        }

        private void WriteFieldSize(DataWriter writer, uint size)
        {
            if (this.parameters.HeaderFieldSizeBytes == 2)
            {
                DebugHelper.Assert(size <= UInt16.MaxValue);
                writer.WriteUInt16((ushort)size);
            }
            else
            {
                DebugHelper.Assert(this.parameters.HeaderFieldSizeBytes == 4);
                writer.WriteUInt32(size);
            }
        }

        /// <summary>
        /// Writes out the KDBX header.
        /// </summary>
        /// <param name="writer">The DataWriter to write to.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task WriteOuterHeaderAsync(DataWriter writer)
        {
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
                DebugHelper.Assert(kdfParams != null);

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
                WriteFieldSize(writer, HeaderData.StreamStartBytes.Length);
                writer.WriteBuffer(HeaderData.StreamStartBytes);
                await writer.StoreAsync();
            }

            if (!this.parameters.UseInnerHeader)
            {
                WriteFieldId(writer, OuterHeaderField.ProtectedStreamKey);
                WriteFieldSize(writer, 32);
                writer.WriteBytes(HeaderData.InnerRandomStreamKey);
                await writer.StoreAsync();

                WriteFieldId(writer, OuterHeaderField.InnerRandomStreamID);
                WriteFieldSize(writer, 4);
                writer.WriteUInt32((uint)HeaderData.InnerRandomStream);
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
        /// Writes out the encrypted KDBX inner header.
        /// </summary>
        /// <param name="writer">The DataWriter to write to.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task WriteInnerHeaderAsync(DataWriter writer)
        {
            WriteFieldId(writer, InnerHeaderField.InnerRandomStreamID);
            WriteFieldSize(writer, 4);
            writer.WriteUInt32((uint)HeaderData.InnerRandomStream);
            await writer.StoreAsync();

            WriteFieldId(writer, InnerHeaderField.InnerRandomStreamKey);
            WriteFieldSize(writer, 32);
            writer.WriteBytes(HeaderData.InnerRandomStreamKey);
            await writer.StoreAsync();

            foreach (ProtectedBinary bin in HeaderData.ProtectedBinaries)
            {
                WriteFieldId(writer, InnerHeaderField.Binary);

                KdbxBinaryFlags flags = KdbxBinaryFlags.None;
                if (bin.ProtectionRequested)
                {
                    flags |= KdbxBinaryFlags.MemoryProtected;
                }
                byte[] data = bin.GetClearData();

                WriteFieldSize(writer, 1 + (uint)data.Length);
                writer.WriteByte((byte)flags);
                writer.WriteBytes(data);
                await writer.StoreAsync();
            }

            WriteFieldId(writer, InnerHeaderField.EndOfHeader);
            WriteFieldSize(writer, 0);
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
            if (this.parameters.Version == KdbxVersion.Three)
            {
                writer.WriteUInt32(FileVersion32_3);
            }
            else
            {
                DebugHelper.Assert(this.parameters.Version == KdbxVersion.Four);
                writer.WriteUInt32(FileVersion32_4);
            }
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
                DebugHelper.Assert(uuid.Equals(ChaCha20Cipher.Uuid));
                return EncryptionAlgorithm.ChaCha20;
            }
        }
    }
}
