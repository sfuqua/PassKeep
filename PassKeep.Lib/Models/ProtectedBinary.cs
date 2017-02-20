using System;

namespace PassKeep.Lib.Models
{
    /// <summary>
    /// Represents binary data (a byte[]) that is potentially
    /// encrypted in memory.
    /// </summary>
    public class ProtectedBinary
    {
        private readonly byte[] data;
        private readonly bool protect;

        /// <summary>
        /// Initializes the binary with the provided data.
        /// </summary>
        /// <param name="data">The memory to capture.</param>
        /// <param name="protect">Whether to protect the data in memory.</param>
        public ProtectedBinary(byte[] data, bool protect)
            : this(data, 0, data.Length, protect) { }

        /// <summary>
        /// Initializes the binary with the provided data.
        /// </summary>
        /// <param name="data">The memory to capture.</param>
        /// <param name="offset">Offset into <paramref name="data"/> to begin capture.</param>
        /// <param name="length">Length of data to capture.</param>
        /// <param name="protect">Whether to protect the data in memory.</param>
        public ProtectedBinary(byte[] data, int offset, int length, bool protect)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (offset + length > data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            this.data = new byte[data.Length];
            Buffer.BlockCopy(data, offset, this.data, 0, length);

            this.protect = protect;
        }

        /// <summary>
        /// Whether the binary was initialized as protected.
        /// </summary>
        public bool ProtectionRequested
        {
            get { return this.protect; }
        }

        /// <summary>
        /// Returns a copy of the (unprotected) data represented by this binary.
        /// </summary>
        /// <returns>A copy of the internal data.</returns>
        public byte[] GetData()
        {
            byte[] buffer = new byte[this.data.Length];
            Buffer.BlockCopy(this.data, 0, buffer, 0, this.data.Length);

            return buffer;
        }

        /// <summary>
        /// Checks equality by comparing protection status and that the
        /// data between the two binaries has the same value.
        /// </summary>
        /// <param name="obj">The object to check for equality.</param>
        /// <returns>Whether <see cref="ProtectionRequested"/> is the same for
        /// both objects and their data are copies of each other.</returns>
        public override bool Equals(object obj)
        {
            ProtectedBinary other = obj as ProtectedBinary;
            if (other == null)
            {
                return false;
            }

            if (ProtectionRequested != other.ProtectionRequested)
            {
                return false;
            }

            byte[] otherData = other.GetData();
            if (this.data.Length != otherData.Length)
            {
                return false;
            }

            for (int i = 0; i < this.data.Length; i++)
            {
                if (this.data[i] != otherData[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
