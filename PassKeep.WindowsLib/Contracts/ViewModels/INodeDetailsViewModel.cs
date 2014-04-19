﻿using PassKeep.Lib.Contracts.Models;
using System.Threading.Tasks;

namespace PassKeep.Lib.Contracts.ViewModels
{
    /// <summary>
    /// A ViewModel representing a detailed view of a Node (e.g., an entry or group).
    /// </summary>
    /// <typeparam name="T">The type of Node.</typeparam>
    public interface INodeDetailsViewModel<T> : IDatabasePersistenceViewModel
        where T : IKeePassNode
    {
        /// <summary>
        /// A ViewModel used for tracking breadcrumbs to the current node.
        /// </summary>
        IDatabaseNavigationViewModel NavigationViewModel
        {
            get;
        }

        /// <summary>
        /// Whether or not editing is enabled for the View.
        /// </summary>
        bool IsReadOnly
        {
            get;
            set;
        }

        /// <summary>
        /// Whether or not this node is new to the document (and so cannot be reverted).
        /// </summary>
        bool IsNew
        {
            get;
        }

        /// <summary>
        /// The Node being viewed in detail.
        /// </summary>
        T WorkingCopy
        {
            get;
        }

        /// <summary>
        /// Reverts any pending changes to the underlying document.
        /// </summary>
        void Revert();
    }
}
