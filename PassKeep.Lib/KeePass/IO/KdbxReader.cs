using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.DatabaseCiphers;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Kdf;
using PassKeep.Lib.KeePass.SecurityTokens;
using SariphLib.Files;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.IO
{
    public sealed class KdbxReader : KdbxFileHandler, IKdbxReader
    {
        private KdbxVersion parserVersion;

        // Number of bytes in encryption IV header;
        // can change depending on encryption algorithm selected.
        private uint expectedIvBytes = 16U;

        // Used to drive the database cipher key
        private KdfParameters kdfParams;

        // Cached security tokens, used to generate a compatible KdbxWriter
        private IBuffer rawKey;

        // "headerInitializationMap" is a lookup table used to determine if 
        // a header field has been seen in the file or not.
        // This code initializes every field to false.
        private readonly Dictionary<KdbxHeaderField, bool> headerInitializationMap =
            new Dictionary<KdbxHeaderField, bool>();

        public KdbxReader()
        {
            this.HeaderData = null;
            this.rawKey = null;
        }

        /// <summary>
        /// Allows read access to the parsed header data of the document, if ReadHeader has been called successfully.
        /// Otherwise, returns null.
        /// </summary>
        public KdbxHeaderData HeaderData
        {
            get;
            private set;
        }

        public IKdbxWriter GetWriter()
        {
            if (this.HeaderData == null)
            {
                throw new InvalidOperationException("Cannot generate a KdbxWriter when the header hasn't even been validated");
            }

            if (this.rawKey == null)
            {
                throw new InvalidOperationException("Cannot generate a KdbxWriter; there are no known security tokens");
            }

            return new KdbxWriter(
                this.rawKey,
                this.HeaderData.InnerRandomStream,
                this.HeaderData.Compression,
                this.HeaderData.TransformRounds
            );
        }

        /// <summary>
        /// Asynchronously attempts to unlock the document file.
        /// </summary>
        /// <remarks>
        /// Algorithm is as of this writing (11/5/2012):
        /// 0. Use UTF8 encoding with no BOM.
        /// 1. Read header.
        /// 2. Compute SHA256 hash of header.
        /// 3. Decrypt the rest of the viewModel using header parameters.
        ///     Relies on:
        ///         a. MasterSeed.Length == 32
        ///             Write masterseed to stream
        ///         b. GenerateKey32(_transformSeed, KeyEncryptionRounds)
        ///             Create raw32 (CreateRawCompositeKey32)
        ///                 Concatenate all data and Sha256
        ///             TransformKey(raw32, _transformSeed, numRounds)
        ///                 Init Rijndael:
        ///                     128 bit (16 byte) blocks
        ///                     ECB mode
        ///                     k = _transformSeed
        ///                     For numRounds:
        ///                         Transform in place raw32[0:15]
        ///                         Transform in place raw32[16:31]
        ///         c. Write 32 bytes of Key32 to stream
        ///         d. aesKey = Sha256 the stream
        ///         e. DecryptStream with aesKey and _encryptionIV
        /// 4. Verify the first 32 bytes of the decrypted viewModel match up with
        ///     "StreamStartBytes" from the header.
        /// 5. Read from the decrypted viewModel as a "HashedBlockStream"
        /// 
        /// File format at the time of this writing (11/5/2012):
        /// 
        /// 4 bytes: SIG1
        /// 4 bytes: SIG2
        /// Failure to match these constants results in a parse Result.
        /// 
        /// 4 bytes: File version
        /// 
        /// Header fields:
        /// 1 byte: Field ID
        /// 2 bytes: Field size (n)
        /// n bytes: Data
        /// </remarks>
        /// <param name="stream">An IRandomAccessStream containing the document to unlock (including the header).</param>
        /// <param name="rawKey">The aggregate raw key to use for decrypting the database.</param>
        /// <param name="token">A token allowing the parse to be cancelled.</param>
        /// <returns>A Task representing the result of the descryiption operation.</returns>
        public async Task<KdbxDecryptionResult> DecryptFile(IRandomAccessStream stream, IBuffer rawKey, CancellationToken token)
        {
            if (this.HeaderData == null)
            {
                throw new InvalidOperationException("Cannot decrypt database before ReadHeader has been called.");
            }

            // Init a SHA256 hash buffer and append the master seed to it
            HashAlgorithmProvider sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            CryptographicHash hash = sha256.CreateHash();
            hash.Append(this.HeaderData.MasterSeed);

            this.rawKey = rawKey;
            Dbg.Trace("Got raw k.");

            // Transform the key (this can take a while)
            // this.HeaderData.TransformSeed, this.HeaderData.TransformRounds
            IBuffer transformedKey;
            try
            {
                transformedKey = await this.kdfParams.CreateEngine().TransformKeyAsync(rawKey, token)
                    .ConfigureAwait(false);
                if (transformedKey == null)
                {
                    throw new OperationCanceledException();
                }

                Dbg.Trace("Got transformed k from KDF.");
            }
            catch (OperationCanceledException)
            {
                return new KdbxDecryptionResult(new ReaderResult(KdbxParserCode.OperationCancelled));
            }

            // In KDBX4, after the header is an HMAC-SHA-256 value computed over the header
            // allowing validation of header integrity. 
            IBuffer hmacKey = HmacBlockHandler.DeriveHmacKey(transformedKey, this.HeaderData.MasterSeed);
            HmacBlockHandler hmacHandler = new HmacBlockHandler(hmacKey);

            IBuffer expectedMac = null;
            if (this.parserVersion >= KdbxVersion.Four)
            {
                var algorithm = MacAlgorithmProvider.OpenAlgorithm(MacAlgorithmNames.HmacSha256);
                var hmacHash = algorithm.CreateHash(hmacHandler.GetKeyForBlock(ulong.MaxValue));

                Dbg.Assert(HeaderData.FullHeader != null);
                hmacHash.Append(HeaderData.FullHeader);

                expectedMac = hmacHash.GetValueAndReset();
            }

            // Hash transformed k (with the master seed) to get final cipher k
            hash.Append(transformedKey);
            IBuffer cipherKey = hash.GetValueAndReset();
            Dbg.Trace("Got final cipher k from transformed k.");

            // Decrypt the document starting from the end of the header
            ulong headerLength = HeaderData.FullHeader.Length;
            if (this.parserVersion >= KdbxVersion.Four)
            {
                // KDBX4 has a hash at the end of the header
                headerLength += 32;
            }

            stream.Seek(headerLength);
            if (expectedMac != null)
            {
                using (DataReader macReader = GetReaderForStream(stream))
                {
                    await macReader.LoadAsync(expectedMac.Length);
                    IBuffer actualMac = macReader.ReadBuffer(expectedMac.Length);

                    for (uint i = 0; i < expectedMac.Length; i++)
                    {
                        if (expectedMac.GetByte(i) != actualMac.GetByte(i))
                        {
                            Dbg.Trace("HMAC comparison failed, return an error.");
                            return new KdbxDecryptionResult(new ReaderResult(KdbxParserCode.CouldNotDecrypt));
                        }
                    }

                    macReader.DetachStream();
                }
            }

            IBuffer cipherText;
            try
            {
                cipherText = await GetCipherText(stream, hmacHandler);
            }
            catch (FormatException ex)
            {
                Dbg.Trace("Encountered an issue reading ciphertext, returning an error.");
                return new KdbxDecryptionResult(new ReaderResult(KdbxParserCode.DataIntegrityProblem, ex));
            }

            IBuffer decryptedFile = DecryptDatabaseData(cipherText, cipherKey);
            if (decryptedFile == null)
            {
                Dbg.Trace("Decryption failed, returning an error.");
                return new KdbxDecryptionResult(new ReaderResult(KdbxParserCode.CouldNotDecrypt));
            }
            Dbg.Trace("Decrypted.");

            // Verify first 32 bytes of the clear data; if StreamStartBytes wasn't set
            // (e.g. due to KDBX4), nothing happens here.
            for (uint i = 0; i < (HeaderData.StreamStartBytes?.Length ?? 0); i++)
            {
                byte actualByte = decryptedFile.GetByte(i);
                byte expectedByte = HeaderData.StreamStartBytes.GetByte(i);

                if (actualByte != expectedByte)
                {
                    Dbg.Trace("Expected stream start bytes did not match actual stream start bytes.");
                    return new KdbxDecryptionResult(new ReaderResult(KdbxParserCode.FirstBytesMismatch));
                }
            }

            Dbg.Trace("Verified that file decrypted properly.");
            XDocument finalTree = await UnhashAndInflate(decryptedFile);
            if (finalTree == null)
            {
                return new KdbxDecryptionResult(new ReaderResult(KdbxParserCode.MalformedXml));
            }

            try
            {
                KdbxDocument parsedDocument = await Task.Run(() => new KdbxDocument(finalTree.Root, this.HeaderData.GenerateRng()));

                // Validate the final parsed header hash before returning
                if (!String.IsNullOrEmpty(parsedDocument.Metadata.HeaderHash) && parsedDocument.Metadata.HeaderHash != this.HeaderData.HeaderHash)
                {
                    return new KdbxDecryptionResult(new ReaderResult(KdbxParserCode.BadHeaderHash));
                }

                return new KdbxDecryptionResult(parsedDocument, this.rawKey);
            }
            catch (KdbxParseException e)
            {
                return new KdbxDecryptionResult(e.Error);
            }
        }

        /// <summary>
        /// Attempts to decrypt a document using the provided information.
        /// </summary>
        /// <param name="stream">A stream representing the entire file (including header)</param>
        /// <param name="password">The password to the document (may be empty but not null)</param>
        /// <param name="keyfile">The keyfile for the document (may be null)</param>
        /// <param name="token">A token allowing the task to be cancelled.</param>
        /// <returns>A task representing the result of the decryption.</returns>
        public async Task<KdbxDecryptionResult> DecryptFile(
            IRandomAccessStream stream,
            string password,
            ITestableFile keyfile,
            CancellationToken token
        )
        {
            Dbg.Assert(password != null);
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            IList<ISecurityToken> tokenList = new List<ISecurityToken>();
            if (!string.IsNullOrEmpty(password))
            {
                tokenList.Add(new MasterPassword(password));
            }

            if (keyfile != null)
            {
                tokenList.Add(new KeyFile(keyfile));
            }

            IBuffer raw32 = await KeyHelper.GetRawKey(tokenList);
            return await DecryptFile(stream, raw32, token);
        }

        /// <summary>
        /// Attempts to load KDBX header data from the provided data stream.
        /// </summary>
        /// <remarks>
        /// On success, the HeaderData property of this KdbxReader will be populated.
        /// On failure, it will be null.
        /// </remarks>
        /// <param name="stream">A stream representing a KDBX document (or header).</param>
        /// <param name="token">A token allowing the operation to be cancelled.</param>
        /// <returns>A Task representing the result of the read operation.</returns>
        public async Task<ReaderResult> ReadHeader(IRandomAccessStream stream, CancellationToken token)
        {
            this.HeaderData = null;

            using (DataReader reader = GetReaderForStream(stream))
            {
                ReaderResult result = await ValidateSignature(reader);
                if (result != ReaderResult.Success)
                {
                    reader.DetachStream();
                    return result;
                }

                result = await ValidateVersion(reader);
                if (result != ReaderResult.Success)
                {
                    reader.DetachStream();
                    return result;
                }

                // If we get this far, we've passed basic sanity checks.
                // Construct a HeaderData entity and start reading fields.
                KdbxHeaderData headerData = new KdbxHeaderData();

                bool gotEndOfHeader = false;
                while (!gotEndOfHeader)
                {
                    if (token.IsCancellationRequested)
                    {
                        return new ReaderResult(KdbxParserCode.OperationCancelled);
                    }

                    try
                    {
                        KdbxHeaderField field = await ReadHeaderField(reader, headerData);
                        if (field == KdbxHeaderField.EndOfHeader)
                        {
                            gotEndOfHeader = true;
                        }

                        this.headerInitializationMap[field] = true;
                    }
                    catch (KdbxParseException e)
                    {
                        reader.DetachStream();
                        return e.Error;
                    }
                }

                // Ensure all headers are initialized
                bool gotAllHeaders = this.headerInitializationMap.All(
                    kvp => this.headerInitializationMap[kvp.Key]
                );

                if (!gotAllHeaders)
                {
                    reader.DetachStream();
                    return new ReaderResult(KdbxParserCode.HeaderMissing);
                }

                // Hash entire header
                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                var hash = sha256.CreateHash();
                ulong streamPos = stream.Position;
                stream.Seek(0);
                await reader.LoadAsync((uint)streamPos);
                headerData.FullHeader = reader.ReadBuffer((uint)streamPos);

                hash.Append(headerData.FullHeader);
                IBuffer plainHeaderHash = hash.GetValueAndReset();

                if (this.parserVersion == KdbxVersion.Three)
                {
                    // The header was parsed successfully - finish creating the object and return success
                    headerData.HeaderHash = CryptographicBuffer.EncodeToBase64String(plainHeaderHash);
                    headerData.Size = streamPos;
                }
                else if (this.parserVersion >= KdbxVersion.Four)
                {
                    // In KDBX4, the header hash is written directly after the header fields.
                    // After the unencrypted hash, an HMAC-SHA-256 hash of the header is written.
                    await reader.LoadAsync(32);
                    IBuffer existingPlainHash = reader.ReadBuffer(32);

                    // Validate plaintext hash
                    if (plainHeaderHash.Length != existingPlainHash.Length)
                    {
                        return new ReaderResult(KdbxParserCode.BadHeaderHash);
                    }

                    for (uint i = 0; i < plainHeaderHash.Length; i++)
                    {
                        if (plainHeaderHash.GetByte(i) != existingPlainHash.GetByte(i))
                        {
                            return new ReaderResult(KdbxParserCode.BadHeaderHash);
                        }
                    }

                    Dbg.Trace("Validated plaintext KDBX4 header hash");
                }

                HeaderData = headerData;
                reader.DetachStream();
                return ReaderResult.Success;
            }
        }

        /// <summary>
        /// Sets up a DataReader with KeePass-appropriate options for the provided data stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>A properly configured DataReader attached to the stream.</returns>
        private static DataReader GetReaderForStream(IInputStream stream)
        {
            return new DataReader(stream)
            {
                UnicodeEncoding = UnicodeEncoding.Utf8,
                ByteOrder = ByteOrder.LittleEndian,
                InputStreamOptions = InputStreamOptions.Partial
            };
        }

        /// <summary>
        /// Given a stream past the header, asynchronously fetches encrypted ciphertext
        /// from the file as a single buffer.
        /// </summary>
        /// <remarks>
        /// For KDBX3.1, this is just the rest of the stream. For KDBX4, it involves
        /// pulling out HMAC blocks from the stream.
        /// </remarks>
        /// <param name="dataStream">The stream representing the remainder of the database file.</param>
        /// <returns>A buffer representing the encrypted database.</returns>
        private async Task<IBuffer> GetCipherText(IRandomAccessStream dataStream, HmacBlockHandler macHandler)
        {
            uint streamRemaining = (uint)(dataStream.Size - dataStream.Position);
            Dbg.Trace("Stream has {0} bytes remaining.", streamRemaining);

            IBuffer fileRemainder;
            using (DataReader reader = GetReaderForStream(dataStream))
            {
                if (this.parserVersion < KdbxVersion.Four)
                {
                    // KDBX 3.1: Rest of file as-is
                    Dbg.Assert(reader.UnconsumedBufferLength == 0);
                    await reader.LoadAsync(streamRemaining).AsTask().ConfigureAwait(false);

                    fileRemainder = reader.ReadBuffer(streamRemaining);
                }
                else
                {
                    // KDBX 4: HMAC block content
                    int bytesLeft = (int)streamRemaining;
                    Dbg.Assert(bytesLeft > 0);

                    fileRemainder = WindowsRuntimeBuffer.Create(bytesLeft);

                    for (ulong index = 0; bytesLeft > 0; index++)
                    {
                        IBuffer block = await macHandler.ReadCipherBlockAsync(reader, index);
                        if (block == null || block.Length == 0)
                        {
                            break;
                        }

                        Dbg.Assert((int)block.Length > 0);
                        bytesLeft -= (int)block.Length;

                        block.CopyTo(0, fileRemainder, fileRemainder.Length, block.Length);
                        fileRemainder.Length += block.Length;
                    }
                }

                reader.DetachStream();
                return fileRemainder;
            }
        }

        /// <summary>
        /// Attempts to decrypt the meat of a document. 
        /// </summary>
        /// <param name="cipherText">A buffer containing the decryptable data (header excluded).</param>
        /// <param name="decryptionKeyBuffer">An IBuffer containing data representing the AES key.</param>
        /// <returns>A Task representing an IBuffer containing the clear data, or null if decryption fails.</returns>
        private IBuffer DecryptDatabaseData(IBuffer cipherText, IBuffer decryptionKeyBuffer)
        {
            if (HeaderData == null)
            {
                throw new InvalidOperationException("Cannot transform a key without valid header data");
            }

            var aes = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            CryptographicKey key = aes.CreateSymmetricKey(decryptionKeyBuffer);
            Dbg.Trace("Created SymmetricKey.");

            try
            {
                return CryptographicEngine.Decrypt(key, cipherText, HeaderData.EncryptionIV);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Removes the HashedBlock semantics from a decrypted file, along with any compression.
        /// </summary>
        /// <param name="decryptedFile">The cleartext (but hashedData and potentially compressed) document.</param>
        /// <returns>An XDocument representing the document, or null if an error occurs.</returns>
        private async Task<XDocument> UnhashAndInflate(IBuffer decryptedFile)
        {
            if (this.HeaderData == null)
            {
                throw new InvalidOperationException("Cannot transform a key without valid header data");
            }

            // Read as hashedData blocks and decompress
            using (HashedBlockParser parser = new HashedBlockParser(decryptedFile))
            {
                IBuffer workingBuffer;
                try
                {
                    workingBuffer = parser.Parse();
                }
                catch(FormatException)
                {
                    return null;
                }

                if (this.HeaderData.Compression == CompressionAlgorithm.GZip)
                {
                    using (GZipStream gzipStream = new GZipStream(workingBuffer.AsStream(), CompressionMode.Decompress))
                    {
                        byte[] buffer = new byte[1024];
                        int read = await gzipStream.ReadAsync(buffer, 0, buffer.Length);
                        List<byte> bytes = new List<byte>();
                        while (read > 0)
                        {
                            bytes.AddRange(buffer.Take(read));
                            read = await gzipStream.ReadAsync(buffer, 0, buffer.Length);
                        }

                        workingBuffer = CryptographicBuffer.CreateFromByteArray(bytes.ToArray());
                    }
                }

                try
                {
                    return XDocument.Load(workingBuffer.AsStream());
                }
                catch (XmlException)
                {
                    return null;
                }
            }
        }

        #region Header parsing

        /// <summary>
        /// Validates that the document has a valid, supported KeePass signature.
        /// </summary>
        /// <param name="reader">A DataReader over the document file.</param>
        /// <returns>A Task representing the result of the validation.</returns>
        private async Task<ReaderResult> ValidateSignature(DataReader reader)
        {
            await reader.LoadAsync(8);

            UInt32 sig1 = reader.ReadUInt32();
            UInt32 sig2 = reader.ReadUInt32();

            if (sig1 == KP1_SIG1 && sig2 == KP1_SIG2)
            {
                return new ReaderResult(KdbxParserCode.SignatureKP1);
            }

            if (sig1 == KP2_PR_SIG1 && sig2 == KP2_PR_SIG2)
            {
                return new ReaderResult(KdbxParserCode.SignatureKP2PR);
            }

            if (!(sig1 == SIG1 && sig2 == SIG2))
            {
                return new ReaderResult(KdbxParserCode.SignatureInvalid);
            }

            return ReaderResult.Success;
        }

        /// <summary>
        /// Validates that the document has a valid, supported KeePass version.
        /// </summary>
        /// <param name="reader">A DataReader over the document file.</param>
        /// <returns>A Task representing the result of the validation.</returns>
        private async Task<ReaderResult> ValidateVersion(DataReader reader)
        {
            await reader.LoadAsync(4);

            uint version = reader.ReadUInt32();
            uint maskedVersion = version & FileVersionMask;
            uint maskedLegacyFormat = FileVersion32_3 & FileVersionMask;
            uint maskedModernFormat = FileVersion32_4 & FileVersionMask;
            if (maskedVersion == maskedLegacyFormat)
            {
                this.parserVersion = KdbxVersion.Three;
            }
            else if (maskedVersion == maskedModernFormat)
            {
                this.parserVersion = KdbxVersion.Four;
            }
            else
            {
                Dbg.Assert(maskedVersion > maskedModernFormat);
                return new ReaderResult(KdbxParserCode.Version);
            }

            // Based on the version, initialize a map of required headers
            foreach (KdbxHeaderField value in Enum.GetValues(typeof(KdbxHeaderField))
                .Cast<KdbxHeaderField>())
            {
                MemberInfo enumMember = typeof(KdbxHeaderField).GetMember(value.ToString())
                    .FirstOrDefault();

                // Skip optional headers regardless of version
                if (enumMember.GetCustomAttribute<OptionalAttribute>() != null)
                {
                    continue;
                }

                // Get the headers that support this version
                var versionAttr = enumMember.GetCustomAttribute<KdbxVersionSupportAttribute>();
                if (versionAttr == null || versionAttr.Supports(this.parserVersion))
                {
                    this.headerInitializationMap[value] = false;
                }
            }

            return ReaderResult.Success;
        }

        /// <summary>
        /// Attempts to read and validate the next header field in the data stream.
        /// </summary>
        /// <param name="reader">A reader of the document file.</param>
        /// <param name="headerData">The header data that has been extracted so far.</param>
        /// <returns>A Task representing the field that was read.</returns>
        private async Task<KdbxHeaderField> ReadHeaderField(DataReader reader, KdbxHeaderData headerData)
        {
            // A header is guaranteed to have 3 (or 5) bytes at the beginning.
            // The first byte represents the type or ID of the header.
            // The next two (or four) bytes represent a 16-bit unsigned integer giving the size of the data field.
            uint sizeBytes = (this.parserVersion < KdbxVersion.Four ? 2U : 4U);

            await reader.LoadAsync(1U + sizeBytes);

            // Read the header ID from the first byte
            var fieldId = (KdbxHeaderField)reader.ReadByte();

            // Read the header data field size from the next two bytes
            uint size;
            if (sizeBytes == 2)
            {
                size = reader.ReadUInt16();
            }
            else
            {
                Dbg.Assert(sizeBytes == 4);
                size = reader.ReadUInt32();
            }

            Dbg.Trace("FieldID: {0}, Size: {1}", fieldId.ToString(), size);
            await reader.LoadAsync(size);

            // The cast above may have succeeded but still resulted in an unknown value (outside of the enum).
            // If so, we need to bail.
            if (!Enum.IsDefined(typeof(KdbxHeaderField), fieldId))
            {
                throw new KdbxParseException(ReaderResult.FromHeaderFieldUnknown((byte)fieldId));
            }

            MemberInfo memberInfo = typeof(KdbxHeaderField).GetMember(fieldId.ToString())
                .FirstOrDefault();
            Dbg.Assert(memberInfo != null);
            KdbxVersionSupportAttribute versionAttr = memberInfo.GetCustomAttribute<KdbxVersionSupportAttribute>();
            if (versionAttr != null)
            {
                Dbg.Trace($"Found version attribute for header: {versionAttr}");
                Dbg.Assert(versionAttr.Supports(this.parserVersion));
            }

            byte[] data = new byte[size];
            reader.ReadBytes(data);

            // Based on the header field in question, the data is validated differently.
            // The size of the data field needs to be validated, and the data itself may need to be parsed.
            switch (fieldId)
            {
                case KdbxHeaderField.EndOfHeader:
                    break;

                case KdbxHeaderField.CipherID:
                    RequireFieldDataSizeEq(fieldId, 16, size);

                    Guid cipherGuid = new Guid(data);
                    if (cipherGuid.Equals(AesCipher.Uuid))
                    {
                        headerData.Cipher = EncryptionAlgorithm.Aes;
                    }
                    else if (cipherGuid.Equals(ChaCha20Cipher.Uuid))
                    {
                        Dbg.Assert(this.parserVersion == KdbxVersion.Four);
                        headerData.Cipher = EncryptionAlgorithm.ChaCha20;
                        this.expectedIvBytes = 12;
                    }
                    else
                    {
                        // If we get here, the cipher provided is not supported.
                        throw new KdbxParseException(ReaderResult.FromHeaderDataUnknown(fieldId, cipherGuid.ToString()));
                    }

                    break;

                case KdbxHeaderField.CompressionFlags:
                    RequireFieldDataSizeEq(fieldId, 4, size);
                    headerData.Compression = (CompressionAlgorithm)BitConverter.ToUInt32(data, 0);
                    RequireEnumDefined(fieldId, headerData.Compression);
                    break;

                case KdbxHeaderField.MasterSeed:
                    RequireFieldDataSizeEq(fieldId, 32, size);
                    headerData.MasterSeed = CryptographicBuffer.CreateFromByteArray(data);
                    break;

                case KdbxHeaderField.TransformSeed:
                    RequireFieldDataSizeEq(fieldId, 32, size);
                    headerData.TransformSeed = CryptographicBuffer.CreateFromByteArray(data);
                    break;

                case KdbxHeaderField.TransformRounds:
                    RequireFieldDataSizeEq(fieldId, 8, size);
                    headerData.TransformRounds = BitConverter.ToUInt64(data, 0);
                    break;

                case KdbxHeaderField.EncryptionIV:
                    RequireFieldDataSizeEq(fieldId, expectedIvBytes, size);
                    headerData.EncryptionIV = CryptographicBuffer.CreateFromByteArray(data);
                    break;

                case KdbxHeaderField.ProtectedStreamKey:
                    RequireFieldDataSize(fieldId, size, (n) => n > 0, "must be nonzero");
                    headerData.ProtectedStreamKey = data;
                    break;

                case KdbxHeaderField.StreamStartBytes:
                    headerData.StreamStartBytes = CryptographicBuffer.CreateFromByteArray(data);
                    break;

                case KdbxHeaderField.InnerRandomStreamID:
                    RequireFieldDataSizeEq(fieldId, 4, size);
                    headerData.InnerRandomStream = (RngAlgorithm)BitConverter.ToUInt32(data, 0);
                    RequireEnumDefined(fieldId, headerData.InnerRandomStream);
                    break;

                case KdbxHeaderField.KdfParameters:
                    try
                    {
                        using (IInputStream memStream = new MemoryStream(data).AsInputStream())
                        {
                            using (DataReader vdReader = GetReaderForStream(memStream))
                            {
                                VariantDictionary kdfParamDict = await VariantDictionary.ReadDictionaryAsync(vdReader);
                                KdfParameters kdfParams = new KdfParameters(kdfParamDict);

                                if (kdfParams.Uuid == AesParameters.AesUuid)
                                {
                                    this.kdfParams = new AesParameters(kdfParamDict);
                                }
                                else if (kdfParams.Uuid == Argon2Parameters.Argon2Uuid)
                                {
                                    this.kdfParams = new Argon2Parameters(kdfParamDict);
                                }
                                else
                                {
                                    throw new FormatException($"Unknown KDF UUID: {kdfParams.Uuid}");
                                }
                            }
                        }
                    }
                    catch (FormatException ex)
                    {
                        throw new KdbxParseException(ReaderResult.FromBadVariantDictionary(ex.Message));
                    }
                    break;

                case KdbxHeaderField.PublicCustomData:
                    try
                    {
                        VariantDictionary customParams = await VariantDictionary.ReadDictionaryAsync(reader);
                    }
                    catch (FormatException ex)
                    {
                        throw new KdbxParseException(ReaderResult.FromBadVariantDictionary(ex.Message));
                    }
                    break;
            }

            return fieldId;
        }

        #endregion

        #region Header field validation helpers

        /// <summary>
        /// Validates that the specified header field has a size matching a provided requirement. Throws on failure.
        /// </summary>
        /// <param name="field">The header field to validate.</param>
        /// <param name="size">The size of the data field.</param>
        /// <param name="requirement">An evaluator function that returns whether the size is valid.</param>
        /// <param name="explanation">A String explanation of the requirement.</param>
        private void RequireFieldDataSize(KdbxHeaderField field, uint size, Predicate<uint> requirement, string explanation)
        {
            bool result = requirement(size);
            if (result)
            {
                return;
            }

            throw new KdbxParseException(ReaderResult.FromHeaderDataSize(field, size, explanation));
        }

        /// <summary>
        /// Validates that the specified header field has a size matching a provided value. Throws on failure.
        /// </summary>
        /// <param name="field">The header field to validate.</param>
        /// <param name="expectedSize">The expected size of the data field.</param>
        /// <param name="actualSize">The actual size of the data field.</param>
        private void RequireFieldDataSizeEq(KdbxHeaderField field, uint expectedSize, uint actualSize)
        {
            RequireFieldDataSize(field, expectedSize, (size) => (size == expectedSize), String.Format("expected: {0}", expectedSize));
        }

        /// <summary>
        /// Validates that the specified enum value is defined in the provided enum.
        /// </summary>
        /// <typeparam name="T">An enum type to validate against.</typeparam>
        /// <param name="field">The header field to validated.</param>
        /// <param name="value">The enum value to validate.</param>
        private void RequireEnumDefined<T>(KdbxHeaderField field, T value)
            where T : struct
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new KdbxParseException(ReaderResult.FromHeaderDataUnknown(field, value.ToString()));
            }
        }

        #endregion
    }
}
