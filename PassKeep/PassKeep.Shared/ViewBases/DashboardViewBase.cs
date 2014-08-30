using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace PassKeep.ViewBases
{
    public abstract class DashboardViewBase : PassKeepPage<IDashboardViewModel>
    {
        public override bool HandleAcceleratorKey(VirtualKey key)
        {
            return false;
        }

        public override IList<ICommandBarElement> GetSecondaryCommandBarElements()
        {
            return null;
        }

        protected override void navigationHelper_LoadState(object sender, Common.LoadStateEventArgs e)
        {
        }

        protected override void navigationHelper_SaveState(object sender, Common.SaveStateEventArgs e)
        {
        }
    }
}
