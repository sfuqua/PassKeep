// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Contracts.Models;
using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Mvvm;
using System;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// An interface for a ViewModel that represents a timer for clearing the clipboard.
    /// </summary>
    public interface IClipboardClearTimerViewModel : IViewModel
    {
        /// <summary>
        /// Fired when a timer successfully terminates.
        /// </summary>
        event EventHandler<ClipboardTimerCompleteEventArgs> TimerComplete;

        /// <summary>
        /// Whether the ViewModel supports clearing the clipboard for a username copy.
        /// </summary>
        bool UserNameClearEnabled
        {
            get;
        }

        /// <summary>
        /// The amount of time remaining for the current username clear timer (0 to 1).
        /// </summary>
        double NormalizedUserNameTimeRemaining
        {
            get;
        }

        /// <summary>
        /// The amount of time remaining (in seconds) for the current username clear timer.
        /// </summary>
        double UserNameTimeRemainingInSeconds
        {
            get;
        }

        /// <summary>
        /// Whether the ViewModel supports clearing the clipboard for a password copy.
        /// </summary>
        bool PasswordClearEnabled
        {
            get;
        }

        /// <summary>
        /// The amount of time remaining for the current password clear timer (0 to 1).
        /// </summary>
        double NormalizedPasswordTimeRemaining
        {
            get;
        }

        /// <summary>
        /// The amount of time remaining (in seconds) for the current password clear timer.
        /// </summary>
        double PasswordTimeRemainingInSeconds
        {
            get;
        }

        /// <summary>
        /// Whether the ViewModel supports clearing the clipboard for a non-credential copy.
        /// </summary>
        bool OtherClearEnabled
        {
            get;
        }

        /// <summary>
        /// The amount of time remaining for the current non-credential clear timer (0 to 1).
        /// </summary>
        double NormalizedOtherTimeRemaining
        {
            get;
        }

        /// <summary>
        /// The amount of time remaining (in seconds) for the current non-credential clear timer.
        /// </summary>
        double OtherTimeRemainingInSeconds
        {
            get;
        }

        /// <summary>
        /// Gets the normalized remaining time [0, 1] for the current timer.
        /// </summary>
        double NormalizedTimeRemaining
        {
            get;
        }

        /// <summary>
        /// Gets the time remaining in seconds for the current timer.
        /// </summary>
        double TimeRemainingInSeconds
        {
            get;
        }

        /// <summary>
        /// Starts the clipboard clear timer.
        /// </summary>
        /// <param name="timerType">The type of clipboard timer being started.</param>
        void StartTimer(ClipboardOperationType timerType);
    }
}
