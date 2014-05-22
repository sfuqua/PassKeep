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
        public RootViewModel(
            ActivationMode activationMode,
            StorageFile openedFile
        )
        {
            this.ActivationMode = activationMode;
            this.OpenedFile = openedFile;
        }

        public ActivationMode ActivationMode
        {
            get;
            private set;
        }

        public StorageFile OpenedFile
        {
            get;
            private set;
        }
    }
}
