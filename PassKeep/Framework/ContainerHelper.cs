using Microsoft.Practices.Unity;
using PassKeep.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PassKeep.Framework
{
    /// <summary>
    /// A class that provides abstracted DI container helper methods.
    /// </summary>
    public class ContainerHelper
    {
        private IUnityContainer _container;

        public ContainerHelper(IUnityContainer container)
        {
            this._container = container;
        }

        /// <summary>
        /// Uses the container to resolve a ViewModel, and navigates to the specified
        /// View type with the ViewModel as an argument.
        /// </summary>
        /// <typeparam name="TView">The type of the View to navigate to</typeparam>
        /// <typeparam name="TViewModel">The type of the View's ViewModel</typeparam>
        /// <param name="navFrame">The Frame to navigate</param>
        /// <param name="overrides">ResolverOverride objects to pass to the container</param>
        /// <returns>Whether navigation was successful</returns>
        public bool ResolveAndNavigate<TView, TViewModel>(Frame navFrame, params ResolverOverride[] overrides)
            where TView : PassKeepPage<TViewModel>
        {
            TViewModel viewModel = this._container.Resolve<TViewModel>(
                overrides
            );
            return navFrame.Navigate(typeof(TView), viewModel);
        }
    }
}
