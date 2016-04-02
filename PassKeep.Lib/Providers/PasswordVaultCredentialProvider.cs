﻿using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Providers;
using SariphLib.Infrastructure;
using System;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// An implementation of <see cref="ICredentialStorageProvider"/> that uses the Windows
    /// <see cref="PasswordVault"/> to secure credentials.
    /// </summary>
    public sealed class PasswordVaultCredentialProvider : ICredentialStorageProvider
    {
        /// <summary>
        /// An Exception with this HResult is thrown by <see cref="PasswordVault"/> when
        /// attempting to add a credential when there are too many in storage.
        /// </summary>
        /// <remarks>
        /// COMException is thrown in .NET, Exception by .NET native.
        /// Testing indicates the current limit is 20 (as of Apr 2 2016).
        /// </remarks>
        private const int ErrorTooManySecrets = unchecked((int)0x80070565);

        /// <summary>
        /// An Exception with this HResult is thrown by <see cref="PasswordVault"/> when
        /// attempting to retrieve or delete a credential that does not exist.
        /// </summary>
        /// <remarks>COMException is thrown in .NET, Exception by .NET native.</remarks>
        private const int ErrorNotFound = unchecked((int)0x80070490);

        private const string ResourceKey = "PassKeepStoredKey";

        private readonly PasswordVault vault;

        /// <summary>
        /// Initializes the PasswordVault.
        /// </summary>
        public PasswordVaultCredentialProvider()
        {
            this.vault = new PasswordVault();
        }

        /// <summary>
        /// Asynchronously fetches data representing the raw
        /// aggregate key for a database.
        /// </summary>
        /// <param name="database">Data identifying the key to fetch.</param>
        /// <returns>A task representing the key data, which will be null if no
        /// stored credential exists.</returns>
        public Task<IBuffer> GetRawKeyAsync(IDatabaseCandidate database)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            PasswordCredential storedCredential;
            try
            {
                storedCredential = this.vault.Retrieve(ResourceKey, GetUserNameToken(database));
            }
            catch (Exception e) when (e.HResult == ErrorNotFound)
            {
                return Task.FromResult<IBuffer>(null);
            }

            storedCredential.RetrievePassword();
            return Task.FromResult(StringToIBuffer(storedCredential.Password));
        }

        /// <summary>
        /// Asynchronously stores the key for a database in a secure location.
        /// The existing credential is overwritten.
        /// </summary>
        /// <param name="database">Data identifying the key for future retrieval.</param>
        /// <param name="key">The key to store.</param>
        /// <returns>A task representing whether the storage is successful.</returns>
        public Task<bool> TryStoreRawKeyAsync(IDatabaseCandidate database, IBuffer key)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            PasswordCredential credential = new PasswordCredential(
                ResourceKey,
                GetUserNameToken(database),
                IBufferToString(key)
            );

            try
            {
                this.vault.Add(credential);
                return Task.FromResult(true);
            }
            catch (Exception e) when (e.HResult == ErrorTooManySecrets)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Converts a database candidate to a string that can be used to lookup a credential
        /// (as a username).
        /// </summary>
        /// <param name="candidate">The database to convert.</param>
        /// <returns>A string that can be used to look up the database in the PasswordVault.</returns>
        private static string GetUserNameToken(IDatabaseCandidate candidate)
        {
            Dbg.Assert(candidate != null);
            return candidate.FileName;
        }

        /// <summary>
        /// Converts a credential to a string that can be stored in <see cref="PasswordVault"/>.
        /// Inverse of <see cref="StringToIBuffer(string)"/>.
        /// </summary>
        /// <param name="buffer">The credential to encode.</param>
        /// <returns>A string suitable for storing in <see cref="PasswordVault"/>.</returns>
        private static string IBufferToString(IBuffer buffer)
        {
            Dbg.Assert(buffer != null);
            return CryptographicBuffer.EncodeToBase64String(buffer);
        }

        /// <summary>
        /// Converts a stored <see cref="PasswordVault"/> password to an <see cref="IBuffer"/>
        /// for database decryption.
        /// </summary>
        /// <param name="str">The string to decode.</param>
        /// <returns>An <see cref="IBuffer"/> suitable for decrypting a KeePass database.</returns>
        private static IBuffer StringToIBuffer(string str)
        {
            Dbg.Assert(!string.IsNullOrEmpty(str));
            return CryptographicBuffer.DecodeFromBase64String(str);
        }
    }
}
