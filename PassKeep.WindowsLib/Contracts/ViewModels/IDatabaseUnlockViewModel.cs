﻿using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Mvvm;
using System;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// Represents a View over a locked document, with the potential for unlocking.
    /// </summary>
    public interface IDatabaseUnlockViewModel : IViewModel
    {
        object SyncRoot { get; }

        /// <summary>
        /// The StorageFile representing the locked document.
        /// </summary>
        StorageFile CandidateFile { get; set; }

        /// <summary>
        /// Whether or not this document is the PassKeep sample document.
        /// </summary>
        bool IsSampleFile { get; }

        /// <summary>
        /// The password used to unlock the document.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// The key file used to unlock the document.
        /// </summary>
        StorageFile KeyFile { get; set; }

        /// <summary>
        /// ActionCommand used to attempt a document unlock using the provided credentials.
        /// </summary>
        ActionCommand UnlockCommand { get; }

        /// <summary>
        /// Event that indicates an attempt to read the header has finished with either a positive or negative result.
        /// </summary>
        event EventHandler HeaderValidated;

        /// <summary>
        /// Event that indicates an unlock attempt has begun.
        /// </summary>
        event EventHandler<CancellableEventArgs> StartedUnlocking;

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
