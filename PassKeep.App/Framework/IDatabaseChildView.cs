// Copyright 2017 Steven Fuqua
// This file is part of PassKeep and is licensed under the GNU GPL v3.
// For the full license, see gpl-3.0.md in this solution or under https://bitbucket.org/sapph/passkeep/src

using PassKeep.Lib.Contracts.Models;
using PassKeep.Lib.Contracts.ViewModels;
using System.Threading.Tasks;

namespace PassKeep.Framework
{
    public interface IDatabaseChildView
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
        Task RequestBreadcrumbNavigation(IDatabaseViewModel dbViewModel, IDatabaseNavigationViewModel navViewModel, IKeePassGroup clickedGroup);
    }
}
