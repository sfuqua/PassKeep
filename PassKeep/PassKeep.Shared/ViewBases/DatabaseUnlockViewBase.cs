﻿using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using Windows.System;

namespace PassKeep.ViewBases
{
    public class DatabaseUnlockViewBase : PassKeepPage<IDatabaseUnlockViewModel>
    {
        public override bool HandleAcceleratorKey(VirtualKey key)
        {
            return false;
        }

        protected override void navigationHelper_LoadState(object sender, Common.LoadStateEventArgs e)
        {
        }

        protected override void navigationHelper_SaveState(object sender, Common.SaveStateEventArgs e)
        {
        }
    }
}
