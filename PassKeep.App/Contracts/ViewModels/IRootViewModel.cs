using PassKeep.Lib.Contracts.Enums;
using Windows.Storage;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IRootViewModel : IViewModel
    {
        ActivationMode ActivationMode
        {
            get;
        }

        StorageFile OpenedFile
        {
            get;
            set;
        }
    }
}
