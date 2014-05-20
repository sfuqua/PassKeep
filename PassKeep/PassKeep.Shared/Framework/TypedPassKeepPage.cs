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
    public class PassKeepPage<TViewModel> : PassKeepPage
        where TViewModel : IViewModel
    {
    }
}
