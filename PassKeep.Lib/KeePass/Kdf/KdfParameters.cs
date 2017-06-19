using PassKeep.Lib.KeePass.IO;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Generates a new parameters object based on reseeding the random keys
        /// that make up the current instance.
        /// </summary>
        /// <returns>A KDF parameters instance with new seeds.</returns>
        public virtual KdfParameters Reseed()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Constructs a serializable <see cref="VariantDictionary"/> representing these parameters.
        /// </summary>
        /// <returns></returns>
        public VariantDictionary ToVariantDictionary()
        {
            return new VariantDictionary(
                ToDictionary()
            );
        }

        /// <summary>
        /// Constructs a dictionary from the values that make up this paramaterization.
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<string, VariantValue> ToDictionary()
        {
            return new Dictionary<string, VariantValue>
            {
                { UuidKey, new VariantValue(Uuid.ToByteArray()) }
            };
        }
    }
}
