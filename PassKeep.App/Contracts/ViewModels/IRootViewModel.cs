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

        IStorageFile CandidateFile
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
            set;
        }
    }
}
