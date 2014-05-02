using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.EventArgClasses;
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
        double UserNameTimeRemaining
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
        double PasswordTimeRemaining
        {
            get;
        }

        /// <summary>
        /// Starts the clipboard clear timer.
        /// </summary>
        /// <param name="timerType">The type of clipboard timer being started.</param>
        void StartTimer(ClipboardTimerType timerType);
    }
}
