using PassKeep.Lib.Contracts.Services;

namespace PassKeep.Lib.Contracts.ViewModels
{
    public interface IRootViewModel : IViewModel
    {
        ActivationMode ActivationMode
        {
            get;
        }
    }
}
