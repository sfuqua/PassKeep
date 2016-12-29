using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.Services;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Files;
using System;
using Windows.Foundation;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IRootViewModel : IViewModel
    {
        /// <summary>
        /// Fired when the automated clipboard clear timer failed to clear the clipboard, in order to notify the view.
        /// </summary>
        event EventHandler ClipboardClearFailed;

        /// <summary>
        /// Fired when the view should allow choosing a location to export a file.
        /// </summary>
        event TypedEventHandler<IRootViewModel, FileRequestedEventArgs> ExportingCachedFile;

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
