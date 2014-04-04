using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Mvvm;
using System;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Represents a View over a locked database, with the potential for unlocking.
    /// </summary>
    public interface IDatabaseUnlockViewModel : IViewModel
    {
        /// <summary>
        /// The StorageFile representing the locked database.
        /// </summary>
        StorageFile CandidateFile { get; set; }

        /// <summary>
        /// Whether or not this database is the PassKeep sample database.
        /// </summary>
        bool IsSampleFile { get; set; }

        /// <summary>
        /// The password used to unlock the database.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// The key file used to unlock the database.
        /// </summary>
        StorageFile KeyFile { get; set; }

        /// <summary>
        /// ActionCommand used to attempt a database unlock using the provided credentials.
        /// </summary>
        ActionCommand UnlockCommand { get; }

        /// <summary>
        /// Event that indicates an unlock attempt has begun.
        /// </summary>
        event EventHandler<CancelableEventArgs> StartedUnlocking;

        /// <summary>
        /// Event that indicates an unlock attempt has stopped (successfully or unsuccessfully).
        /// </summary>
        event EventHandler StoppedUnlocking;

        /// <summary>
        /// Event that indicates a decrypted document is ready for consumtpion.
        /// </summary>
        event EventHandler<DocumentReadyEventArgs> DocumentReady;

        /// <summary>
        /// Whether the cleartext header of the candidate file is valid.
        /// </summary>
        bool HasGoodHeader { get; }

        /// <summary>
        /// The result of the last parse operation (either header validation or decryption).
        /// </summary>
        ReaderResult ParseResult { get; }
    }
}
