using System;

namespace PassKeep.Lib.Util
{
    /// <summary>
    /// Utility class for dealing with bytes and arrays of them.
    /// </summary>
    public static class ByteHelper
    {
        /// <summary>
        /// Performs an in-place xor of a set of bytes (data) with
        /// a pad of values.
        /// </summary>
        /// <param name="pad">The values that dictate the xor operation.</param>
        /// <param name="padOffset">Offset into the pad for xor.</param>
        /// <param name="data">The data to modify in-place with the xor.</param>
        /// <param name="dataOffset">Offset into the data.</param>
        /// <param name="count">How many bytes to operate on.</param>
        public static void Xor(byte[] pad, int padOffset, byte[] data, int dataOffset, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (pad == null)
            {
                throw new ArgumentNullException(nameof(pad));
            }

            for (int i = 0; i < count; i++)
            {
                data[i + dataOffset] ^= pad[i + padOffset];
            }
        }
    }
}
