// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.Services;
using System;
using Windows.Foundation;

namespace PassKeep.Lib.Services
{
    /// <summary>
    /// Abstracts interface to the clipboard for handling sensitive data (e.g. user credentials).
    /// </summary>
    public class SensitiveClipboardService : ISensitiveClipboardService
    {
        private IClipboardProvider clipboard;

        /// <summary>
        /// Constructs the service with the specified clipboard.
        /// </summary>
        /// <param name="clipboard">The clipboard to use.</param>
        public SensitiveClipboardService(IClipboardProvider clipboard)
        {
            this.clipboard = clipboard;
        }

        /// <summary>
        /// Fired when data is copied to the clipboard.
        /// </summary>
        public event TypedEventHandler<ISensitiveClipboardService, ClipboardOperationType> CredentialCopied;

        /// <summary>
        /// Fires the <see cref="CredentialCopied"/> event.
        /// </summary>
        /// <param name="copyType"></param>
        private void FireCredentialCopied(ClipboardOperationType copyType)
        {
            CredentialCopied?.Invoke(this, copyType);
        }

        /// <summary>
        /// Copies sensitive data to the clipboard.
        /// </summary>
        /// <param name="credential">The data to copy.</param>
        /// <param name="copyType">The type of data being copied.</param>
        public void CopyCredential(string credential, ClipboardOperationType copyType)
        {
            if (String.IsNullOrEmpty(credential))
            {
                this.clipboard.SetContent(null);
            }
            else
            { 
                this.clipboard.SetContent(credential);
                FireCredentialCopied(copyType);
            }
        }


        /// <summary>
        /// Attempts to clear the clipboard.
        /// </summary>
        /// <returns>Whether the clear was successful.</returns>
        public bool TryClear()
        {
            try
            {
                this.clipboard.Clear();
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
