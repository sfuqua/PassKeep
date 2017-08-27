// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using SariphLib.Files;
using SariphLib.Diagnostics;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.SecurityTokens
{
    /// <summary>
    /// A security token that is represented by an arbitrary file on disk.
    /// </summary>
    /// <remarks>Security tokens can have a specific XML schema, or they can
    /// be treated as binary files.</remarks>
    public sealed class KeyFile : ISecurityToken
    {
        private readonly ITestableFile file;
        private IBuffer cachedKeyData;

        /// <summary>
        /// Constructs an instance from the specified file
        /// </summary>
        /// <param name="file"></param>
        public KeyFile(ITestableFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            this.file = file;
            this.cachedKeyData = null;
        }

        /// <summary>
        /// Asynchronously fetches the data from this KeyFile.
        /// </summary>
        /// <remarks>
        /// The algorithm for transforming a keyfile into decryption data is as follows:
        ///  * First, try to parse the file as XML and retrieve the appropriate node in the tree
        ///  * Failing that, if the file is 32 bytes, use the data as-is ("LoadBinaryKey32")
        ///  * If the file is 64 bytes, evaluate the "LoadHexKey32" algorithm
        ///  * Failing all else, compute the SHA256 hash of the file.
        ///  </remarks>
        /// <returns>An IBuffer representing the security token data.</returns>
        public async Task<IBuffer> GetBufferAsync()
        {
            if (this.cachedKeyData != null)
            {
                return this.cachedKeyData;
            }

            using (var stream = await this.file.AsIStorageFile.OpenReadAsync())
            {
                using (var reader = new DataReader(stream))
                {
                    uint size = (uint)stream.Size;
                    await reader.LoadAsync(size);
                    IBuffer fileData = reader.ReadBuffer(size);

                    // First try to parse as XML
                    if (TryReadKeyFileXml(fileData, out this.cachedKeyData))
                    {
                        return this.cachedKeyData;
                    }

                    // If XML didn't work, we use the entire file.
                    // First we attempt to use special decoding based on file size, if that does't work
                    // or the size doesn't match, we hash the entire file.
                    switch (size)
                    {
                        case 32:
                            this.cachedKeyData = LoadBinaryKey32(fileData);
                            break;
                        case 64:
                            this.cachedKeyData = LoadHexKey32(fileData);
                            break;
                    }

                    if (this.cachedKeyData == null)
                    {
                        var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                        var hash = sha256.CreateHash();
                        hash.Append(fileData);

                        this.cachedKeyData = hash.GetValueAndReset();
                    }

                    return this.cachedKeyData;
                }
            }
        }
        
        /// <summary>
        /// Attempts to decode the file as XML.
        /// </summary>
        /// <remarks>
        /// Valid files have this format:
        /// (KeyFile)
        ///   (Key)
        ///     (Data)base64-string(/Data)
        ///   (/Key)
        /// (/KeyFile)
        /// </remarks>
        /// <param name="fileData">The binary representation of the file we are attempting to decode.</param>
        /// <param name="output">The buffer that will be filled with the key data on success.</param>
        /// <returns>Whether decoding as XML succeeded.</returns>
        private static bool TryReadKeyFileXml(IBuffer fileData, out IBuffer output)
        {
            output = null;

            using (var fileStream = fileData.AsStream())
            {
                try
                {
                    var root = XDocument.Load(fileStream).Root;
                    if (root == null || root.Name != "KeyFile")
                    {
                        return false;
                    }

                    var keyElement = root.Element("Key");
                    if (keyElement == null)
                    {
                        return false;
                    }

                    var dataElement = keyElement.Element("Data");
                    if (dataElement == null)
                    {
                        return false;
                    }

                    try
                    {
                        output = CryptographicBuffer.DecodeFromBase64String(dataElement.Value);
                        return true;
                    }
                    catch (Exception hr)
                    {
                        if ((uint)hr.HResult == 0x80090005)
                        {
                            return false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (XmlException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// A 32-byte keyfile is used as is. This is abstracted into the same 
        /// function name as KeePass for maintainability.
        /// </summary>
        /// <param name="fileData">A buffer containing the binary data of the file.</param>
        /// <returns></returns>
        private static IBuffer LoadBinaryKey32(IBuffer fileData)
        {
            DebugHelper.Assert(fileData.Length == 32);
            return fileData;
        }

        /// <summary>
        /// A 64-byte keyfile is read as a UTF8 string, which must represent a hex string.
        /// If the string is not hex, null is returned.
        /// Otherwise the hex string is reinterpreted as a byte array and returned.
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        private static IBuffer LoadHexKey32(IBuffer fileData)
        {
            DebugHelper.Assert(fileData.Length == 64);
            try
            {
                string hexString = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, fileData);
                IBuffer hexData = CryptographicBuffer.DecodeFromHexString(hexString);

                DebugHelper.Assert(hexData.Length == 32);
                return hexData;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
