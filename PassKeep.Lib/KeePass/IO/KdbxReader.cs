using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.SecurityTokens;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.IO
{
    public sealed class KdbxReader : KdbxFileHandler, IKdbxReader
    {
        // Cached security tokens, used to generate a compatible KdbxWriter
        private IBuffer rawKey;

        // "headerInitializationMap" is a lookup table used to determine if 
        // a header field has been seen in the file or not.
        // This code initializes every field to false.
        private Dictionary<KdbxHeaderField, bool> headerInitializationMap =
            Enum.GetValues(typeof(KdbxHeaderField)).Cast<KdbxHeaderField>().ToDictionary(
                (field) => field, (field) => false
            );

        public KdbxReader()
        {
            // Comments are not important/required for a successful parse
            headerInitializationMap[KdbxHeaderField.Comment] = true;
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

            // Transform the key (this can take a while)
            IBuffer transformedKey = await KeyHelper.TransformKey(rawKey, this.HeaderData.TransformSeed, this.HeaderData.TransformRounds, token);
            if (transformedKey == null)
            {
                return new KdbxDecryptionResult(new ReaderResult(KdbxParserCode.OperationCancelled));
            }

            this.rawKey = rawKey;
            Dbg.Trace("Got raw k.");

            // Hash transformed k (with the master seed) to get final AES k
            hash.Append(transformedKey);
            IBuffer aesKeyBuffer = hash.GetValueAndReset();
            Dbg.Trace("Got final AES k from transformed k.");

            // Decrypt the document starting from the end of the header
            stream.Seek(this.HeaderData.Size);
            IBuffer decryptedFile = await DecryptDatabaseData(stream, aesKeyBuffer);
            if (decryptedFile == null)
            {
                Dbg.Trace("Decryption failed, returning an error.");
                return new KdbxDecryptionResult(new ReaderResult(KdbxParserCode.CouldNotDecrypt));
            }
            Dbg.Trace("Decrypted.");

            // Verify first 32 bytes of the clear data
            for (uint i = 0; i < this.HeaderData.StreamStartBytes.Length; i++)
            {
                byte actualByte = decryptedFile.GetByte(i);
                byte expectedByte = this.HeaderData.StreamStartBytes.GetByte(i);

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
            StorageFile keyfile,
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

                        headerInitializationMap[field] = true;
                    }
                    catch (KdbxParseException e)
                    {
                        reader.DetachStream();
                        return e.Error;
                    }
                }

                // Ensure all headers are initialized
                bool gotAllHeaders = headerInitializationMap.All(
                        kvp => headerInitializationMap[kvp.Key]
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
                hash.Append(reader.ReadBuffer((uint)streamPos));
                
                // The header was parsed successfully - finish creating the object and return success
                headerData.HeaderHash = CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());
                headerData.Size = streamPos;
                this.HeaderData = headerData;

                reader.DetachStream();
                return ReaderResult.Success;
            }
        }

        /// <summary>
        /// Sets up a DataReader with KeePass-appropriate options for the provided data stream.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <returns>A properly configured DataReader attached to the stream.</returns>
        private static DataReader GetReaderForStream(IRandomAccessStream stream)
        {
            return new DataReader(stream)
            {
                UnicodeEncoding = UnicodeEncoding.Utf8,
                ByteOrder = ByteOrder.LittleEndian,
                InputStreamOptions = InputStreamOptions.Partial
            };
        }

        /// <summary>
        /// Attempts to decrypt the meat of a document. 
        /// </summary>
        /// <param name="dataStream">A stream over the decryptable data (header excluded).</param>
        /// <param name="decryptionKeyBuffer">An IBuffer containing data representing the AES key.</param>
        /// <returns>A Task representing an IBuffer containing the clear data, or null if decryption fails.</returns>
        private async Task<IBuffer> DecryptDatabaseData(IRandomAccessStream dataStream, IBuffer decryptionKeyBuffer)
        {
            if (this.HeaderData == null)
            {
                throw new InvalidOperationException("Cannot transform a key without valid header data");
            }

            var aes = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            CryptographicKey key = aes.CreateSymmetricKey(decryptionKeyBuffer);
            Dbg.Trace("Created SymmetricKey.");

            uint streamRemaining = (uint)(dataStream.Size - dataStream.Position);
            Dbg.Trace("Stream has {0} bytes remaining.", streamRemaining);

            using (DataReader reader = GetReaderForStream(dataStream))
            {
                Dbg.Assert(reader.UnconsumedBufferLength == 0);
                await reader.LoadAsync(streamRemaining);

                IBuffer fileRemainder = reader.ReadBuffer(streamRemaining);
                reader.DetachStream();

                Dbg.Trace("Decrypting...");

                try
                {
                    return CryptographicEngine.Decrypt(key, fileRemainder, this.HeaderData.EncryptionIV);
                }
                catch
                {
                    return null;
                }
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

            UInt32 version = reader.ReadUInt32();
            if ((version & FileVersionMask) > (FileVersion32 & FileVersionMask))
            {
                return new ReaderResult(KdbxParserCode.Version);
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
            // A header is guaranteed to have 3 bytes at the beginning.
            // The first byte represents the type or ID of the header.
            // The next two bytes represent a 16-bit unsigned integer giving the size of the data field.

            await reader.LoadAsync(3);

            // Read the header ID from the first byte
            var fieldId = (KdbxHeaderField)reader.ReadByte();

            // Read the header data field size from the next two bytes
            UInt16 size = reader.ReadUInt16();

            Dbg.Trace("FieldID: {0}, Size: {1}", fieldId.ToString(), size);
            await reader.LoadAsync(size);

            // The cast above may have succeeded but still resulted in an unknown value (outside of the enum).
            // If so, we need to bail.
            if (!Enum.IsDefined(typeof(KdbxHeaderField), fieldId))
            {
                throw new KdbxParseException(ReaderResult.FromHeaderFieldUnknown((byte)fieldId));
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
                    if (cipherGuid.Equals(AesUuid.Uid))
                    {
                        headerData.Cipher = EncryptionAlgorithm.Aes;
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
                    RequireFieldDataSizeEq(fieldId, 16, size);
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
        private void RequireFieldDataSize(KdbxHeaderField field, int size, Predicate<int> requirement, string explanation)
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
        private void RequireFieldDataSizeEq(KdbxHeaderField field, int expectedSize, int actualSize)
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
