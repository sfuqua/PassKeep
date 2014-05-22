using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassKeep.ViewBases
{
    public class DashboardViewBase : PassKeepPage<IDashboardViewModel>
    {
        protected override void navigationHelper_SaveState(object sender, Common.SaveStateEventArgs e)
        {

        }

        protected override void navigationHelper_LoadState(object sender, Common.LoadStateEventArgs e)
        {

        }

        public override bool HandleAcceleratorKey(Windows.System.VirtualKey key)
        {
            return false;
        }
    }
}
