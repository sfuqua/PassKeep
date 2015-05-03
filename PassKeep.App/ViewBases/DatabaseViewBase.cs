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
    }
}
