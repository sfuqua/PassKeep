using PassKeep.Lib.Contracts.Enums;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Mvvm;
using Windows.Storage;

namespace PassKeep.Lib.ViewModels
{
    /// <summary>
    /// Serves as a ViewModel for the root container of the app.
    /// </summary>
    public class RootViewModel : BindableBase, IRootViewModel
    {
        private IStorageFile _openedFile;
        private IDatabaseParentViewModel _decryptedDatabase;
        private IPasswordGenViewModel _passwordGenViewModel;

        public RootViewModel(
            ActivationMode activationMode,
            IStorageFile openedFile
        )
        {
            this.ActivationMode = activationMode;
            this.CandidateFile = openedFile;
        }

        public ActivationMode ActivationMode
        {
            get;
            private set;
        }

        public IStorageFile CandidateFile
        {
            get { return this._openedFile;  }
            set { SetProperty(ref this._openedFile, value); }
        }

        public IDatabaseParentViewModel DecryptedDatabase
        {
            get { return this._decryptedDatabase; }
            set { SetProperty(ref this._decryptedDatabase, value); }
        }

        public IPasswordGenViewModel PasswordGenViewModel
        {
            get { return this._passwordGenViewModel; }
            set { SetProperty(ref this._passwordGenViewModel, value); }
        }
    }
}
