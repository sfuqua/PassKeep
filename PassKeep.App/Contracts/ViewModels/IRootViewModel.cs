using PassKeep.Lib.Contracts.Enums;
using System;
using System.Threading;
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
        /// Text to display on a loading overlay.
        /// </summary>
        string LoadingText
        {
            get;
        }

        /// <summary>
        /// Whether a load is in progress.
        /// </summary>
        bool IsLoading
        {
            get;
        }

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
        }

        IAppSettingsViewModel AppSettingsViewModel
        {
            get;
        }

        void StartLoad(string loadingText, CancellationTokenSource cts);

        void FinishLoad();

        void CancelCurrentLoad();
    }
}
