using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.KeePass.Kdf;
using SariphLib.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (settingsProvider == null)
            {
                throw new ArgumentNullException(nameof(settingsProvider));
            }

            this.settingsProvider = settingsProvider;
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
        int KdfIterations
        {
            get
            {
                if (KdfGuid == this.argonParams.Uuid)
                {
                    return this.argonParams.tran
                }
            }
            set;
        }

        /// <summary>
        /// If Argon2 is the <see cref="Cipher"/>, configures the degree
        /// of parallelism.
        /// </summary>
        int ArgonParallelism
        {
            get;
            set;
        }

        /// <summary>
        /// If Argon2 is the <see cref="Cipher"/>, configures the amount of
        /// memory used.
        /// </summary>
        int ArgonBlockCount
        {
            get;
            set;
        }
    }
}
