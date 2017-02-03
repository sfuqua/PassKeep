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

        /// <summary>
        /// Helper to left shift a uint.
        /// </summary>
        /// <param name="x">The value to shift.</param>
        /// <param name="c">How many bits to shift.</param>
        /// <returns>The shifted value.</returns>
        public static uint LeftShift(uint x, int c)
        {
            return (x << c) | (x >> (32 - c));
        }

        /// <summary>
        /// Given a data buffer, reads out a little endian uint.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <param name="offset">Offset into <paramref name="data"/>.</param>
        /// <returns>A uint based on the first four bytes from <paramref name="data"/>.</returns>
        public static uint BufferToLittleEndianUInt(byte[] data, int offset)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length - offset < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt32(data, offset);
            }

            return (uint)(data[0 + offset] + LeftShift(data[1 + offset], 8) + LeftShift(data[2 + offset], 16) + LeftShift(data[3 + offset], 24));
        }

        /// <summary>
        /// Given a uint, returns four bytes in little endian order.
        /// </summary>
        /// <param name="number">The data to break down.</param>
        /// <returns>The four bytes in little endian order that make up <paramref name="number"/>.</returns>
        public static byte[] GetLittleEndianBytes(uint number)
        {
            return new byte[4]
            {
                (byte)(number & 0xFF),
                (byte)((number >> 8) & 0xFF),
                (byte)((number >> 16) & 0xFF),
                (byte)((number >> 24) & 0xFF)
            };
        }
    }
}
