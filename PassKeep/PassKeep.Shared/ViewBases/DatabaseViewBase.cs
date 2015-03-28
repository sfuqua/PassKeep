using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using SariphLib.Infrastructure;
using Windows.System;

namespace PassKeep.ViewBases
{
    public abstract class DatabaseViewBase : PassKeepPage<IDatabaseViewModel>
    {
        private const string ActiveGroupKey = "ActiveGroup";

        public override bool HandleAcceleratorKey(VirtualKey key)
        {
            return false;
        }

        protected override void navigationHelper_LoadState(object sender, Common.LoadStateEventArgs e)
        {
            if (e.PageState != null && e.PageState.ContainsKey(DatabaseViewBase.ActiveGroupKey))
            {
                IKeePassGroup activeGroup = e.PageState[DatabaseViewBase.ActiveGroupKey] as IKeePassGroup;
                Dbg.Assert(activeGroup != null);

                this.ViewModel.NavigationViewModel.SetGroup(activeGroup);
            }
        }

        protected override void navigationHelper_SaveState(object sender, Common.SaveStateEventArgs e)
        {
            e.PageState[DatabaseViewBase.ActiveGroupKey] =
                this.ViewModel.NavigationViewModel.ActiveGroup;
        }
    }
}
