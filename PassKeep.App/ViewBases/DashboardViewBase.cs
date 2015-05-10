using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;

namespace PassKeep.ViewBases
{
    public abstract class DashboardViewBase : PassKeepPage<IDashboardViewModel>
    {
        protected DashboardViewBase() : base()
        { }
    }
}
