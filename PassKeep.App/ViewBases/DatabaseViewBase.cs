using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;

namespace PassKeep.ViewBases
{
    public abstract class DatabaseViewBase : DatabaseChildViewBase<IDatabaseViewModel>
    {
        private const string ActiveGroupKey = "ActiveGroup";
    }
}
