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
        /// <summary>
        /// Provides access to the page's strongly typed DataContext.
        /// </summary>
        public TViewModel ViewModel
        {
            get { return this.DataContext as TViewModel; }
            set { this.DataContext = value; }
        }
    }
}
