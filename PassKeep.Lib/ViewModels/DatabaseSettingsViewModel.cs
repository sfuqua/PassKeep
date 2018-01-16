// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Kdf;
using SariphLib.Diagnostics;
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
                DebugHelper.Assert(this.aesParams != null);
                
                this.argonParams = new Argon2Parameters(2, 64, 100);
            }
            else
            {
                DebugHelper.Assert(KdfGuid.Equals(Argon2Parameters.Argon2Uuid));
                this.argonParams = this.settingsProvider.KdfParameters as Argon2Parameters;
                DebugHelper.Assert(this.argonParams != null);

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
                        DebugHelper.Assert(false);
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(KdfIterations));
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
                    DebugHelper.Assert(KdfGuid == this.aesParams.Uuid);
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
                    DebugHelper.Assert(KdfGuid == this.aesParams.Uuid);
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

        /// <summary>
        /// Retrieves the current <see cref="KdfParameters"/> represented by these settings.
        /// </summary>
        /// <returns></returns>
        public KdfParameters GetKdfParameters()
        {
            return this.settingsProvider.KdfParameters;
        }

        /// <summary>
        /// Replaces the current <see cref="KdfParameters"/>
        /// </summary>
        /// <param name="parameters"></param>
        public void SetKdfParameters(KdfParameters parameters)
        {
            this.settingsProvider.KdfParameters = parameters;
            if (parameters is Argon2Parameters argonParams)
            {
                this.argonParams = argonParams;
            }
            else if (parameters is AesParameters aesParams)
            {
                this.aesParams = aesParams;
            }
        }
    }
}
