using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;

namespace PassKeep.Models
{
    public class KeePassUuid
    {
        public Guid Uid
        {
            get;
            private set;
        }

        public string EncodedValue
        {
            get;
            private set;
        }

        public KeePassUuid()
            : this(Guid.NewGuid())
        { }

        public KeePassUuid(Guid guid)
        {
            Uid = guid;
            EncodedValue = CryptographicBuffer.EncodeToBase64String(Uid.ToByteArray().AsBuffer());
        }

        public KeePassUuid(string encoded)
        {
            Debug.Assert(!string.IsNullOrEmpty(encoded));
            if (string.IsNullOrEmpty(encoded))
            {
                //throw new ArgumentException("string cannot be null or empty", "encoded");
                encoded = Convert.ToBase64String(Guid.Empty.ToByteArray());
            }

            try
            {
                byte[] bytes = CryptographicBuffer.DecodeFromBase64String(encoded).ToArray();
                Debug.Assert(bytes.Length == 16);
                if (bytes.Length != 16)
                {
                    throw new ArgumentException("wrong number of bytes in base64 string", "encoded");
                }
                Uid = new Guid(bytes);
                EncodedValue = encoded;
            }
            catch (Exception hr)
            {
                if ((uint)hr.HResult == 0x80090005)
                {
                    throw new ArgumentException("Unable to decode base64 string", "encoded");
                }
                else
                {
                    throw;
                }
            }
        }

        public override bool Equals(object obj)
        {
            KeePassUuid other = obj as KeePassUuid;
            if (other == null)
            {
                return false;
            }

            return Uid.Equals(other.Uid);
        }

        public override string ToString()
        {
            return EncodedValue;
        }

        public override int GetHashCode()
        {
            return Uid.GetHashCode();
        }

        public KeePassUuid Clone()
        {
            return new KeePassUuid(this.Uid);
        }
    }
}
