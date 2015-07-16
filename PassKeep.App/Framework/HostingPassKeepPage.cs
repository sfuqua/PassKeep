using PassKeep.Lib.Contracts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Framework
{
    /// <summary>
    /// A PassKeepPage that is capable of hosting other pages - handles wiring up ViewModels for child pages as well as
    /// automatic event handlers, etc.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel.</typeparam>
    public abstract class HostingPassKeepPage<TViewModel> : PassKeepPage<TViewModel>, IHostingPage
        where TViewModel : class, IViewModel
    {
        public abstract Frame ContentFrame
        {
            get;
        }
    }
}
