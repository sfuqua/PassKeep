using PassKeep.Lib.KeePass.IO;
using System;
using Windows.Storage.Streams;

namespace PassKeep.Lib.KeePass.Kdf
{
    /// <summary>
    /// Base parameters common to all KDF algorithms.
    /// </summary>
    public class KdfParameters
    {
        public static readonly string UuidKey = "$UUID";

        private readonly Guid uuid;

        /// <summary>
        /// Parses base parameters from the provided dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary containing KDF parameters.</param>
        public KdfParameters(VariantDictionary dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            object uuid = dictionary.GetValue(UuidKey);
            if (uuid == null)
            {
                throw new FormatException("No UUID value in dictionary for KdfParameters");
            }

            byte[] uuidBytes = uuid as byte[];
            if (uuidBytes == null)
            {
                throw new FormatException("UUID value in KdfParameters was not a byte array");
            }

            if (uuidBytes.Length != 16)
            {
                throw new FormatException($"Expected 16 UUID bytes in KdfParameters, got {uuidBytes.Length}");
            }

            this.uuid = new Guid(uuidBytes);
        }

        /// <summary>
        /// Initializes the parameters with a specific UUID.
        /// </summary>
        /// <param name="uuid">UUID for the KDF.</param>
        public KdfParameters(Guid uuid)
        {
            this.uuid = uuid;
        }

        /// <summary>
        /// The UUID for this KDF.
        /// </summary>
        public Guid Uuid
        {
            get { return this.uuid; }
        }

        /// <summary>
        /// Constructs a KDF crypto engine that can be used to transform
        /// a provided key.
        /// </summary>
        /// <returns>A crypto engine used to transform a composite key.</returns>
        public virtual IKdfEngine CreateEngine()
        {
            throw new NotImplementedException();
        }
    }
}
