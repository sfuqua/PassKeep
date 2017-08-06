// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Enums;
using Windows.Foundation;

namespace PassKeep.Lib.Contracts.Services
{
    /// <summary>
    /// Abstracts interface to the clipboard for handling sensitive data (e.g. user credentials).
    /// </summary>
    public interface ISensitiveClipboardService
    {
        /// <summary>
        /// Fired when data is copied to the clipboard.
        /// </summary>
        event TypedEventHandler<ISensitiveClipboardService, ClipboardOperationType> CredentialCopied;

        /// <summary>
        /// Copies sensitive data to the clipboard.
        /// </summary>
        /// <param name="credential">The data to copy.</param>
        /// <param name="copyType">The type of data being copied.</param>
        void CopyCredential(string credential, ClipboardOperationType copyType);

        /// <summary>
        /// Attempts to clear the clipboard.
        /// </summary>
        /// <returns>Whether the clear was successful.</returns>
        bool TryClear();
    }
}
