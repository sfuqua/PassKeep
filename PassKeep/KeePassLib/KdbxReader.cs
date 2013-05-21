using PassKeep.KeePassLib.SecurityTokens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PassKeep.KeePassLib
{
    public class KdbxReader : KdbxHandler, IDisposable
    {
        private IRandomAccessStream _stream;
        private DataReader _reader;
        public string HeaderHash { get; private set; }
        private CancellationTokenSource decryptionCts;
        private string _password;
        private KeyFile _keyfile;

        // unlockFile a dictionary to "false" for every k in KdbxHeaderField
        private Dictionary<KdbxHeaderField, bool> headerInitializationMap =
            Enum.GetValues(typeof(KdbxHeaderField)).Cast<KdbxHeaderField>().ToDictionary(
                (field) => field, (field) => false
            );

        public KdbxReader(IRandomAccessStream stream)
        {
            _stream = stream;
            _reader = new DataReader(_stream)
            {
                UnicodeEncoding = UnicodeEncoding.Utf8,
                ByteOrder = ByteOrder.LittleEndian,
                InputStreamOptions = InputStreamOptions.Partial
            };

            headerInitializationMap[KdbxHeaderField.Comment] = true;
        }

        public KdbxWriter GetWriter()
        {
            return new KdbxWriter(
                _password,
                _keyfile,
                _masterRng.Algorithm,
                _compression,
                _transformRounds
            ) { Document = this.Document };
        }

        /// <summary>
        /// Algorith is as of this writing (11/5/2012):
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
        /// Failure to match these constants results in a parse Error.
        /// 
        /// 4 bytes: File version
        /// 
        /// Header fields:
        /// 1 byte: Field ID
        /// 2 bytes: Field size (n)
        /// n bytes: Data
        /// 
        /// </summary>
        public async Task<KeePassError> DecryptFile(string password, StorageFile keyfile)
        {
            decryptionCts = new CancellationTokenSource();

            Debug.Assert(password != null);
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            _password = password;

            ulong streamPosition = _stream.Position;

            // Init a SHA256 hash buffer and append the master seed to it
            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var hash = sha256.CreateHash();
            hash.Append(_masterSeed);

            // Get k from user data and salt
            var securityTokens = new List<ISecurityToken>();
            if (!string.IsNullOrEmpty(password))
            {
                securityTokens.Add(new MasterPassword(password));
            }
            if (keyfile != null)
            {
                _keyfile = new KeyFile(keyfile);
                securityTokens.Add(_keyfile);
            }
            else
            {
                _keyfile = null;
            }
            IBuffer raw32 = await KeePassHelper.GetRawKey(securityTokens);
            IBuffer transformed = await KeePassHelper.TransformKey(raw32, _transformSeed, _transformRounds, decryptionCts.Token);
            if (transformed == null)
            {
                Debug.WriteLine("Decryption was cancelled. Resetting.");
                _stream.Seek(streamPosition);
                return new KeePassError(KdbxParseError.OperationCancelled);
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

            uint streamLeft = (uint)(_stream.Size - _stream.Position);
            Debug.WriteLine("Stream has {0} bytes remaining.", streamLeft);
            Debug.Assert(_reader.UnconsumedBufferLength == 0);
            await _reader.LoadAsync(streamLeft);
            IBuffer fileRemainder = _reader.ReadBuffer(streamLeft);
            Debug.WriteLine("Decrypting...");
            IBuffer decryptedFile;
            try
            {
                decryptedFile = CryptographicEngine.Decrypt(key, fileRemainder, _encryptionIV);
            }
            catch (Exception)
            {
                Debug.WriteLine("Decryption failed with exception, returning error.");
                _stream.Seek(streamPosition);
                return new KeePassError(KdbxParseError.CouldNotDecrypt);
            }
            Debug.WriteLine("Decrypted.");

            // Verify first 32 bytes
            for (uint i = 0; i < _streamStartBytes.Length; i++)
            {
                byte actualByte = decryptedFile.GetByte(i);
                byte expectedByte = _streamStartBytes.GetByte(i);

                if (actualByte != expectedByte)
                {
                    _stream.Seek(streamPosition);
                    return new KeePassError(KdbxParseError.CouldNotDecrypt);
                }
            }

            Debug.WriteLine("File decrypted properly.");

            // Read as hashed blocks and decompress

            using (var parser = new HashedBlockParser(decryptedFile))
            {
                IBuffer workingBuffer = parser.Parse();
                if (_compression == CompressionAlgorithm.GZip)
                {
                    using (var gzipStream = new GZipStream(workingBuffer.AsStream(), CompressionMode.Decompress))
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
                    Document = XDocument.Load(workingBuffer.AsStream());
                }
                catch (XmlException)
                {
                    _stream.Seek(streamPosition);
                    throw new FormatException("Unable to parse decrypted XML.");
                }

                Debug.WriteLine("Got KDBX tree.");
            }

            _stream.Seek(streamPosition);
            return KeePassError.None;
        }

        public void CancelDecrypt()
        {
            decryptionCts.Cancel();
        }

        public async Task<KeePassError> ReadHeader()
        {
            _stream.Seek(0);

            KeePassError result = await validateSignature();
            if (result != KeePassError.None)
            {
                return result;
            }

            result = await validateVersion();
            if (result != KeePassError.None)
            {
                return result;
            }

            bool gotEndOfHeader = false; ;
            while (!gotEndOfHeader)
            {
                try
                {
                    KdbxHeaderField field = await readHeaderField();
                    if (field == KdbxHeaderField.EndOfHeader)
                    {
                        gotEndOfHeader = true;
                    }

                    Debug.Assert(!headerInitializationMap[field]);
                    headerInitializationMap[field] = true;
                }
                catch (KdbxParseException e)
                {
                    return e.Error;
                }
            }

            // Ensure all headers are initialized
            bool gotAllHeaders = headerInitializationMap.All(
                    kvp => headerInitializationMap[kvp.Key]
                );
            Debug.Assert(gotAllHeaders);
            if (!gotAllHeaders)
            {
                return new KeePassError(KdbxParseError.HeaderMissing);
            }

            // Hash entire header
            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var hash = sha256.CreateHash();
            ulong streamPos = _stream.Position;
            _stream.Seek(0);
            await _reader.LoadAsync((uint)streamPos);
            hash.Append(_reader.ReadBuffer((uint)streamPos));
            HeaderHash = CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());

            return KeePassError.None;
        }

        private async Task<KeePassError> validateSignature()
        {
            await _reader.LoadAsync(8);

            UInt32 sig1 = _reader.ReadUInt32();
            UInt32 sig2 = _reader.ReadUInt32();

            if (sig1 == KP1_SIG1 && sig2 == KP1_SIG2)
            {
                return new KeePassError(KdbxParseError.SignatureKP1);
            }

            if (sig1 == KP2_PR_SIG1 && sig2 == KP2_PR_SIG2)
            {
                return new KeePassError(KdbxParseError.SignatureKP2PR);
            }

            if (!(sig1 == SIG1 && sig2 == SIG2))
            {
                return new KeePassError(KdbxParseError.SignatureInvalid);
            }

            return KeePassError.None;
        }

        private async Task<KeePassError> validateVersion()
        {
            await _reader.LoadAsync(4);

            UInt32 version = _reader.ReadUInt32();
            if ((version & FileVersionMask) > (FileVersion32 & FileVersionMask))
            {
                return new KeePassError(KdbxParseError.Version);
            }

            return KeePassError.None;
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

        private async Task<KdbxHeaderField> readHeaderField()
        {
            await _reader.LoadAsync(3);

            var fieldId = (KdbxHeaderField)_reader.ReadByte();
            UInt16 size = _reader.ReadUInt16();
            Debug.WriteLine("FieldID: {0}, Size: {1}", fieldId.ToString(), size);
            await _reader.LoadAsync(size);

            if (!Enum.IsDefined(typeof(KdbxHeaderField), fieldId))
            {
                throw new KdbxParseException(KeePassError.FromHeaderFieldUnknown((byte)fieldId));
            }

            byte[] data = new byte[size];
            _reader.ReadBytes(data);

            switch (fieldId)
            {
                case KdbxHeaderField.EndOfHeader:
                    break;
                case KdbxHeaderField.CipherID:
                    requireFieldDataSizeEq(fieldId, 16, size);

                    Guid cipherGuid = new Guid(data);
                    Debug.Assert(cipherGuid.Equals(AesUuid.Uid));
                    if (!cipherGuid.Equals(AesUuid.Uid))
                    {
                        throw new KdbxParseException(KeePassError.FromHeaderDataUnknown(fieldId.ToString()));
                    }
                    break;

                case KdbxHeaderField.CompressionFlags:
                    requireFieldDataSizeEq(fieldId, 4, size);
                    _compression = (CompressionAlgorithm) BitConverter.ToUInt32(data, 0);
                    requireEnumDefined(fieldId, _compression);
                    break;

                case KdbxHeaderField.MasterSeed:
                    requireFieldDataSizeEq(fieldId, 32, size);
                    _masterSeed = CryptographicBuffer.CreateFromByteArray(data);
                    break;

                case KdbxHeaderField.TransformSeed:
                    requireFieldDataSizeEq(fieldId, 32, size);
                    _transformSeed = CryptographicBuffer.CreateFromByteArray(data);
                    break;

                case KdbxHeaderField.TransformRounds:
                    requireFieldDataSizeEq(fieldId, 8, size);
                    _transformRounds = BitConverter.ToUInt64(data, 0);
                    break;

                case KdbxHeaderField.EncryptionIV:
                    requireFieldDataSizeEq(fieldId, 16, size);
                    _encryptionIV = CryptographicBuffer.CreateFromByteArray(data);
                    break;

                case KdbxHeaderField.ProtectedStreamKey:
                    requireFieldDataSize(fieldId, size, (n) => n > 0, "must be nonzero");
                    _protectedStreamKey = data;
                    break;

                case KdbxHeaderField.StreamStartBytes:
                    _streamStartBytes = CryptographicBuffer.CreateFromByteArray(data);
                    break;

                case KdbxHeaderField.InnerRandomStreamID:
                    requireFieldDataSizeEq(fieldId, 4, size);
                    RngAlgorithm alg = (RngAlgorithm) BitConverter.ToUInt32(data, 0);
                    requireEnumDefined(fieldId, alg);
                    switch (alg)
                    {
                        case RngAlgorithm.ArcFourVariant:
                            _masterRng = new ArcFourRng(_protectedStreamKey);
                            break;
                        case RngAlgorithm.Salsa20:
                            _masterRng = new Salsa20Rng(_protectedStreamKey);
                            break;
                        default:
                            Debug.Assert(false);
                            throw new KdbxParseException(
                                KeePassError.FromHeaderDataUnknown(fieldId.ToString())
                            );
                    }
                    break;
            }

            return fieldId;
        }

        #region IDisposable

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_reader != null)
                    {
                        _reader.DetachStream();
                        _reader.Dispose();
                    }

                    if (_stream != null)
                    {
                        _stream.Dispose();
                    }
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
