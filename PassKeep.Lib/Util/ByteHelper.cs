// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

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
        /// Helper to left rotate a uint.
        /// </summary>
        /// <param name="x">The value to rotate.</param>
        /// <param name="c">How many bits to rotate.</param>
        /// <returns>The rotated value.</returns>
        public static uint RotateLeft(uint x, int c)
        {
            return (x << c) | (x >> (32 - c));
        }

        /// <summary>
        /// Helper to left rotate a ulong.
        /// </summary>
        /// <param name="x">The value to rotate.</param>
        /// <param name="c">How many bits to rotate.</param>
        /// <returns>The rotated value.</returns>
        public static ulong RotateLeft(ulong x, int c)
        {
            return (x << c) | (x >> (64 - c));
        }

        /// <summary>
        /// Helper to right rotate a uint.
        /// </summary>
        /// <param name="x">The value to rotate.</param>
        /// <param name="c">How many bits to rotate.</param>
        /// <returns>The rotated value.</returns>
        public static uint RotateRight(uint x, int c)
        {
            return (x >> c) ^ (x << (32 - c));
        }

        /// <summary>
        /// Helper to right rotate a ulong.
        /// </summary>
        /// <param name="x">The value to rotate.</param>
        /// <param name="c">How many bits to rotate.</param>
        /// <returns>The rotated value.</returns>
        public static ulong RotateRight(ulong x, int c)
        {
            return (x >> c) ^ (x << (64 - c));
        }

        /// <summary>
        /// Given a data buffer, reads out a little endian uint.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <param name="offset">Offset into <paramref name="data"/>.</param>
        /// <returns>A uint based on the first four bytes from <paramref name="data"/>.</returns>
        public static uint BufferToLittleEndianUInt32(byte[] data, int offset)
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

            return (uint)(data[0 + offset] | (data[1 + offset] << 8) | (data[2 + offset] << 16) | (data[3 + offset] << 24));
        }

        /// <summary>
        /// Given a data buffer, reads out a little endian ulong.
        /// </summary>
        /// <param name="data">The data to convert.</param>
        /// <param name="offset">Offset into <paramref name="data"/>.</param>
        /// <returns>A ulong based on the first eight bytes from <paramref name="data"/>.</returns>
        public static ulong BufferToLittleEndianUInt64(byte[] data, int offset)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length - offset < 8)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt64(data, offset);
            }

            return (ulong)(data[0 + offset]
                | (data[1 + offset] << 0x08)
                | (data[2 + offset] << 0x10)
                | (data[3 + offset] << 0x18)
                | (data[4 + offset] << 0x20)
                | (data[5 + offset] << 0x28)
                | (data[6 + offset] << 0x30)
                | (data[7 + offset] << 0x38));
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

        /// <summary>
        /// Given a uint, copies four bytes in little endian order into the given buffer.
        /// </summary>
        /// <param name="number">The data to break down.</param>
        /// <param name="buffer">The buffer to copy the bytes into.</param>
        /// <param name="offset">Where to start the copy.</param>
        public static void GetLittleEndianBytes(uint number, byte[] buffer, int offset = 0)
        {
            if (buffer == null)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer));
            }

            if (buffer.Length - offset < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer));
            }

            buffer[offset + 0] = (byte)(number & 0xFF);
            buffer[offset + 1] = (byte)((number >> 0x08) & 0xFF);
            buffer[offset + 2] = (byte)((number >> 0x10) & 0xFF);
            buffer[offset + 3] = (byte)((number >> 0x18) & 0xFF);
        }

        /// <summary>
        /// Given a ulong, returns eight bytes in little endian order.
        /// </summary>
        /// <param name="number">The data to break down.</param>
        /// <returns>The eight bytes in little endian order that make up <paramref name="number"/>.</returns>
        public static byte[] GetLittleEndianBytes(ulong number)
        {
            return new byte[8]
            {
                (byte)(number & 0xFF),
                (byte)((number >> 0x08) & 0xFF),
                (byte)((number >> 0x10) & 0xFF),
                (byte)((number >> 0x18) & 0xFF),
                (byte)((number >> 0x20) & 0xFF),
                (byte)((number >> 0x28) & 0xFF),
                (byte)((number >> 0x30) & 0xFF),
                (byte)((number >> 0x38) & 0xFF)
            };
        }

        /// <summary>
        /// Given a ulong, copies eight bytes in little endian order into the given buffer.
        /// </summary>
        /// <param name="number">The data to break down.</param>
        /// <param name="buffer">The buffer to copy the bytes into.</param>
        /// <param name="offset">Buffer offset to begin the copy.</param>
        public static void GetLittleEndianBytes(ulong number, byte[] buffer, int offset = 0)
        {
            if (buffer == null)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer));
            }

            if (buffer.Length - offset < 8)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer));
            }

            buffer[0 + offset] = (byte)(number & 0xFF);
            buffer[1 + offset] = (byte)((number >> 0x08) & 0xFF);
            buffer[2 + offset] = (byte)((number >> 0x10) & 0xFF);
            buffer[3 + offset] = (byte)((number >> 0x18) & 0xFF);
            buffer[4 + offset] = (byte)((number >> 0x20) & 0xFF);
            buffer[5 + offset] = (byte)((number >> 0x28) & 0xFF);
            buffer[6 + offset] = (byte)((number >> 0x30) & 0xFF);
            buffer[7 + offset] = (byte)((number >> 0x38) & 0xFF);
        }
    }
}
