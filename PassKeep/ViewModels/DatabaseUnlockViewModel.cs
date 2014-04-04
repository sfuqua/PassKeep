using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Xml.Linq;
using PassKeep.Common;
using PassKeep.KeePassLib;
using Windows.Storage;
using PassKeep.Controls;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Mvvm;

namespace PassKeep.ViewModels
{
    public sealed class DatabaseUnlockViewModel : ViewModelBase
    {
        private StorageFile _file;
        public StorageFile File
        {
            get { return _file; }
            private set { SetProperty(ref _file, value); }
        }

        private bool? _goodHeader;
        public bool? GoodHeader
        {
            get { return _goodHeader; }
            private set { SetProperty(ref _goodHeader, value); }
        }

        private KeePassError _error;
        public KeePassError Error
        {
            get { return _error; }
            set { SetProperty(ref _error, value); }
        }

        private KdbxReader _reader;
        public KdbxReader Reader
        {
            get { return _reader; }
        }

        public ActionCommand UnlockDatabaseCommand { get; set; }

        public event EventHandler<CancelableEventArgs> StartedUnlock;
        private void onStartedUnlock()
        {
            Action cancelUnlock = () =>
                {
                    _reader.CancelDecrypt();
                };

            if (StartedUnlock != null)
            {
                StartedUnlock(this, new CancelableEventArgs(cancelUnlock));
            }
        }

        public event EventHandler DoneUnlock;
        private void onDoneUnlock()
        {
            if (DoneUnlock != null)
            {
                DoneUnlock(this, new EventArgs());
            }
        }

        private string _password;
        public string Password
        {
            get { return _password ?? String.Empty; }
            set { SetProperty(ref _password, value); }
        }

        private StorageFile _keyfile;
        public StorageFile Keyfile
        {
            get { return _keyfile; }
            set { SetProperty(ref _keyfile, value); }
        }

        public bool IsSample
        {
            get;
            private set;
        }
        public DatabaseUnlockViewModel(ConfigurationViewModel appSettings, StorageFile file, bool isSample = false)
            : base(appSettings)
        {
            Debug.Assert(file != null);
            if (file == null)
            {
                throw new ArgumentNullException("viewModel");
            }

            File = file;
            GoodHeader = null;
            Error = KeePassError.None;

            UnlockDatabaseCommand = new ActionCommand(canUnlock, unlockFile);

            IsSample = isSample;
        }

        private bool canUnlock()
        {
            // Must be able to read the file
            if (_reader == null)
            {
                return false;
            }
            
            // Must have a good header
            if (!GoodHeader.HasValue || !GoodHeader.Value)
            {
                return false;
            }

            return true;
        }

        public async void ValidateFile()
        {
            Debug.Assert(File != null);
            if (File == null)
            {
                throw new InvalidOperationException();
            }

            if (_reader == null)
            {
                _reader = new KdbxReader();
            }

            try
            {
                using (var fileStream = await File.OpenReadAsync())
                {
                    Error = await _reader.ReadHeader(fileStream);
                }
            }
            catch (COMException)
            {
                // Bug in Windows 8.1 preview - opening a stream to a SkyDrive file can fail.
                // There is no workaround at this time.
                Error = new KeePassError(KdbxParseError.UnableToReadFile);
            }

            GoodHeader = (Error == KeePassError.None);
            UnlockDatabaseCommand.RaiseCanExecuteChanged();
        }

        private async void unlockFile()
        {
            Debug.Assert(_reader != null);
            if (_reader == null)
            {
                throw new InvalidOperationException("File has not been validated, please call ValidateFile first");
            }
            Debug.Assert(GoodHeader.HasValue && GoodHeader.Value);
            if (!GoodHeader.HasValue || !GoodHeader.Value)
            {
                throw new InvalidOperationException("File header is bad, please call ValidateFile with a good database first");
            }

            Debug.WriteLine("Attempting to unlock file from the ViewModel...");
            onStartedUnlock();

            try
            {
                using (var fileStream = await File.OpenReadAsync())
                {
                    Error = await _reader.DecryptFile(fileStream, Password, Keyfile);
                }
            }
            catch (COMException)
            {
                // Bug in Windows 8.1 preview - opening a stream to a SkyDrive file can fail.
                // There is no workaround at this time.
                Error = new KeePassError(KdbxParseError.UnableToReadFile);
            }

            onDoneUnlock();
            if (Error == KeePassError.None)
            {
                onDocumentReady(_reader.Document);
            }
        }



        #region Result Management

        public static DatabaseUnlockViewModel FromState(Dictionary<string, object> state)
        {
            Debug.Assert(state != null);
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            Debug.Assert(state.ContainsKey("ViewModel"));
            if (!state.ContainsKey("ViewModel"))
            {
                throw new ArgumentException("state does not contain the ViewModel", "state");
            }
            DatabaseUnlockViewModel viewModel = state["ViewModel"] as DatabaseUnlockViewModel;
            if (viewModel == null)
            {
                throw new ArgumentException("state's File value is not a DatabaseParseViewModel", "state");
            }

            return viewModel;
        }

        #endregion
    }
}
