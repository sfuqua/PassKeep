using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;

namespace PassKeep.Lib.Contracts.Providers
{
    public interface IDatabaseNodeViewModelFactory
    {
        IDatabaseNodeViewModel Assemble(IKeePassNode node);
    }
}
