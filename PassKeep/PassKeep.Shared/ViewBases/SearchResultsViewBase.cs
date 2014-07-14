using PassKeep.Common;
using PassKeep.Framework;
using PassKeep.Lib.Contracts.ViewModels;

namespace PassKeep.ViewBases
{
    public abstract class SearchResultsViewBase : PassKeepPage<ISearchViewModel>
    {
        public override bool HandleAcceleratorKey(Windows.System.VirtualKey key)
        {
            return false;
        }

        protected override void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }
    }
}
