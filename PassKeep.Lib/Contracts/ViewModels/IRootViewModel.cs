// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using SariphLib.Files;
using System;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IRootViewModel : IViewModel
    {
        /// <summary>
        /// Fired when the automated clipboard clear timer failed to clear the clipboard, in order to notify the view.
        /// </summary>
        event EventHandler ClipboardClearFailed;

        /// <summary>
        /// Whether the app is currenly saving a database.
        /// </summary>
        bool IsSaving
        {
            get;
        }

        ActivationMode ActivationMode
        {
            get;
        }

        ITestableFile CandidateFile
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
        }

        IHelpViewModel HelpViewModel
        {
            get;
        }

        IAppSettingsViewModel AppSettingsViewModel
        {
            get;
        }

        /// <summary>
        /// Notifies the view of UI-blocking operations.
        /// </summary>
        ITaskNotificationService TaskNotificationService
        {
            get;
        }
    }
}
