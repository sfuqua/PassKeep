// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.KeePass.Rng;
using PassKeep.Lib.Util;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Security.Cryptography;

namespace PassKeep.Lib.Models
{
    /// <summary>
    /// Represents binary data (a byte[]) that is potentially
    /// encrypted in memory.
    /// </summary>
    public class ProtectedBinary
    {
        /// <summary>
        /// Random data used to encrypt the payload data of this binary.
        /// </summary>
        private static readonly byte[] EncKey = CryptographicBuffer.GenerateRandom(32).ToArray();
        private static int Id = 0;

        private readonly uint id;
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

            this.id = (uint)Id;
            Interlocked.Increment(ref Id);

            this.data = new byte[length];
            Buffer.BlockCopy(data, offset, this.data, 0, length);

            this.protect = protect;

            if (ProtectionRequested)
            {
                XorData(this.data);
            }
        }

        /// <summary>
        /// Whether the binary was initialized as protected.
        /// </summary>
        public bool ProtectionRequested
        {
            get { return this.protect; }
        }

        /// <summary>
        /// Returns a copy of the (potentially protected) data represented by this binary.
        /// </summary>
        /// <returns>A copy of the internal data.</returns>
        public byte[] GetRawData()
        {
            byte[] buffer = new byte[this.data.Length];
            Buffer.BlockCopy(this.data, 0, buffer, 0, this.data.Length);

            return buffer;
        }

        /// <summary>
        /// Returns a copy of the (unprotected) data represented by this binary.
        /// </summary>
        /// <returns>A copy of the internal data, decrypted.</returns>
        public byte[] GetClearData()
        {
            byte[] buffer = GetRawData();
            
            if (ProtectionRequested)
            {
                XorData(buffer);
            }

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

            byte[] otherData = other.GetClearData();
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

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 27;
                hash = (13 * hash) + ProtectionRequested.GetHashCode();
                hash = (13 * hash) + this.data.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Encrypts or decrypts the provided data.
        /// </summary>
        /// <param name="data"></param>
        private void XorData(byte[] data)
        {
            ChaCha20 cipher = new ChaCha20(
                EncKey,
                new byte[12],
                this.id
            );

            ByteHelper.Xor(
                cipher.GetBytes((uint)data.Length),
                0,
                data,
                0,
                data.Length
            );
        }
    }
}
