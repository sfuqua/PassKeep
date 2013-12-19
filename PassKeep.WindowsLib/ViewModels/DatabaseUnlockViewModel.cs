using PassKeep.Lib.Contracts.KeePass;
using PassKeep.Lib.Contracts.ViewModels;
using PassKeep.Lib.EventArgClasses;
using SariphLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace PassKeep.Lib.ViewModels
{
    public sealed class DatabaseUnlockViewModel : BindableBase, IDatabaseUnlockViewModel
    {
        private StorageFile _candidateFile;
        public StorageFile CandidateFile
        {
            get
            {
                return _candidateFile;
            }
            set
            {
                if (_candidateFile != null && SetProperty(ref _candidateFile, value))
                {

                }
            }
        }

        private string _password;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                SetProperty(ref _password, value);
            }
        }

        private StorageFile _keyFile;
        public StorageFile KeyFile
        {
            get
            {
                return _keyFile;
            }
            set
            {
                SetProperty(ref _keyFile, value);
            }
        }

        private ActionCommand _unlockCommand;
        public ActionCommand UnlockCommand
        {
            get
            {
                return _unlockCommand;
            }
            private set
            {
                SetProperty(ref _unlockCommand, value);
            }
        }

        public event EventHandler<CancelableEventArgs> StartedUnlocking;
        private void raiseStartedUnlocking(IKdbxReader reader)
        {
            if (StartedUnlocking != null)
            {
                CancelableEventArgs eventArgs = new CancelableEventArgs(reader.Cancel);
                StartedUnlocking(this, eventArgs);
            }
        }


        public event EventHandler StoppedUnlocking;
        private void raiseStoppedUnlocking()
        {
            if (StoppedUnlocking != null)
            {
                StoppedUnlocking(this, new EventArgs());
            }
        }


        public event EventHandler<DocumentReadyEventArgs> DocumentReady;
        private void raiseDocumentReady(XDocument document, IRandomNumberGenerator rng)
        {
            if (DocumentReady != null)
            {
                DocumentReady(this, new DocumentReadyEventArgs(document, rng));
            }
        }

        public bool HasGoodHeader
        {
            get { throw new NotImplementedException(); }
        }
    }
}
