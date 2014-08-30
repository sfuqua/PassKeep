using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace PassKeep.ViewBases
{
    public abstract class SearchResultsViewBase : PassKeepPage<ISearchViewModel>
    {
        public override bool HandleAcceleratorKey(VirtualKey key)
        {
            return false;
        }

        public override IList<ICommandBarElement> GetPrimaryCommandBarElements()
        {
            return null;
        }

        protected override void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }
    }
}
