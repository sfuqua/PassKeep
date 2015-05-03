using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace PassKeep.ViewBases
{
    public abstract class DashboardViewBase : PassKeepPage<IDashboardViewModel>
    {
        protected DashboardViewBase(bool primaryAvailable = true, bool secondaryAvailable = true)
            : base(primaryAvailable, secondaryAvailable)
        { }

        public override bool HandleAcceleratorKey(VirtualKey key)
        {
            return false;
        }

        public override IList<ICommandBarElement> GetSecondaryCommandBarElements()
        {
            return null;
        }
    }
}
