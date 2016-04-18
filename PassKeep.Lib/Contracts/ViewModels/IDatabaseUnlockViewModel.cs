using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Mvvm;
using SariphLib.Eventing;
using System;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;
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
        /// The database candidate potentially representing the locked document.
        /// </summary>
        IDatabaseCandidate CandidateFile { get; }

        /// <summary>
        /// Whether <see cref="CandidateFile"/> represents a read-only file.
        /// </summary>
        bool IsReadOnly { get; }

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
        /// Whether to not allow remembering the current database.
        /// </summary>
        bool ForbidRememberingDatabase { get; }

        /// <summary>
        /// Whether to remember this database on a successful unlock for future access.
        /// </summary>
        bool RememberDatabase { get; set; }

        /// <summary>
        /// An access list for remembering permission to files for the future.
        /// </summary>
        IDatabaseAccessList FutureAccessList { get; }

        /// <summary>
        /// ActionCommand used to attempt a document unlock using the provided credentials.
        /// </summary>
        AsyncActionCommand UnlockCommand { get; }

        /// <summary>
        /// Loads saved credentials from storage and then performs the same work as
        /// <see cref="UnlockCommand"/>.
        /// </summary>
        AsyncActionCommand UseSavedCredentialsCommand { get; }

        /// <summary>
        /// Event that indicates an attempt to read the header has finished with either a positive or negative result.
        /// </summary>
        event EventHandler HeaderValidated;

        /// <summary>
        /// Event that indicates a decrypted document is ready for consumtpion.
        /// </summary>
        event EventHandler<DocumentReadyEventArgs> DocumentReady;

        /// <summary>
        /// Event that indicates a stored credential could not be added because the provider was full.
        /// </summary>
        event EventHandler<CredentialStorageFailureEventArgs> CredentialStorageFailed;

        /// <summary>
        /// Whether the cleartext header of the candidate file is valid.
        /// </summary>
        bool HasGoodHeader { get; }

        /// <summary>
        /// The result of the last parse operation (either header validation or decryption).
        /// </summary>
        ReaderResult ParseResult { get; }

        /// <summary>
        /// Whether this database has saved credentials that can be auto-populated.
        /// </summary>
        bool HasSavedCredentials { get; }

        /// <summary>
        /// Whether to save this database's credentials on a successful decryption.
        /// </summary>
        bool SaveCredentials { get; set; }

        /// <summary>
        /// The status of the user identity verification service. If the
        /// service is unavailable, <see cref="SaveCredentials"/> should be false.
        /// </summary>
        UserConsentVerifierAvailability IdentityVerifiability { get; }

        /// <summary>
        /// Updates the ViewModel with a new candidate file, which kicks off
        /// a new header validation and stored credential check.
        /// </summary>
        /// <param name="newCandidate">The new database candidate.</param>
        /// <returns>A task that completes when the candidat is updated.</returns>
        Task UpdateCandidateFileAsync(IDatabaseCandidate newCandidate);
    }
}
