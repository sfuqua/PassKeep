using PassKeep.Lib.Contracts.KeePass;
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
        private StorageFile _file;

        /// <summary>
        /// Constructs an instance from the specified file
        /// </summary>
        /// <param name="file"></param>
        public KeyFile(StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            _file = file;
        }

        // Attempts to read the file as the XML format
        // Valid files have this format:
        /* <KeyFile>
         *     <Key>
         *         <Data>base64-string</Data>
         *     </Key>
         * </KeyFile>
         */

        private bool readKeyfileXml(IBuffer data, out IBuffer output)
        {
            output = null;

            using (var fileStream = data.AsStream())
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
        /// Asynchronously fetches the data from this KeyFile
        /// </summary>
        /// <remarks>XML is tried first, otherwise the binary data is returned</remarks>
        /// <returns>An IBuffer representing the security token data</returns>
        public async Task<IBuffer> GetBuffer()
        {
            using (var stream = await _file.OpenReadAsync())
            {
                using (var reader = new DataReader(stream))
                {
                    uint size = (uint)stream.Size;
                    await reader.LoadAsync(size);
                    IBuffer fileData = reader.ReadBuffer(size);

                    IBuffer xmlData;
                    if (!readKeyfileXml(fileData, out xmlData))
                    {
                        stream.Seek(0);
                        await reader.LoadAsync(size);
                    }
                    else
                    {
                        return xmlData;
                    }

                    // Loading as a file entails:
                    // * If file is 32 bytes, use that (LoadBinaryKey32 == file data).
                    // * If 64 bytes, "LoadHexKey32" - read as string, ensure hex (or fall through), convert to bytes.
                    IBuffer keyData;
                    switch (size)
                    {
                        case 32:
                            keyData = fileData;
                            break;
                        case 64:
                            string hex = reader.ReadString(64);
                            if (hex.Length != 64)
                            {
                                goto default;
                            }

                            try
                            {
                                keyData = CryptographicBuffer.DecodeFromHexString(hex);
                            }
                            catch (Exception hr)
                            {
                                if ((uint)hr.HResult == 0x80090005)
                                {
                                    goto default;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            break;
                        default:
                            var sha256 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
                            var hash = sha256.CreateHash();
                            hash.Append(fileData);

                            keyData = hash.GetValueAndReset();
                            break;
                    }

                    return keyData;
                }
            }
        }
    }
}
