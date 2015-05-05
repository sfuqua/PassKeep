using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;

namespace PassKeep.ViewBases
{
    public abstract class DatabaseViewBase : PassKeepPage<IDatabaseViewModel>
    {
        private const string ActiveGroupKey = "ActiveGroup";
    }
}
