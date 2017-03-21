﻿using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Kdf;
using SariphLib.Infrastructure;
using System;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Exposes KDF and cipher settings to the view.
    /// </summary>
    public class DatabaseSettingsViewModel : AbstractViewModel, IDatabaseSettingsViewModel
    {
        private readonly IDatabaseSettingsProvider settingsProvider;

        private AesParameters aesParams;
        private Argon2Parameters argonParams;

        public DatabaseSettingsViewModel(IDatabaseSettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));

            if (KdfGuid.Equals(AesParameters.AesUuid))
            {
                this.aesParams = this.settingsProvider.KdfParameters as AesParameters;
                Dbg.Assert(this.aesParams != null);

                // FIXME: Better defaults
                this.argonParams = new Argon2Parameters(2, 2, 2);
            }
            else
            {
                Dbg.Assert(KdfGuid.Equals(Argon2Parameters.Argon2Uuid));
                this.argonParams = this.settingsProvider.KdfParameters as Argon2Parameters;
                Dbg.Assert(this.argonParams != null);

                this.aesParams = new AesParameters(6000);
            }
        }

        /// <summary>
        /// Algorithm to use for encrypting the database.
        /// </summary>
        public EncryptionAlgorithm Cipher
        {
            get { return this.settingsProvider.Cipher; }
            set
            {
                if (Cipher != value)
                {
                    this.settingsProvider.Cipher = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identifier for the key derivation function to use.
        /// </summary>
        public Guid KdfGuid
        {
            get { return this.settingsProvider.KdfParameters.Uuid; }
            set
            {
                if (KdfGuid != value)
                {
                    if (value.Equals(Argon2Parameters.Argon2Uuid))
                    {
                        this.settingsProvider.KdfParameters = this.argonParams;
                    }
                    else if (value.Equals(AesParameters.AesUuid))
                    {
                        this.settingsProvider.KdfParameters = this.aesParams;
                    }
                    else
                    {
                        Dbg.Assert(false);
                    }

                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of transform rounds to use for the KDF.
        /// </summary>
        public ulong KdfIterations
        {
            get
            {
                if (KdfGuid == this.argonParams.Uuid)
                {
                    return this.argonParams.Iterations;
                }
                else
                {
                    Dbg.Assert(KdfGuid == this.aesParams.Uuid);
                    return this.aesParams.Rounds;
                }
            }
            set
            {
                if (KdfGuid == this.argonParams.Uuid)
                {
                    this.argonParams.Iterations = value;
                }
                else
                {
                    Dbg.Assert(KdfGuid == this.aesParams.Uuid);
                    this.aesParams.Rounds = value;
                }
            }
        }

        /// <summary>
        /// If Argon2 is the <see cref="Cipher"/>, configures the degree
        /// of parallelism.
        /// </summary>
        public uint ArgonParallelism
        {
            get => this.argonParams.Parallelism;
            set => this.argonParams.Parallelism = value;
        }

        /// <summary>
        /// If Argon2 is the <see cref="Cipher"/>, configures the amount of
        /// memory used.
        /// </summary>
        public ulong ArgonBlockCount
        {
            get => this.argonParams.BlockCount;
            set => this.argonParams.BlockCount = value;
        }
    }
}
