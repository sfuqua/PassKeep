using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PassKeep.KeePassLib.SecurityTokens
{
    public class KeyFile : ISecurityToken
    {
        private StorageFile _file;
        public KeyFile(StorageFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            _file = file;
        }

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
