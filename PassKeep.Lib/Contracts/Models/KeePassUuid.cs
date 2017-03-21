using SariphLib.Infrastructure;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;

namespace PassKeep.Lib.Contracts.Models
{
    /// <summary>
    /// Represents a combined Guid and its base64 encoding.
    /// </summary>
    /// <remarks>Used for identifying objects in the DOM tree</remarks>
    public class KeePassUuid
    {
        /// <summary>
        /// A Uuid that wraps <see cref="Guid.Empty"/>.
        /// </summary>
        public static readonly KeePassUuid Empty = new KeePassUuid(Guid.Empty);

        /// <summary>
        /// The globally unique identifier for this object
        /// </summary>
        public Guid Uid
        {
            get;
            private set;
        }

        /// <summary>
        /// The base64 encoded value of the associated Guid
        /// </summary>
        public string EncodedValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates an instance from a new Guid
        /// </summary>
        public KeePassUuid()
            : this(Guid.NewGuid())
        { }

        /// <summary>
        /// Generates an instance from the given Guid
        /// </summary>
        /// <param name="guid"></param>
        public KeePassUuid(Guid guid)
        {
            Uid = guid;
            EncodedValue = CryptographicBuffer.EncodeToBase64String(Uid.ToByteArray().AsBuffer());
        }

        /// <summary>
        /// Generates an instance from a base64 encoded string
        /// </summary>
        /// <param name="encoded">The string to convert - 
        /// null or empty results in an empty Guid</param>
        public KeePassUuid(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
            {
                encoded = Convert.ToBase64String(Guid.Empty.ToByteArray());
            }

            try
            {
                byte[] bytes = CryptographicBuffer.DecodeFromBase64String(encoded).ToArray();
                Dbg.Assert(bytes.Length == 16);
                if (bytes.Length != 16)
                {
                    throw new ArgumentException("Wrong number of bytes in base64 string", nameof(encoded));
                }
                Uid = new Guid(bytes);
                EncodedValue = encoded;
            }
            catch (Exception hr)
            {
                if ((uint)hr.HResult == 0x80090005)
                {
                    throw new ArgumentException("Unable to decode base64 string", nameof(encoded));
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
            return new KeePassUuid(Uid);
        }
    }
}
