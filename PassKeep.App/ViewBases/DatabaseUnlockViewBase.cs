using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace PassKeep.ViewBases
{
    public abstract class DatabaseUnlockViewBase : PassKeepPage<IDatabaseUnlockViewModel>
    {
        public override bool HandleAcceleratorKey(VirtualKey key)
        {
            return false;
        }

        public override IList<ICommandBarElement> GetPrimaryCommandBarElements()
        {
            return null;
        }

        public override IList<ICommandBarElement> GetSecondaryCommandBarElements()
        {
            return null;
        }
    }
}
