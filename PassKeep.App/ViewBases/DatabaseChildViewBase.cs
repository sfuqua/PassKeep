using PassKeep.Framework;
using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using System.Threading.Tasks;

namespace PassKeep.ViewBases
{
    /// <summary>
    /// A <see cref="PassKeepPage"/> that serves as the child Page for a <see cref="DatabaseParentViewBase"/>.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel.</typeparam>
    public abstract class DatabaseChildViewBase<TViewModel> : PassKeepPage<TViewModel>, IDatabaseChildView
        where TViewModel : class, IViewModel
    {
        /// <summary>
        /// Provides a means of the parent page requesting a navigate from a clicked breadcrumb to the specified group.
        /// </summary>
        /// <remarks>
        /// This allows the page to preempt the navigate or do necessary cleanup.
        /// </remarks>
        /// <param name="dbViewModel">The DatabaseViewModel to use for the navigation.</param>
        /// <param name="navViewModel">The NavigationViewModel to update.</param>
        /// <param name="clickedGroup">The group to navigate to.</param>
        /// <returns>A Task representing the request.</returns>
        public abstract Task RequestBreadcrumbNavigation(IDatabaseViewModel dbViewModel, IDatabaseNavigationViewModel navViewModel, IKeePassGroup clickedGroup);
    }
}
