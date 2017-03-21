using PassKeep.Lib.Contracts.Providers;
using PassKeep.Lib.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassKeep.Framework
{
    /// <summary>
    /// A strongly typed PassKeepPage that is aware of its own ViewModel.
    /// </summary>
    /// <typeparam name="TViewModel">The type of ViewModel used by this page.</typeparam>
    public abstract class PassKeepPage<TViewModel> : PassKeepPage
        where TViewModel : class, IViewModel
    {
        protected PassKeepPage()
            : base()
        { }

        /// <summary>
        /// Provides access to the page's strongly typed DataContext.
        /// </summary>
        public TViewModel ViewModel
        {
            get { return DataContext as TViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        /// Saves state when app is being suspended.
        /// </summary>
        public override void HandleSuspend()
        {
            if (ViewModel != null)
            {
                ViewModel.HandleAppSuspend();
            }
        }

        /// <summary>
        /// Restores state when app is resumed.
        /// </summary>
        public override void HandleResume()
        {
            if (ViewModel != null)
            {
                ViewModel.HandleAppResume();
            }
        }
    }
}
