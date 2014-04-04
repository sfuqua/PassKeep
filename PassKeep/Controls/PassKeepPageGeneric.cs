using PassKeep.Common;
using PassKeep.Lib.Contracts.ViewModels;

namespace PassKeep.Controls
{
    /// <summary>
    /// Represents a page of the app that is responsible for its own ViewModel,
    /// of a known type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel for this View</typeparam>
    public abstract class PassKeepPage<TViewModel> : PassKeepPage
        where TViewModel : class, IViewModel
    {
        /// <summary>
        /// Provides access to the ViewModel for this View
        /// </summary>
        public TViewModel ViewModel
        {
            get;
            protected set;
        }

        /// <summary>
        /// Loads the ViewModel from page state if it exists, otherwise tries to 
        /// cast one from the NavigationParameter.
        /// </summary>
        /// <remarks>Called by the View's NavigationHelper</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void navHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            bool gotVm = false;
            if (e.PageState != null)
            {
                if (e.PageState.ContainsKey("ViewModel"))
                {
                    this.ViewModel = e.PageState["ViewModel"] as TViewModel;
                    if (this.ViewModel == null)
                    {
                        throw new ViewModelException("Unable to cast ViewModel PageState to desired ViewModel type");
                    }

                    gotVm = true;
                }
            }

            if (!gotVm)
            {
                if (e.NavigationParameter == null)
                {
                    throw new ViewModelException("No NavigationParameter was specified");
                }

                this.ViewModel = e.NavigationParameter as TViewModel;
                if (this.ViewModel == null)
                {
                    throw new ViewModelException("Unable to cast NavigationParameter to desired ViewModel type");
                }
            }

            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Persists the ViewModel to the page state and closes appbars.
        /// </summary>
        /// <remarks>Called by the View's NavigationHelper</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void navHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["ViewModel"] = this.ViewModel;
            BottomAppBar.IsSticky = false;
            BottomAppBar.IsOpen = false;
        }
    }
}
