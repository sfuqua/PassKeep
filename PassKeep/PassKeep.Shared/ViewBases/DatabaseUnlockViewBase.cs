using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using Windows.System;

namespace PassKeep.ViewBases
{
    public abstract class DatabaseUnlockViewBase : PassKeepPage<IDatabaseUnlockViewModel>
    {
        public override bool HandleAcceleratorKey(VirtualKey key)
        {
            return false;
        }
    }
}
