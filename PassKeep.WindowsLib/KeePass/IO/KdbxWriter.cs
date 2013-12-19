using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.KeePass.Dom;
using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.KeePass.SecurityTokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public string Password
        {
            get;
            set;
        }
        public KeyFile KeyFile
        {
            get;
            set;
        }


        public KdbxWriter(string password, KeyFile keyfile, ulong transformRounds = 6000)
            : this(password, keyfile, RngAlgorithm.Salsa20, CompressionAlgorithm.GZip, transformRounds)
        { }

        public KdbxWriter(
                string password,
                KeyFile keyfile,
                RngAlgorithm rngAlgorithm,
                CompressionAlgorithm compression,
                ulong transformRounds
            )
            : base()
        {
            Password = password;
            KeyFile = keyfile;
            _compression = compression;
            _masterSeed = CryptographicBuffer.GenerateRandom(32);
            _transformSeed = CryptographicBuffer.GenerateRandom(32);
            _transformRounds = transformRounds;
            _encryptionIV = CryptographicBuffer.GenerateRandom(16);
            _streamStartBytes = CryptographicBuffer.GenerateRandom(32);
            _protectedStreamKey = CryptographicBuffer.GenerateRandom(32).ToArray();

            switch (rngAlgorithm)
            {
                case RngAlgorithm.ArcFourVariant:
                    _masterRng = new ArcFourVariant(_protectedStreamKey);
                    break;
                case RngAlgorithm.Salsa20:
                    _masterRng = new Salsa20(_protectedStreamKey);
                    break;
                default:
                    Debug.Assert(false);
                    throw new ArgumentException();
            }
        }

        public async Task<bool> Write(StorageFile file, KdbxDocument document)
        {
            StorageFile outputFile = await
                ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                    "output.kdbx",
                    CreationCollisionOption.ReplaceExisting
                );

            using (IRandomAccessStream fileStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
                {
                    bool result = await Write(outputStream, document);
                    if (!result)
                    {
                        return false;
                    }
                }
            }

            await outputFile.CopyAndReplaceAsync(file);
            return true;
        }

        public async Task<bool> Write(IOutputStream stream, KdbxDocument document)
        {
            Debug.Assert(stream != null);
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (DataWriter writer = new DataWriter(stream))
            {
                writer.UnicodeEncoding = UnicodeEncoding.Utf8;
                writer.ByteOrder = ByteOrder.LittleEndian;

                using (InMemoryRandomAccessStream headerStream = new InMemoryRandomAccessStream())
                {
                    using (DataWriter headerWriter = new DataWriter(headerStream) { UnicodeEncoding = UnicodeEncoding.Utf8, ByteOrder = ByteOrder.LittleEndian })
                    {
                        writeSignature(headerWriter);
                        writeVersion(headerWriter);
                        await headerWriter.StoreAsync();

                        await writeHeader(headerWriter);
                        await headerWriter.StoreAsync();

                        headerWriter.DetachStream();
                    }

                    headerStream.Seek(0);
                    using (DataReader headerReader = new DataReader(headerStream) { UnicodeEncoding = UnicodeEncoding.Utf8, ByteOrder = ByteOrder.LittleEndian })
                    {
                        await headerReader.LoadAsync((uint)headerStream.Size);
                        IBuffer headerBuffer = headerReader.ReadBuffer((uint)headerStream.Size);

                        var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                        var hash = sha256.CreateHash();
                        hash.Append(headerBuffer);

                        IBuffer hashedHeaderBuffer = hash.GetValueAndReset();
                        string headerHash = CryptographicBuffer.EncodeToBase64String(hashedHeaderBuffer);
                        document.Metadata.HeaderHash = headerHash;

                        XDocument xmlDocument = new XDocument(document.ToXml(_masterRng.Clone()));
                        cts = new CancellationTokenSource();
                        IBuffer body = await getBody(xmlDocument, cts.Token);
                        try
                        {
                            if (body == null)
                            {
                                throw new OperationCanceledException();
                            }

                            cts.Token.ThrowIfCancellationRequested();
                            cts = null;
                        }
                        catch (OperationCanceledException)
                        {
                            return false;
                        }

                        writer.WriteBuffer(headerBuffer);
                        await writer.StoreAsync();
                        writer.WriteBuffer(body);
                        await writer.StoreAsync();
                    }
                }

                writer.DetachStream();
                return true;
            }
        }

        private async Task<IBuffer> getBody(XDocument xmlDocument, CancellationToken token)
        {
            using (var mStream = new MemoryStream())
            {
                using (var gzStream = new GZipStream(mStream, CompressionMode.Compress))
                {
                    Stream writeStream;
                    switch (_compression)
                    {
                        case CompressionAlgorithm.GZip:
                            writeStream = gzStream;
                            break;
                        default:
                            writeStream = mStream;
                            break;
                    }

                    xmlDocument.Save(writeStream);
                }
                IBuffer zipped = mStream.ToArray().AsBuffer();
                IBuffer hashed = await HashedBlockWriter.Create(zipped);

                IBuffer clearFile = (new byte[_streamStartBytes.Length + hashed.Length]).AsBuffer();
                _streamStartBytes.CopyTo(0, clearFile, 0, _streamStartBytes.Length);
                hashed.CopyTo(0, clearFile, _streamStartBytes.Length, hashed.Length);

                var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                var hash = sha256.CreateHash();
                hash.Append(_masterSeed);

                var securityTokens = new List<ISecurityToken>();
                if (!string.IsNullOrEmpty(Password))
                {
                    securityTokens.Add(new MasterPassword(Password));
                }
                if (KeyFile != null)
                {
                    securityTokens.Add(KeyFile);
                }
                IBuffer raw32 = await KeyHelper.GetRawKey(securityTokens);
                IBuffer transformed = await KeyHelper.TransformKey(raw32, _transformSeed, _transformRounds, token);
                if (transformed == null)
                {
                    Debug.WriteLine("Encryption was cancelled. Returning.");
                    return null;
                }

                Debug.WriteLine("Got raw k.");

                // Hash transformed k (with the master seed) to get final AES k
                hash.Append(transformed);
                IBuffer aesKeyBuffer = hash.GetValueAndReset();
                Debug.WriteLine("Got final AES k from transformed k.");

                // Do the decryption on the rest of the viewModel
                var aes = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
                var key = aes.CreateSymmetricKey(aesKeyBuffer);
                Debug.WriteLine("Created SymmetricKey.");

                IBuffer encrypted = CryptographicEngine.Encrypt(key, clearFile, _encryptionIV);
                byte[] encBytes = encrypted.ToArray();

                return encrypted;
            }
        }

        private void writeFieldId(DataWriter writer, KdbxHeaderField field)
        {
            writer.WriteByte((byte)field);
        }

        private void writeFieldSize(DataWriter writer, ushort size)
        {
            writer.WriteUInt16(size);
        }

        private void writeFieldSize(DataWriter writer, uint size)
        {
            writeFieldSize(writer, (ushort)size);
        }

        private void writeFieldSize(DataWriter writer, int size)
        {
            writeFieldSize(writer, (ushort)size);
        }

        private async Task writeHeader(DataWriter writer)
        {
            // We assume AES because that's all the reader supports.
            writeFieldId(writer, KdbxHeaderField.CipherID);
            writeFieldSize(writer, 16);
            writer.WriteBytes(AesUuid.Uid.ToByteArray());
            await writer.StoreAsync();

            writeFieldId(writer, KdbxHeaderField.CompressionFlags);
            writeFieldSize(writer, 4);
            writer.WriteUInt32((UInt32)_compression);
            await writer.StoreAsync();

            writeFieldId(writer, KdbxHeaderField.MasterSeed);
            writeFieldSize(writer, 32);
            writer.WriteBuffer(_masterSeed);
            await writer.StoreAsync();

            writeFieldId(writer, KdbxHeaderField.TransformSeed);
            writeFieldSize(writer, 32);
            writer.WriteBuffer(_transformSeed);
            await writer.StoreAsync();

            writeFieldId(writer, KdbxHeaderField.TransformRounds);
            writeFieldSize(writer, 8);
            writer.WriteUInt64(_transformRounds);
            await writer.StoreAsync();

            writeFieldId(writer, KdbxHeaderField.EncryptionIV);
            writeFieldSize(writer, 16);
            writer.WriteBuffer(_encryptionIV);
            await writer.StoreAsync();

            writeFieldId(writer, KdbxHeaderField.ProtectedStreamKey);
            writeFieldSize(writer, 32);
            writer.WriteBytes(_protectedStreamKey);
            await writer.StoreAsync();

            writeFieldId(writer, KdbxHeaderField.StreamStartBytes);
            writeFieldSize(writer, _streamStartBytes.Length);
            writer.WriteBuffer(_streamStartBytes);
            await writer.StoreAsync();

            writeFieldId(writer, KdbxHeaderField.InnerRandomStreamID);
            writeFieldSize(writer, 4);
            writer.WriteUInt32((UInt32)_masterRng.Algorithm);
            await writer.StoreAsync();

            writeFieldId(writer, KdbxHeaderField.EndOfHeader);
            writeFieldSize(writer, 4);
            writer.WriteByte(0x0D);
            writer.WriteByte(0x0A);
            writer.WriteByte(0x0D);
            writer.WriteByte(0x0A);
            await writer.StoreAsync();
        }

        private void writeSignature(DataWriter writer)
        {
            writer.WriteUInt32(SIG1);
            writer.WriteUInt32(SIG2);
        }

        private void writeVersion(DataWriter writer)
        {
            writer.WriteUInt32(FileVersion32);
        }

        private void requireFieldDataSize(KdbxHeaderField field, int size, Predicate<int> requirement, string explanation)
        {
            bool result = requirement(size);
            Debug.Assert(result);
            if (result)
            {
                return;
            }

            throw new KdbxParseException(KeePassError.FromHeaderDataSize(field.ToString(), size, explanation));
        }

        private void requireFieldDataSizeEq(KdbxHeaderField field, int expectedSize, int actualSize)
        {
            requireFieldDataSize(field, expectedSize, (size) => size == expectedSize, string.Format("expected: {0}", expectedSize));
        }

        private void requireEnumDefined<T>(KdbxHeaderField field, T value)
            where T : struct
        {
            Debug.Assert(Enum.IsDefined(typeof(T), value));
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new KdbxParseException(KeePassError.FromHeaderDataUnknown(value.ToString()));
            }
        }
    }
}
