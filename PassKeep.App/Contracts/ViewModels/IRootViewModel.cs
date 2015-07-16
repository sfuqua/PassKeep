﻿using PassKeep.Lib.Contracts.Enums;
using System;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IRootViewModel : IViewModel
    {
        /// <summary>
        /// Fired when the automated clipboard clear timer failed to clear the clipboard, in order to notify the view.
        /// </summary>
        event EventHandler ClipboardClearFailed;

        ActivationMode ActivationMode
        {
            get;
        }

        IStorageFile CandidateFile
        {
            get;
            set;
        }

        IClipboardClearTimerViewModel ClipboardClearViewModel
        {
            get;
            set;
        }

        IDatabaseParentViewModel DecryptedDatabase
        {
            get;
            set;
        }

        IPasswordGenViewModel PasswordGenViewModel
        {
            get;
            set;
        }
    }
}
